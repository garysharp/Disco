using Disco.Client.Interop;
using Disco.Models.ClientServices;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

namespace Disco.Client.Extensions
{
    public static class EnrolExtensions
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

                string odjFile = Path.GetTempFileName();
                File.WriteAllBytes(odjFile, Convert.FromBase64String(enrolResponse.OfflineDomainJoinManifest));

                string odjWindowsPath = Environment.GetEnvironmentVariable("SystemRoot");
                string odjProcessArguments = $"/REQUESTODJ /LOADFILE \"{odjFile}\" /WINDOWSPATH \"{odjWindowsPath}\" /LOCALOS";

                ProcessStartInfo odjProcessStartInfo = new ProcessStartInfo("DJOIN.EXE", odjProcessArguments)
                {
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    LoadUserProfile = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                string odjResult;
                using (Process odjProcess = System.Diagnostics.Process.Start(odjProcessStartInfo))
                {
                    odjResult = odjProcess.StandardOutput.ReadToEnd();
                    odjProcess.WaitForExit(20000); // 20 Seconds
                }
                Presentation.UpdateStatus("Enrolling Device", $"Offline Domain Join Result:\r\n{odjResult}", true, -1, 3000);

                if (File.Exists(odjFile))
                    File.Delete(odjFile);

                // Flush Logged-On History
                if (!string.IsNullOrEmpty(enrolResponse.DomainName))
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
