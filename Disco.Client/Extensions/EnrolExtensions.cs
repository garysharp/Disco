using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.ClientServices;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace Disco.Client.Extensions
{
    public static class EnrolExtensions
    {

        public static void Build(this Enrol enrol)
        {
            enrol.DeviceUUID = Interop.SystemAudit.DeviceUUID;
            enrol.DeviceSerialNumber = Interop.SystemAudit.DeviceSerialNumber;

            enrol.DeviceComputerName = Interop.LocalAuthentication.ComputerName;

            enrol.DeviceManufacturer = Interop.SystemAudit.DeviceManufacturer;
            enrol.DeviceModel = Interop.SystemAudit.DeviceModel;
            enrol.DeviceModelType = Interop.SystemAudit.DeviceType;

            enrol.DeviceIsPartOfDomain = Interop.SystemAudit.DeviceIsPartOfDomain;

            // LAN
            enrol.DeviceLanMacAddress = Interop.Network.PrimaryLanMacAddress;

            // WAN
            enrol.DeviceWlanMacAddress = Interop.Network.PrimaryWlanMacAddress;

            // Certificates
            enrol.DeviceCertificates = Interop.Certificates.GetCertificateSubjects(StoreName.My, StoreLocation.LocalMachine);
        }

        public static void Process(this EnrolResponse enrolResponse)
        {
            if (enrolResponse == null)
                throw new ClientServiceException("Enrolment", "Server denied enrolment (Empty Response)");

            ErrorReporting.EnrolmentSessionId = enrolResponse.SessionId;
            
            if (!string.IsNullOrEmpty(enrolResponse.ErrorMessage))
                throw new ClientServiceException("Enrolment", enrolResponse.ErrorMessage);

            // Offline Domain Join
            bool requireReboot = enrolResponse.ApplyOfflineDomainJoin();

            // Certificates
            enrolResponse.ApplyDeviceCertificates();

            // Device Owner
            enrolResponse.ApplyDeviceAssignedUser();


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
            if (!string.IsNullOrWhiteSpace(enrolResponse.OfflineDomainJoin))
            {
                Presentation.UpdateStatus("Enrolling Device", string.Format("Performing Offline Domain Join:{0}Renaming Computer: {1} -> {2}", Environment.NewLine, Interop.LocalAuthentication.ComputerName, enrolResponse.DeviceComputerName), true, -1, 1500);

                string odjFile = Path.GetTempFileName();
                File.WriteAllBytes(odjFile, Convert.FromBase64String(enrolResponse.OfflineDomainJoin));

                string odjWindowsPath = Environment.GetEnvironmentVariable("SystemRoot");
                string odjProcessArguments = string.Format("/REQUESTODJ /LOADFILE \"{0}\" /WINDOWSPATH \"{1}\" /LOCALOS", odjFile, odjWindowsPath);

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
                Presentation.UpdateStatus("Enrolling Device", string.Format("Offline Domain Join Result:{0}{1}", Environment.NewLine, odjResult), true, -1, 3000);

                if (File.Exists(odjFile))
                    File.Delete(odjFile);

                // Flush Logged-On History
                if (!string.IsNullOrEmpty(enrolResponse.DeviceDomainName))
                {
                    using (RegistryKey regWinlogon  = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", true)){
                        regWinlogon.SetValue("DefaultDomainName", enrolResponse.DeviceDomainName, RegistryValueKind.String);
                        regWinlogon.SetValue("DefaultUserName", String.Empty, RegistryValueKind.String);
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
            if (!string.IsNullOrWhiteSpace(enrolResponse.DeviceAssignedUserSID))
            {
                Presentation.UpdateStatus("Enrolling Device", string.Format(@"Configuring permissions for the device owner:{0}{1} ({2}\{3})", Environment.NewLine, enrolResponse.DeviceAssignedUserName, enrolResponse.DeviceAssignedUserDomain, enrolResponse.DeviceAssignedUserUsername), true, -1, 3000);

                Interop.LocalAuthentication.AddLocalGroupMembership("Administrators", enrolResponse.DeviceAssignedUserSID, enrolResponse.DeviceAssignedUserUsername, enrolResponse.DeviceAssignedUserDomain);

                // Make Windows think this user was the last to logon
                using (RegistryKey regWinlogon = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", true))
                {
                    regWinlogon.SetValue("DefaultDomainName", enrolResponse.DeviceAssignedUserDomain, RegistryValueKind.String);
                    regWinlogon.SetValue("DefaultUserName", enrolResponse.DeviceAssignedUserUsername, RegistryValueKind.String);
                }
                using (RegistryKey regLogonUI = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\LogonUI", true))
                {
                    regLogonUI.SetValue("LastLoggedOnUser", string.Format(@"{0}\{1}", enrolResponse.DeviceAssignedUserDomain, enrolResponse.DeviceAssignedUserUsername), RegistryValueKind.String);
                }
            }
        }

        /// <summary>
        /// Processes a Client Service Enrol Response for Device Certificate Actions
        /// </summary>
        /// <param name="enrolResponse"></param>
        private static void ApplyDeviceCertificates(this EnrolResponse enrolResponse)
        {
            // Only run if a Certificate was supplied
            if (!string.IsNullOrEmpty(enrolResponse.DeviceCertificate))
            {
                Presentation.UpdateStatus("Enrolling Device", "Configuring Wireless Certificates", true, -1, 1000);

                var certPersonalBytes = Convert.FromBase64String(enrolResponse.DeviceCertificate);
                var certPersonal = new X509Certificate2(certPersonalBytes, "password", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
                if (certPersonal == null)
                    throw new ClientServiceException("Enrolment > Device Certificate", "Unable to Import Device Certificate Provided, possibly check password.");

                // Certificate Removal
                if (enrolResponse.DeviceCertificateRemoveExisting != null && enrolResponse.DeviceCertificateRemoveExisting.Count > 0)
                {
                    List<Regex> regExMatchesSubject = new List<Regex>();
                    foreach (var subjectRegEx in enrolResponse.DeviceCertificateRemoveExisting)
                        regExMatchesSubject.Add(new Regex(subjectRegEx, RegexOptions.IgnoreCase));

                    // Remove from 'My' Store
                    Interop.Certificates.RemoveCertificates(StoreName.My, StoreLocation.LocalMachine, regExMatchesSubject, certPersonal);
                    // Remove from 'Root' Store
                    Interop.Certificates.RemoveCertificates(StoreName.Root, StoreLocation.LocalMachine, regExMatchesSubject, certPersonal);
                    // Remove from 'CertificateAuthority' Store
                    Interop.Certificates.RemoveCertificates(StoreName.CertificateAuthority, StoreLocation.LocalMachine, regExMatchesSubject, certPersonal);
                }

                // Add Certificate
                Presentation.UpdateStatus("Enrolling Device", string.Format("Configuring Wireless Certificates{0}Installing Certificate: {1}", Environment.NewLine, Interop.Certificates.GetCertificateFriendlyName(certPersonal)), true, -1);
                Interop.Certificates.AddCertificate(StoreName.My, StoreLocation.LocalMachine, certPersonal);
            }
        }
        
    }
}
