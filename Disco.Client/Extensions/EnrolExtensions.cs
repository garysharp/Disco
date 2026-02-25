using Disco.Client.Interop;
using Disco.Models.ClientServices;
using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;

namespace Disco.Client.Extensions
{
    internal static class EnrolExtensions
    {
        public static void Build(this Enrol enrol)
        {
            enrol.ComputerName = Environment.MachineName;
            enrol.RunningUserDomain = Environment.UserDomainName;
            enrol.RunningUserName = Environment.UserName;
            enrol.RunningInteractively = Environment.UserInteractive;

            // Hardware Audit
            enrol.Hardware = Hardware.Information;
            enrol.SerialNumber = enrol.Hardware.SerialNumber;

            // Apply System Information
            enrol.ApplySystemInformation();

            // Certificates
            enrol.Certificates = Certificates.GetAllCertificates();

            // Wireless Profiles
            enrol.WirelessProfiles = WirelessNetwork.GetWirelessProfiles();
        }

        public static void Process(this EnrolResponse enrolResponse)
        {
            if (enrolResponse == null)
                throw new ClientServiceException("Enrolment", "Server denied enrolment (Empty Response)");

            ErrorReporting.EnrolmentSessionId = enrolResponse.SessionId;

            if (!string.IsNullOrEmpty(enrolResponse.ErrorMessage))
                throw new ClientServiceException("Enrolment", enrolResponse.ErrorMessage);

            if (enrolResponse.IsPending)
                return;

            // Offline Domain Join
            bool requireReboot = enrolResponse.ApplyOfflineDomainJoin();

            // Device Owner
            enrolResponse.ApplyDeviceAssignedUser();

            // Certificates
            enrolResponse.ApplyDeviceCertificates();

            // Wireless Profiles
            enrolResponse.ApplyWirelessProfiles();

            Presentation.UpdateStatus("Enrolling Device", "Device Enrolment Successfully Completed", false, 0, 1500);

            Program.RebootRequired = requireReboot;
            Program.AllowUninstall = enrolResponse.AllowBootstrapperUninstall;
        }

        [Flags]
        private enum NETSETUP_PROVISION_FLAGS : int
        {
            NETSETUP_PROVISION_DOWNLEVEL_PRIV_SUPPORT = 0x00000001,
            NETSETUP_PROVISION_REUSE_ACCOUNT = 0x00000002,
            NETSETUP_PROVISION_USE_DEFAULT_PASSWORD = 0x00000004,
            NETSETUP_PROVISION_SKIP_ACCOUNT_SEARCH = 0x00000008,
            NETSETUP_PROVISION_ROOT_CA_CERTS = 0x00000010,
            NETSETUP_PROVISION_PERSISTENTSITE = 0x00000020,
            NETSETUP_PROVISION_ONLINE_CALLER = 0x40000000,
            NETSETUP_PROVISION_CHECK_PWD_ONLY = unchecked((int)0x80000000),
        }

        [DllImport("Netapi32.dll", CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.I4)]
        private static extern int NetRequestOfflineDomainJoin(
            [In] IntPtr pProvisionBinData,
            [In, MarshalAs(UnmanagedType.I4)] int cbProvisionBinDataSize,
            [In, MarshalAs(UnmanagedType.I4)] NETSETUP_PROVISION_FLAGS dwOptions,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpWindowsPath
            );

        /// <summary>
        /// Processes a Client Service Enrol Response for Offline Domain Join Actions
        /// </summary>
        /// <param name="enrolResponse"></param>
        /// <returns>Boolean indicating whether a reboot is required.</returns>
        private static bool ApplyOfflineDomainJoin(this EnrolResponse enrolResponse)
        {
            if (!string.IsNullOrWhiteSpace(enrolResponse.OfflineDomainJoinManifest))
            {
                Presentation.UpdateStatus("Enrolling Device", $"Performing Offline Domain Join:\r\nRenaming Computer: {Environment.MachineName} -> {enrolResponse.ComputerName}", true, -1, 1500);

                var provisionData = Convert.FromBase64String(enrolResponse.OfflineDomainJoinManifest);
                string systemRoot = Environment.GetEnvironmentVariable("SystemRoot");

                var provisionDataPointer = Marshal.AllocCoTaskMem(provisionData.Length);
                Marshal.Copy(provisionData, 0, provisionDataPointer, provisionData.Length);
                var joinResult = default(int);
                try
                {
                    joinResult = NetRequestOfflineDomainJoin(provisionDataPointer, provisionData.Length, NETSETUP_PROVISION_FLAGS.NETSETUP_PROVISION_ONLINE_CALLER, systemRoot);
                }
                finally
                {
                    Marshal.FreeCoTaskMem(provisionDataPointer);
                }

                if (joinResult != 0)
                {
                    var win32Exception = new System.ComponentModel.Win32Exception(joinResult);
                    Presentation.UpdateStatus("Enrolling Device", $"Offline Domain Join Failed:\r\n{win32Exception.Message} [{joinResult}]", true, -1, 3000);
                    throw new InvalidOperationException($"Offline Domain Join Failed:\r\n{win32Exception.Message} [{joinResult}]");
                }
                else
                {
                    Presentation.UpdateStatus("Enrolling Device", $"Offline Domain Join Succeeded", true, -1, 2000);
                }

                // Flush Logged-On History
                if (enrolResponse.SetAssignedUserForLogon && !string.IsNullOrEmpty(enrolResponse.DomainName))
                {
                    using (RegistryKey regWinlogon = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", true))
                    {
                        regWinlogon.SetValue("DefaultDomainName", enrolResponse.DomainName, RegistryValueKind.String);
                        regWinlogon.SetValue("DefaultUserName", string.Empty, RegistryValueKind.String);
                    }
                    using (RegistryKey regLogonUI = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\LogonUI", true))
                    {
                        regLogonUI.DeleteValue("LastLoggedOnUser", false);
                    }
                }

                return true; // Reboot required
            }
            else
            {
                // No Domain Join
                return false; // Reboot not required
            }
        }

        /// <summary>
        /// Processes a Client Service Enrol Response for Device Assigned User Actions
        /// </summary>
        /// <param name="enrolResponse"></param>
        private static void ApplyDeviceAssignedUser(this EnrolResponse enrolResponse)
        {
            // Only run task if Assigned User was specified
            if (!string.IsNullOrWhiteSpace(enrolResponse.AssignedUserSID))
            {
                Presentation.UpdateStatus("Enrolling Device", $"Configuring the device owner:\r\n{enrolResponse.AssignedUserDescription} ({enrolResponse.AssignedUserDomain}\\{enrolResponse.AssignedUserUsername})", true, -1, 3000);

                if (enrolResponse.AssignedUserIsLocalAdmin)
                    LocalAuthentication.AddLocalGroupMembership("Administrators", enrolResponse.AssignedUserSID, enrolResponse.AssignedUserUsername, enrolResponse.AssignedUserDomain);
            }

            if (enrolResponse.SetAssignedUserForLogon && !string.IsNullOrEmpty(enrolResponse.AssignedUserDomain) && !string.IsNullOrEmpty(enrolResponse.AssignedUserUsername))
            {
                // Make Windows think this user was the last to logon
                using (RegistryKey regWinlogon = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", true))
                {
                    regWinlogon.SetValue("DefaultDomainName", enrolResponse.AssignedUserDomain, RegistryValueKind.String);
                    regWinlogon.SetValue("DefaultUserName", enrolResponse.AssignedUserUsername, RegistryValueKind.String);
                }
                using (RegistryKey regLogonUI = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\LogonUI", true))
                {
                    regLogonUI.SetValue("LastLoggedOnUser", $@"{enrolResponse.AssignedUserDomain}\{enrolResponse.AssignedUserUsername}", RegistryValueKind.String);
                }
            }
        }

        private static void ApplyDeviceCertificates(this EnrolResponse enrolResponse)
        {
            if (enrolResponse.Certificates != null)
            {
                Presentation.UpdateStatus("Enrolling Device", "Configuring Certificates", true, -1, 1000);

                enrolResponse.Certificates.Apply();
            }
        }

        private static void ApplyWirelessProfiles(this EnrolResponse enrolResponse)
        {
            if (enrolResponse.WirelessProfiles != null)
            {
                Presentation.UpdateStatus("Enrolling Device", "Configuring Wireless Profiles", true, -1, 1000);

                enrolResponse.WirelessProfiles.Apply();
            }
        }
    }
}
