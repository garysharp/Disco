using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Disco.ClientBootstrapper.Interop
{
    public static class InstallInterop
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, MoveFileFlags dwFlags);
        [Flags]
        enum MoveFileFlags
        {
            MOVEFILE_REPLACE_EXISTING = 0x00000001,
            MOVEFILE_COPY_ALLOWED = 0x00000002,
            MOVEFILE_DELAY_UNTIL_REBOOT = 0x00000004,
            MOVEFILE_WRITE_THROUGH = 0x00000008,
            MOVEFILE_CREATE_HARDLINK = 0x00000010,
            MOVEFILE_FAIL_IF_NOT_TRACKABLE = 0x00000020
        }

        private static void Install(string RootFilesystemLocation, RegistryKey RootRegistryLocation, string FilesystemInstallLocation, string VirtualRootFilesystemLocation)
        {
            var SourceLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var InstallLocation = Path.Combine(RootFilesystemLocation, FilesystemInstallLocation);
            var BootstrapperCmdLinePath = Path.Combine(VirtualRootFilesystemLocation, FilesystemInstallLocation, "Disco.ClientBootstrapper.exe");

            var GroupPolicyScriptsIniLocation = Path.Combine(RootFilesystemLocation, "Windows\\System32\\GroupPolicy\\Machine\\Scripts\\scripts.ini");
            var GroupPolicyScriptsIniBackupLocation = Path.Combine(RootFilesystemLocation, "Windows\\System32\\GroupPolicy\\Machine\\Scripts\\disco_scripts.ini");

            // Create file system Location
            #region "Create File System Location"
            Program.Status.UpdateStatus(null, null, "Creating Installation Location");
            Program.SleepThread(500, false);
            if (Directory.Exists(InstallLocation))
            {
                // Try and Delete Directory
                try
                {
                    Directory.Delete(InstallLocation, true);
                }
                catch (Exception ex)
                {
                    throw new IOException(string.Format("Unable to delete folder: ", InstallLocation), ex);
                }
            }
            if (!Directory.Exists(InstallLocation))
            {
                var installDir = Directory.CreateDirectory(InstallLocation);
                installDir.Attributes = installDir.Attributes | FileAttributes.Hidden;
            }
            #endregion

            // Copy files to file system location
            #region "Copy to File System"
            Program.Status.UpdateStatus(null, null, "Copying Files");
            Program.SleepThread(500, false);

            // Copy Bootstrapper
            // ie: Executing Assembly
            File.Copy(System.Reflection.Assembly.GetExecutingAssembly().Location, Path.Combine(InstallLocation, "Disco.ClientBootstrapper.exe"));

            foreach (var file in Directory.EnumerateFiles(SourceLocation))
            {
                var fileName = Path.GetFileName(file);
                
                // Only Copy Certain Files

                // Copy Wireless Certificates
                if (fileName.StartsWith("WLAN_Cert_Root_", StringComparison.OrdinalIgnoreCase) ||
                    fileName.StartsWith("WLAN_Cert_Intermediate_", StringComparison.OrdinalIgnoreCase) ||
                    fileName.StartsWith("WLAN_Cert_Personal_", StringComparison.OrdinalIgnoreCase))
                    File.Copy(file, Path.Combine(InstallLocation, fileName));

                // Copy Wireless Profiles
                if (fileName.StartsWith("WLAN_Profile_", StringComparison.OrdinalIgnoreCase) &&
                    fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    File.Copy(file, Path.Combine(InstallLocation, fileName));
                
            }
            #endregion

            // Backup & Create Group Policy Scripts.ini
            #region "Group Policy Scripts.ini"
            Program.Status.UpdateStatus(null, null, "Creating Group Policy Script Entry");
            Program.SleepThread(500, false);
            // Backup
            if (!File.Exists(GroupPolicyScriptsIniBackupLocation))
            {
                if (File.Exists(GroupPolicyScriptsIniLocation))
                {
                    File.Move(GroupPolicyScriptsIniLocation, GroupPolicyScriptsIniBackupLocation);
                }
            }

            // Create
            if (File.Exists(GroupPolicyScriptsIniLocation))
                File.Delete(GroupPolicyScriptsIniLocation);
            if (!Directory.Exists(Path.GetDirectoryName(GroupPolicyScriptsIniLocation)))
                Directory.CreateDirectory(Path.GetDirectoryName(GroupPolicyScriptsIniLocation));
            using (var scriptsIniStream = File.Open(GroupPolicyScriptsIniLocation, FileMode.Create, FileAccess.Write))
            {
                using (var scriptsIniStreamWriter = new StreamWriter(scriptsIniStream, Encoding.Unicode))
                {
                    scriptsIniStreamWriter.Write(string.Format("[Startup]{0}0CmdLine={1}{0}0Parameters=/AllowUninstall", Environment.NewLine, BootstrapperCmdLinePath));
                    scriptsIniStreamWriter.Flush();
                }
            }
            #endregion

            // Backup & Create Group Policy Registry
            #region "Group Policy Registry"
            Program.Status.UpdateStatus(null, null, "Creating Group Policy Registry Entries");
            Program.SleepThread(500, false);
            // Backup Scripts
            using (var regGroupPolicy = RootRegistryLocation.OpenSubKey("Microsoft\\Windows\\CurrentVersion\\Group Policy", true))
            {
                if (regGroupPolicy != null && regGroupPolicy.GetSubKeyNames().Contains("Scripts") && !regGroupPolicy.GetSubKeyNames().Contains("Disco_Scripts"))
                {
                    RegistryUtilities.RenameSubKey(regGroupPolicy, "Scripts", "Disco_Scripts");
                }
            }

            // Create Scripts
            RootRegistryLocation.CreateSubKey("Microsoft\\Windows\\CurrentVersion\\Group Policy\\Scripts\\Shutdown").Dispose();
            using (var regScriptsStartup = RootRegistryLocation.CreateSubKey("Microsoft\\Windows\\CurrentVersion\\Group Policy\\Scripts\\Startup\\0"))
            {
                regScriptsStartup.SetValue("GPO-ID", "LocalGPO", RegistryValueKind.String);
                regScriptsStartup.SetValue("SOM-ID", "Local", RegistryValueKind.String);
                regScriptsStartup.SetValue("FileSysPath", Path.Combine(Environment.SystemDirectory, "GroupPolicy\\Machine"), RegistryValueKind.String);
                regScriptsStartup.SetValue("DisplayName", "Local Group Policy", RegistryValueKind.String);
                regScriptsStartup.SetValue("GPOName", "Local Group Policy", RegistryValueKind.String);
                regScriptsStartup.SetValue("PSScriptOrder", 1, RegistryValueKind.DWord);
                using (var regScriptsStartup0 = regScriptsStartup.CreateSubKey("0"))
                {
                    regScriptsStartup0.SetValue("Script", BootstrapperCmdLinePath, RegistryValueKind.String);
                    regScriptsStartup0.SetValue("Parameters", "/AllowUninstall", RegistryValueKind.String);
                    regScriptsStartup0.SetValue("IsPowershell", 0, RegistryValueKind.DWord);
                    regScriptsStartup0.SetValue("ExecTime", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, RegistryValueKind.Binary);
                }
            }
            RootRegistryLocation.CreateSubKey("Microsoft\\Windows\\CurrentVersion\\Group Policy\\State\\Machine\\Scripts\\Shutdown").Dispose();

            // Backup Scripts State
            using (var regGroupPolicy = RootRegistryLocation.OpenSubKey("Microsoft\\Windows\\CurrentVersion\\Group Policy\\State\\Machine", true))
            {
                if (regGroupPolicy != null && regGroupPolicy.GetSubKeyNames().Contains("Scripts") && !regGroupPolicy.GetSubKeyNames().Contains("Disco_Scripts"))
                {
                    RegistryUtilities.RenameSubKey(regGroupPolicy, "Scripts", "Disco_Scripts");
                }
            }

            // Create Scripts State
            using (var regStateScriptsStartup = RootRegistryLocation.CreateSubKey("Microsoft\\Windows\\CurrentVersion\\Group Policy\\State\\Machine\\Scripts\\Startup\\0"))
            {
                regStateScriptsStartup.SetValue("GPO-ID", "LocalGPO", RegistryValueKind.String);
                regStateScriptsStartup.SetValue("SOM-ID", "Local", RegistryValueKind.String);
                regStateScriptsStartup.SetValue("FileSysPath", Path.Combine(Environment.SystemDirectory, "GroupPolicy\\Machine"), RegistryValueKind.String);
                regStateScriptsStartup.SetValue("DisplayName", "Local Group Policy", RegistryValueKind.String);
                regStateScriptsStartup.SetValue("GPOName", "Local Group Policy", RegistryValueKind.String);
                regStateScriptsStartup.SetValue("PSScriptOrder", 1, RegistryValueKind.DWord);
                using (var regStateScriptsStartup0 = regStateScriptsStartup.CreateSubKey("0"))
                {
                    regStateScriptsStartup0.SetValue("Script", BootstrapperCmdLinePath, RegistryValueKind.String);
                    regStateScriptsStartup0.SetValue("Parameters", "/AllowUninstall", RegistryValueKind.String);
                    regStateScriptsStartup0.SetValue("ExecTime", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, RegistryValueKind.Binary);
                }
            }
            #endregion

            // Set Registry Startup Environment Policies
            #region "Registry Startup Policies"
            Program.Status.UpdateStatus(null, null, "Creating Startup Policy Registry Entries");
            Program.SleepThread(500, false);
            using (var regWinlogon = RootRegistryLocation.OpenSubKey("Microsoft\\Windows NT\\CurrentVersion\\Winlogon", true))
            {
                regWinlogon.SetValue("HideStartupScripts", 0, RegistryValueKind.DWord);
                regWinlogon.SetValue("RunStartupScriptSync", 1, RegistryValueKind.DWord);
            }
            #endregion
        }

        public static void Install(string InstallLocation, string WimImageId, string TempPath)
        {
            Program.Status.UpdateStatus("Installing Bootstrapper", "Starting", "Please wait...", false);

            if (string.IsNullOrWhiteSpace(InstallLocation))
                InstallLocation = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "Disco");

            if (InstallLocation.EndsWith(".wim", StringComparison.OrdinalIgnoreCase))
            {
                // Offline File System (WIM)
                Program.Status.UpdateStatus("Installing Bootstrapper (Offline)", "Installing", string.Format("Install Location: {0}", InstallLocation));
                Program.SleepThread(1000, false);

                // Mount WIM
                int wimImageIndex = 0;
                using (var wim = new WIMInterop.WindowsImageContainer(InstallLocation, WIMInterop.WindowsImageContainer.CreateFileMode.OpenExisting, WIMInterop.WindowsImageContainer.CreateFileAccess.Write))
                {
                    if (WimImageId == null)
                        WimImageId = "1";
                    if (!int.TryParse(WimImageId, out wimImageIndex))
                    {
                        Program.Status.UpdateStatus(null, "Analysing WIM", string.Format("Looking for Image Name: {0}", WimImageId));
                        Program.SleepThread(500, false);
                        for (int i = 0; i < wim.ImageCount; i++)
                        {
                            var wimImageInfo = new System.Xml.XmlDocument();
                            using (var wimImage = wim[i])
                                wimImageInfo.LoadXml(wimImage.ImageInformation);
                            var wimImageInfoName = wimImageInfo.SelectSingleNode("//IMAGE/NAME");
                            if (wimImageInfoName != null && wimImageInfoName.InnerText.Equals(WimImageId, StringComparison.OrdinalIgnoreCase))
                            {
                                wimImageIndex = i + 1;
                                Program.Status.UpdateStatus(null, "Analysing WIM", string.Format("Found Image Id '{0}' at Index {1}", WimImageId, wimImageIndex));
                                Program.SleepThread(500, false);
                                break;
                            }
                        }
                    }
                }
                if (wimImageIndex == 0)
                {
                    Program.Status.UpdateStatus(null, "Error", string.Format("Unable to load WIM Image Id: {0}", WimImageId));
                    Program.SleepThread(5000, false);
                    return;
                }

                // Get Temp Path
                var wimMountPath = Path.Combine(TempPath ?? Path.GetTempPath(), "DiscoClientBootstrapperWimMount");
                if (Directory.Exists(wimMountPath))
                    Directory.Delete(wimMountPath, true);
                Directory.CreateDirectory(wimMountPath);

                var wimTempMountPath = Path.Combine(TempPath ?? Path.GetTempPath(), "DiscoClientBootstrapperWimTempMount");
                if (Directory.Exists(wimTempMountPath))
                    Directory.Delete(wimTempMountPath, true);
                Directory.CreateDirectory(wimTempMountPath);

                bool wimCommitChanges = true;
                WIMInterop.WindowsImageContainer.NativeMethods.MessageCallback m_MessageCallback = null;
                try
                {
                    // Mount WIM
                    Program.Status.UpdateStatus(null, "Mounting WIM", string.Format("Mounting WIM Image to '{0}'", wimMountPath));
                    Program.SleepThread(500, false);
                    m_MessageCallback = new WIMInterop.WindowsImageContainer.NativeMethods.MessageCallback(WimImageEventMessagePump);
                    Interop.WIMInterop.WindowsImageContainer.NativeMethods.RegisterCallback(m_MessageCallback);

                    Interop.WIMInterop.WindowsImageContainer.NativeMethods.MountImage(wimMountPath, InstallLocation, wimImageIndex, wimTempMountPath);

                    // Load Local Machine Registry
                    var wimHivePath = Path.Combine(wimMountPath, "Windows\\System32\\config\\SOFTWARE");
                    Program.Status.UpdateStatus(null, "Mounting Offline Registry Hive", string.Format("Mounting Offline Registry Hive at '{0}'", wimHivePath));
                    Program.SleepThread(500, false);
                    using (var wimReg = new RegistryInterop(RegistryInterop.RegistryHives.HKEY_LOCAL_MACHINE, "DiscoClientBootstrapperWimHive", wimHivePath))
                    {
                        using (RegistryKey rootRegistryLocation = Registry.LocalMachine.OpenSubKey("DiscoClientBootstrapperWimHive", true))
                        {
                            string rootFileSystemLocation = wimMountPath;
                            string fileSystemInstallLocation = "Disco";
                            string virtualRootFileSystemLocation = "C:\\";

                            Install(rootFileSystemLocation, rootRegistryLocation, fileSystemInstallLocation, virtualRootFileSystemLocation);
                        }

                        // Unload Local Machine Registry
                        Program.Status.UpdateStatus(null, "Unmounting Offline Registry Hive", string.Format("Unmounting Offline Registry Hive at '{0}'", wimHivePath));
                        Program.SleepThread(500, false);
                        wimReg.Unload();
                    }
                }
                catch (Exception)
                {
                    wimCommitChanges = false;
                    throw;
                }
                finally
                {
                    // Unmount WIM
                    Program.Status.UpdateStatus(null, "Unmounting WIM", string.Format("Unmounting WIM Image at '{0}'", wimMountPath));
                    Program.SleepThread(500, false);
                    Interop.WIMInterop.WindowsImageContainer.NativeMethods.DismountImage(wimMountPath, InstallLocation, wimImageIndex, wimCommitChanges);

                    if (m_MessageCallback != null)
                    {
                        Interop.WIMInterop.WindowsImageContainer.NativeMethods.UnregisterMessageCallback(m_MessageCallback);
                        m_MessageCallback = null;
                    }

                    if (Directory.Exists(wimMountPath))
                        Directory.Delete(wimMountPath, true);
                    if (Directory.Exists(wimTempMountPath))
                        Directory.Delete(wimTempMountPath, true);
                }
            }
            else
            {
                // Online File System
                Program.Status.UpdateStatus("Installing Bootstrapper (Online)", "Installing", string.Format("Install Location: {0}", InstallLocation), true, -1);
                Program.SleepThread(1000, false);
                string rootFileSystemLocation = Path.GetPathRoot(InstallLocation);
                RegistryKey rootRegistryLocation = Registry.LocalMachine.OpenSubKey("SOFTWARE", true);
                string fileSystemInstallLocation = InstallLocation.Substring(rootFileSystemLocation.Length);

                Install(rootFileSystemLocation, rootRegistryLocation, fileSystemInstallLocation, rootFileSystemLocation);
                Program.Status.UpdateStatus(null, "Online File System Installation Complete", string.Empty, true, -1);
                Program.SleepThread(1000, false);
            }
            Program.Status.UpdateStatus(null, "Complete", "Finished Installing Bootstrapper");
            Program.SleepThread(1500, false);
        }

        private static uint WimImageEventMessagePump(
            uint MessageId,
            IntPtr wParam,
            IntPtr lParam,
            IntPtr UserData
        )
        {
            uint status = (uint)Interop.WIMInterop.WindowsImageContainer.NativeMethods.WIMMessage.WIM_MSG_SUCCESS;
            WIMInterop.DefaultImageEventArgs eventArgs = new WIMInterop.DefaultImageEventArgs(wParam, lParam, UserData);

            //System.Diagnostics.Debug.WriteLine(MessageId);

            switch ((WIMInterop.WindowsImageContainer.ImageEventMessage)MessageId)
            {

                case Interop.WIMInterop.WindowsImageContainer.ImageEventMessage.Progress:
                case Interop.WIMInterop.WindowsImageContainer.ImageEventMessage.MountCleanupProgress:
                    var timeRemainingMil = eventArgs.LeftParameter.ToInt32();
                    string timeRemainingMessage;
                    if (timeRemainingMil > 0)
                        timeRemainingMessage = TimeSpan.FromMilliseconds(timeRemainingMil).ToString(@"hh\:mm\:ss");
                    else
                        timeRemainingMessage = "Calculating, please wait...";

                    var progress = eventArgs.WideParameter.ToInt32();
                    Program.Status.UpdateStatus(null, null, string.Format("Time remaining: {0}", timeRemainingMessage), true, progress);
                    
                    break;
                default:
                    break;
            }

            return status;
        }

        public static void Uninstall()
        {
            // Application Directory
            var appDirectory = Program.InlinePath.Value;
            if (Program.AllowUninstall && !appDirectory.StartsWith("\\\\"))
            {
                Program.Status.UpdateStatus("System Preparation (Bootstrapper)", "Uninstalling Bootstrapper...", string.Empty, false, 0);
                Program.SleepThread(1000, true);
                //var uninstallScriptLocation = System.IO.Path.Combine(appDirectory, "UninstallBootstrapper.vbs");
                //if (System.IO.File.Exists(uninstallScriptLocation))
                //{
                //    var bootstrapperPID = System.Diagnostics.Process.GetCurrentProcess().Id;
                //    var cscriptPath = System.IO.Path.Combine(Environment.SystemDirectory, "cscript.exe");
                //    var cscriptArgs = string.Format("\"{0}\" /WaitForProcessID:{1}", uninstallScriptLocation, bootstrapperPID);

                //    var startProc = new ProcessStartInfo(cscriptPath, cscriptArgs);
                //    startProc.WorkingDirectory = Environment.SystemDirectory;
                //    startProc.WindowStyle = ProcessWindowStyle.Hidden;

                //    Process.Start(startProc);
                //}

                // Remove Registry Entries
                using (var regWinlogon = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon", true))
                {
                    regWinlogon.DeleteValue("HideStartupScripts", false);
                    regWinlogon.DeleteValue("RunStartupScriptSync", false);
                }
                Registry.LocalMachine.DeleteSubKeyTree("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Group Policy\\Scripts\\Shutdown", false);
                Registry.LocalMachine.DeleteSubKeyTree("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Group Policy\\Scripts\\Startup", false);
                Registry.LocalMachine.DeleteSubKeyTree("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Group Policy\\State\\Machine\\Scripts\\Shutdown", false);
                Registry.LocalMachine.DeleteSubKeyTree("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Group Policy\\State\\Machine\\Scripts\\Startup", false);

                // Restore Registry Backups
                using (var regGroupPolicy = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Group Policy", true))
                {
                    if (regGroupPolicy != null && regGroupPolicy.GetSubKeyNames().Contains("Disco_Scripts"))
                    {
                        regGroupPolicy.DeleteSubKeyTree("Scripts");
                        RegistryUtilities.RenameSubKey(regGroupPolicy, "Disco_Scripts", "Scripts");
                    }
                }
                using (var regGroupPolicy = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Group Policy\\State\\Machine", true))
                {
                    if (regGroupPolicy != null && regGroupPolicy.GetSubKeyNames().Contains("Disco_Scripts"))
                    {
                        regGroupPolicy.DeleteSubKeyTree("Scripts");
                        RegistryUtilities.RenameSubKey(regGroupPolicy, "Disco_Scripts", "Scripts");
                    }
                }

                // Delete Group Policy Script File
                var groupPolicyScriptsPath = Path.Combine(Environment.SystemDirectory, "GroupPolicy\\Machine\\Scripts\\scripts.ini");
                if (File.Exists(groupPolicyScriptsPath))
                    File.Delete(groupPolicyScriptsPath);
                var groupPolicyScriptsBackupPath = Path.Combine(Environment.SystemDirectory, "GroupPolicy\\Machine\\Scripts\\disco_scripts.ini");
                if (File.Exists(groupPolicyScriptsBackupPath))
                    File.Move(groupPolicyScriptsBackupPath, groupPolicyScriptsPath);

                // Queue Folder for Deletion at next startup
                ForceDeleteFolder(new DirectoryInfo(appDirectory));
            }
        }

        private static void ForceDeleteFolder(DirectoryInfo d)
        {
            foreach (var sd in d.GetDirectories())
                ForceDeleteFolder(sd);
            foreach (var f in d.GetFiles())
            {
                try
                {
                    File.Delete(f.FullName);
                }
                catch (Exception)
                {
                    MoveFileEx(f.FullName, null, MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);
                }
            }

            try
            {
                Directory.Delete(d.FullName);
            }
            catch (Exception)
            {
                MoveFileEx(d.FullName, null, MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);
            }
        }
    }

    public static class RegistryUtilities
    {
        /// <summary>
        /// Renames a subkey of the passed in registry key since 
        /// the Framework totally forgot to include such a handy feature.
        /// </summary>
        /// <param name="regKey">The RegistryKey that contains the subkey 
        /// you want to rename (must be writeable)</param>
        /// <param name="subKeyName">The name of the subkey that you want to rename
        /// </param>
        /// <param name="newSubKeyName">The new name of the RegistryKey</param>
        /// <returns>True if succeeds</returns>
        public static bool RenameSubKey(RegistryKey parentKey,
            string subKeyName, string newSubKeyName)
        {
            CopyKey(parentKey, subKeyName, newSubKeyName);
            parentKey.DeleteSubKeyTree(subKeyName);
            return true;
        }

        /// <summary>
        /// Copy a registry key.  The parentKey must be writeable.
        /// </summary>
        /// <param name="parentKey"></param>
        /// <param name="keyNameToCopy"></param>
        /// <param name="newKeyName"></param>
        /// <returns></returns>
        public static bool CopyKey(RegistryKey parentKey,
            string keyNameToCopy, string newKeyName)
        {
            //Create new key
            using (RegistryKey destinationKey = parentKey.CreateSubKey(newKeyName))
            {
                //Open the sourceKey we are copying from
                using (RegistryKey sourceKey = parentKey.OpenSubKey(keyNameToCopy))
                {
                    RecurseCopyKey(sourceKey, destinationKey);
                }
            }

            return true;
        }

        private static void RecurseCopyKey(RegistryKey sourceKey, RegistryKey destinationKey)
        {
            //copy all the values
            foreach (string valueName in sourceKey.GetValueNames())
            {
                object objValue = sourceKey.GetValue(valueName);
                RegistryValueKind valKind = sourceKey.GetValueKind(valueName);

                if (valueName == "ExecTime")
                {
                    destinationKey.SetValue(valueName, objValue, RegistryValueKind.Binary);
                }
                else
                {
                    destinationKey.SetValue(valueName, objValue, valKind);
                }
            }

            //For Each subKey 
            //Create a new subKey in destinationKey 
            //Call myself 
            foreach (string sourceSubKeyName in sourceKey.GetSubKeyNames())
            {
                using (RegistryKey sourceSubKey = sourceKey.OpenSubKey(sourceSubKeyName))
                {
                    using (RegistryKey destSubKey = destinationKey.CreateSubKey(sourceSubKeyName))
                    {
                        RecurseCopyKey(sourceSubKey, destSubKey);
                    }
                }
            }
        }
    }


}
