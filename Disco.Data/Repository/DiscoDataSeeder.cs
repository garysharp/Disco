using Disco.Models.Repository;
using System;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;

namespace Disco.Data.Repository
{
    public static class DiscoDataSeeder
    {
        public static void SeedDatabase(this DiscoDataContext Database)
        {
            Database.SeedDeploymentId();
            Database.SeedDeviceModels();
            Database.SeedDeviceProfiles();
            Database.SeedJobTypes();
            Database.SeedJobSubTypes();

            Database.SaveChanges();

            // Migration Maintenance
            Database.MigratePreDomainObjects();
        }

        public static void SeedDeploymentId(this DiscoDataContext Database)
        {
            if (Database.ConfigurationItems.Count(ci => ci.Scope == "System" && ci.Key == "DeploymentId") == 0)
            {
                var deploymentId = Guid.NewGuid().ToString("D");
                Database.ConfigurationItems.Add(new ConfigurationItem { Scope = "System", Key = "DeploymentId", Value = deploymentId });
            }
            if (Database.ConfigurationItems.Count(ci => ci.Scope == "System" && ci.Key == "DeploymentSecret") == 0)
            {
                var deploymentId = Guid.NewGuid().ToString("N");
                Database.ConfigurationItems.Add(new ConfigurationItem { Scope = "System", Key = "DeploymentSecret", Value = deploymentId });
            }
        }
        public static void SeedJobTypes(this DiscoDataContext Database)
        {
            if (Database.JobTypes.Count() == 0)
            {
                Database.JobTypes.Add(new JobType { Id = JobType.JobTypeIds.HWar, Description = "Hardware - Warranty" });
                Database.JobTypes.Add(new JobType { Id = JobType.JobTypeIds.HNWar, Description = "Hardware - Non-Warranty" });
                Database.JobTypes.Add(new JobType { Id = JobType.JobTypeIds.HMisc, Description = "Hardware - Misc" });
                Database.JobTypes.Add(new JobType { Id = JobType.JobTypeIds.SImg, Description = "Software - Reimage" });
                Database.JobTypes.Add(new JobType { Id = JobType.JobTypeIds.SApp, Description = "Software - Application" });
                Database.JobTypes.Add(new JobType { Id = JobType.JobTypeIds.SOS, Description = "Software - Operating System" });
            }
            // 2012-05-22
            #region "User Management" Added
            if (Database.JobTypes.Count(jt => jt.Id == JobType.JobTypeIds.UMgmt) == 0)
                Database.JobTypes.Add(new JobType { Id = JobType.JobTypeIds.UMgmt, Description = "User - Management" });
            #endregion
            // End
        }
        public static void SeedDeviceModels(this DiscoDataContext Database)
        {
            if (Database.DeviceModels.Count() == 0)
            {
                Database.DeviceModels.Add(new DeviceModel { Manufacturer = "Unknown", Model = "Unknown", Description = "Unknown Device Model", ModelType = "Unknown" });
            }
            UpdateDeviceModelConfiguration(Database);
            // Removed: 2013-01-14 G#
            //UpdateDeviceModelImageStorage(context);

            // Added: 2013-02-07 G#
            UpdateDeviceModelDuplicates(Database);
        }
        public static void SeedDeviceProfiles(this DiscoDataContext Database)
        {
            if (Database.DeviceProfiles.Count() == 0)
            {
                Database.DeviceProfiles.Add(
                    new DeviceProfile
                    {
                        ShortName = "WS",
                        Name = "Default",
                        Description = "Initial Default Workstation Profile",
                        ComputerNameTemplate = DeviceProfile.DefaultComputerNameTemplate,
                        DistributionType = DeviceProfile.DistributionTypes.OneToMany
                    });
            }
            else
            {
                // Bug Fix - correct invalid Computer Name Templates
                var invalidProfiles = Database.DeviceProfiles.Where(dp => dp.ComputerNameTemplate == "DeviceProfile.ShortName + ''-'' + SerialNumber");
                foreach (var p in invalidProfiles)
                    p.ComputerNameTemplate = DeviceProfile.DefaultComputerNameTemplate;
            }
        }
        public static void SeedJobSubTypes(this DiscoDataContext Database)
        {
            if (Database.JobSubTypes.Count() == 0)
            {
                Database.JobSubTypes.Add(new JobSubType { Id = "Bag", JobTypeId = JobType.JobTypeIds.HWar, Description = "Bag" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Battery", JobTypeId = JobType.JobTypeIds.HWar, Description = "Battery" });
                Database.JobSubTypes.Add(new JobSubType { Id = "BezelCaseBottom", JobTypeId = JobType.JobTypeIds.HWar, Description = "Bezel - Case Bottom" });
                Database.JobSubTypes.Add(new JobSubType { Id = "BezelCaseTop", JobTypeId = JobType.JobTypeIds.HWar, Description = "Bezel - Case Top" });
                Database.JobSubTypes.Add(new JobSubType { Id = "BezelScreenInner", JobTypeId = JobType.JobTypeIds.HWar, Description = "Bezel - Screen Inner" });
                Database.JobSubTypes.Add(new JobSubType { Id = "BezelScreenTop", JobTypeId = JobType.JobTypeIds.HWar, Description = "Bezel - Screen Top" });
                Database.JobSubTypes.Add(new JobSubType { Id = "BluetoothAdapter", JobTypeId = JobType.JobTypeIds.HWar, Description = "Bluetooth Adapter" });
                Database.JobSubTypes.Add(new JobSubType { Id = "CPU", JobTypeId = JobType.JobTypeIds.HWar, Description = "CPU" });
                Database.JobSubTypes.Add(new JobSubType { Id = "HardDrive", JobTypeId = JobType.JobTypeIds.HWar, Description = "Hard Drive" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Keyboard", JobTypeId = JobType.JobTypeIds.HWar, Description = "Keyboard" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Motherboard", JobTypeId = JobType.JobTypeIds.HWar, Description = "Motherboard" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Mouse", JobTypeId = JobType.JobTypeIds.HWar, Description = "Mouse/Track Pad" });
                Database.JobSubTypes.Add(new JobSubType { Id = "PowerAdapter", JobTypeId = JobType.JobTypeIds.HWar, Description = "Power Adapter" });
                Database.JobSubTypes.Add(new JobSubType { Id = "PowerCord", JobTypeId = JobType.JobTypeIds.HWar, Description = "Power Cord/Socket" });
                Database.JobSubTypes.Add(new JobSubType { Id = "RAM", JobTypeId = JobType.JobTypeIds.HWar, Description = "RAM" });
                Database.JobSubTypes.Add(new JobSubType { Id = "ReplacementDevice", JobTypeId = JobType.JobTypeIds.HWar, Description = "Replacement Device" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Screen", JobTypeId = JobType.JobTypeIds.HWar, Description = "Screen" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Speakers", JobTypeId = JobType.JobTypeIds.HWar, Description = "Speakers" });
                Database.JobSubTypes.Add(new JobSubType { Id = "WebCamera", JobTypeId = JobType.JobTypeIds.HWar, Description = "Web Camera" });
                Database.JobSubTypes.Add(new JobSubType { Id = "WirelessAdapter", JobTypeId = JobType.JobTypeIds.HWar, Description = "Wireless Adapter" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Other", JobTypeId = JobType.JobTypeIds.HWar, Description = "Other" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Bag", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Bag" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Battery", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Battery" });
                Database.JobSubTypes.Add(new JobSubType { Id = "BezelCaseBottom", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Bezel - Case Bottom" });
                Database.JobSubTypes.Add(new JobSubType { Id = "BezelCaseTop", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Bezel - Case Top" });
                Database.JobSubTypes.Add(new JobSubType { Id = "BezelScreenInner", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Bezel - Screen Inner" });
                Database.JobSubTypes.Add(new JobSubType { Id = "BezelScreenTop", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Bezel - Screen Top" });
                Database.JobSubTypes.Add(new JobSubType { Id = "BluetoothAdapter", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Bluetooth Adapter" });
                Database.JobSubTypes.Add(new JobSubType { Id = "CPU", JobTypeId = JobType.JobTypeIds.HNWar, Description = "CPU" });
                Database.JobSubTypes.Add(new JobSubType { Id = "HardDrive", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Hard Drive" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Keyboard", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Keyboard" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Motherboard", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Motherboard" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Mouse", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Mouse/Track Pad" });
                Database.JobSubTypes.Add(new JobSubType { Id = "PowerAdapter", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Power Adapter" });
                Database.JobSubTypes.Add(new JobSubType { Id = "PowerCord", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Power Cord/Socket" });
                Database.JobSubTypes.Add(new JobSubType { Id = "RAM", JobTypeId = JobType.JobTypeIds.HNWar, Description = "RAM" });
                Database.JobSubTypes.Add(new JobSubType { Id = "ReplacementDevice", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Replacement Device" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Screen", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Screen" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Speakers", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Speakers" });
                Database.JobSubTypes.Add(new JobSubType { Id = "WebCamera", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Web Camera" });
                Database.JobSubTypes.Add(new JobSubType { Id = "WirelessAdapter", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Wireless Adapter" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Other", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Other" });
                Database.JobSubTypes.Add(new JobSubType { Id = "ExternalHardDrive", JobTypeId = JobType.JobTypeIds.HMisc, Description = "External Hard Drive" });
                Database.JobSubTypes.Add(new JobSubType { Id = "IWB", JobTypeId = JobType.JobTypeIds.HMisc, Description = "Interactive Whiteboard" });
                Database.JobSubTypes.Add(new JobSubType { Id = "InternetDongle", JobTypeId = JobType.JobTypeIds.HMisc, Description = "Internet Dongle" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Keyboard", JobTypeId = JobType.JobTypeIds.HMisc, Description = "External Keyboard" });
                Database.JobSubTypes.Add(new JobSubType { Id = "MobilePhone", JobTypeId = JobType.JobTypeIds.HMisc, Description = "Mobile Phone" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Mouse", JobTypeId = JobType.JobTypeIds.HMisc, Description = "External Mouse" });
                Database.JobSubTypes.Add(new JobSubType { Id = "MP3Player", JobTypeId = JobType.JobTypeIds.HMisc, Description = "MP3 Player" });
                Database.JobSubTypes.Add(new JobSubType { Id = "PrinterScanner", JobTypeId = JobType.JobTypeIds.HMisc, Description = "Printer/Scanner" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Projector", JobTypeId = JobType.JobTypeIds.HMisc, Description = "Projector" });
                Database.JobSubTypes.Add(new JobSubType { Id = "USBFlashDrive", JobTypeId = JobType.JobTypeIds.HMisc, Description = "USB Flash Drive" });
                Database.JobSubTypes.Add(new JobSubType { Id = "WebCamera", JobTypeId = JobType.JobTypeIds.HMisc, Description = "Web Camera" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Other", JobTypeId = JobType.JobTypeIds.HMisc, Description = "Other" });
                Database.JobSubTypes.Add(new JobSubType { Id = "ContentIllegal", JobTypeId = JobType.JobTypeIds.SImg, Description = "Content - Illegal" });
                Database.JobSubTypes.Add(new JobSubType { Id = "ContentInappropriate", JobTypeId = JobType.JobTypeIds.SImg, Description = "Content - Inappropriate" });
                Database.JobSubTypes.Add(new JobSubType { Id = "CorruptOS", JobTypeId = JobType.JobTypeIds.SImg, Description = "Corrupt Operating System" });
                Database.JobSubTypes.Add(new JobSubType { Id = "HardwareChanges", JobTypeId = JobType.JobTypeIds.SImg, Description = "Hardware Changes" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Malware", JobTypeId = JobType.JobTypeIds.SImg, Description = "Malware" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Performance", JobTypeId = JobType.JobTypeIds.SImg, Description = "Performance" });
                Database.JobSubTypes.Add(new JobSubType { Id = "UserRequest", JobTypeId = JobType.JobTypeIds.SImg, Description = "User Request" });
                Database.JobSubTypes.Add(new JobSubType { Id = "UpdatedImage", JobTypeId = JobType.JobTypeIds.SImg, Description = "Updated Image" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Other", JobTypeId = JobType.JobTypeIds.SImg, Description = "Other" });
                Database.JobSubTypes.Add(new JobSubType { Id = "CurriculumTool", JobTypeId = JobType.JobTypeIds.SApp, Description = "Curriculum Tool" });
                Database.JobSubTypes.Add(new JobSubType { Id = "GamingEntertainment", JobTypeId = JobType.JobTypeIds.SApp, Description = "Gaming/Entertainment" });
                Database.JobSubTypes.Add(new JobSubType { Id = "ImageManipulation", JobTypeId = JobType.JobTypeIds.SApp, Description = "Image Manipulation" });
                Database.JobSubTypes.Add(new JobSubType { Id = "MultimediaPlayback", JobTypeId = JobType.JobTypeIds.SApp, Description = "Multimedia Playback" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Presentation", JobTypeId = JobType.JobTypeIds.SApp, Description = "Presentation" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Spreadsheet", JobTypeId = JobType.JobTypeIds.SApp, Description = "Spreadsheet" });
                Database.JobSubTypes.Add(new JobSubType { Id = "StaffTool", JobTypeId = JobType.JobTypeIds.SApp, Description = "Staff Tool" });
                Database.JobSubTypes.Add(new JobSubType { Id = "StudentReporting", JobTypeId = JobType.JobTypeIds.SApp, Description = "Student Reporting" });
                Database.JobSubTypes.Add(new JobSubType { Id = "VideoEditing", JobTypeId = JobType.JobTypeIds.SApp, Description = "Video Editing" });
                Database.JobSubTypes.Add(new JobSubType { Id = "WebBrowser", JobTypeId = JobType.JobTypeIds.SApp, Description = "Web Browser" });
                Database.JobSubTypes.Add(new JobSubType { Id = "WebBrowserPlugin", JobTypeId = JobType.JobTypeIds.SApp, Description = "Web Browser Plugin" });
                Database.JobSubTypes.Add(new JobSubType { Id = "WordProcessing", JobTypeId = JobType.JobTypeIds.SApp, Description = "Word Processing" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Other", JobTypeId = JobType.JobTypeIds.SApp, Description = "Other" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Appearance", JobTypeId = JobType.JobTypeIds.SOS, Description = "Appearance & Personalisation" });
                Database.JobSubTypes.Add(new JobSubType { Id = "DisplaySettings", JobTypeId = JobType.JobTypeIds.SOS, Description = "Display Settings" });
                Database.JobSubTypes.Add(new JobSubType { Id = "DriversIWB", JobTypeId = JobType.JobTypeIds.SOS, Description = "Drivers - Interactive Whiteboard" });
                Database.JobSubTypes.Add(new JobSubType { Id = "DriversPrintScan", JobTypeId = JobType.JobTypeIds.SOS, Description = "Drivers - Printers/Scanners" });
                Database.JobSubTypes.Add(new JobSubType { Id = "DriversSystem", JobTypeId = JobType.JobTypeIds.SOS, Description = "Drivers - System" });
                Database.JobSubTypes.Add(new JobSubType { Id = "DriversOther", JobTypeId = JobType.JobTypeIds.SOS, Description = "Drivers - Other" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Group Policy", JobTypeId = JobType.JobTypeIds.SOS, Description = "Group Policy" });
                Database.JobSubTypes.Add(new JobSubType { Id = "KeyboardMouseConfig", JobTypeId = JobType.JobTypeIds.SOS, Description = "Keyboard/Mouse Configuration" });
                Database.JobSubTypes.Add(new JobSubType { Id = "MalwareProtection", JobTypeId = JobType.JobTypeIds.SOS, Description = "Malware Protection" });
                Database.JobSubTypes.Add(new JobSubType { Id = "NetworkDrives", JobTypeId = JobType.JobTypeIds.SOS, Description = "Network Drives" });
                Database.JobSubTypes.Add(new JobSubType { Id = "NetworkWired", JobTypeId = JobType.JobTypeIds.SOS, Description = "Network - Wired" });
                Database.JobSubTypes.Add(new JobSubType { Id = "NetworkWireless", JobTypeId = JobType.JobTypeIds.SOS, Description = "Network - Wireless" });
                Database.JobSubTypes.Add(new JobSubType { Id = "NetworkOther", JobTypeId = JobType.JobTypeIds.SOS, Description = "Network - Other" });
                Database.JobSubTypes.Add(new JobSubType { Id = "PatchUpdate", JobTypeId = JobType.JobTypeIds.SOS, Description = "Patches/Updates" });
                Database.JobSubTypes.Add(new JobSubType { Id = "PowerManagement", JobTypeId = JobType.JobTypeIds.SOS, Description = "Power Management" });
                Database.JobSubTypes.Add(new JobSubType { Id = "PrintersScanners", JobTypeId = JobType.JobTypeIds.SOS, Description = "Printers/Scanners" });
                Database.JobSubTypes.Add(new JobSubType { Id = "SoundConfig", JobTypeId = JobType.JobTypeIds.SOS, Description = "Sound Configuration" });
                Database.JobSubTypes.Add(new JobSubType { Id = "Other", JobTypeId = JobType.JobTypeIds.SOS, Description = "Other" });
            }

            // Feature Request 2012-04-27 by Elijah: https://disco.uservoice.com/forums/159707-feedback/suggestions/2803945-customisable-job-sub-types
            #region "Optical Drive" Added
            if (Database.JobSubTypes.Count(jst => jst.JobTypeId == JobType.JobTypeIds.HNWar && jst.Id == "OpticalDrive") == 0)
                Database.JobSubTypes.Add(new JobSubType { Id = "OpticalDrive", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Optical Drive" });
            if (Database.JobSubTypes.Count(jst => jst.JobTypeId == JobType.JobTypeIds.HWar && jst.Id == "OpticalDrive") == 0)
                Database.JobSubTypes.Add(new JobSubType { Id = "OpticalDrive", JobTypeId = JobType.JobTypeIds.HWar, Description = "Optical Drive" });
            #endregion
            // End Feature Request

            // 2012-05-22
            #region "User Management" Added
            if (Database.JobSubTypes.Count(jst => jst.JobTypeId == JobType.JobTypeIds.UMgmt) == 0)
            {
                Database.JobSubTypes.Add(new JobSubType { Id = JobSubType.UserManagementJobSubTypes.Infringement, JobTypeId = JobType.JobTypeIds.UMgmt, Description = JobSubType.UserManagementJobSubTypes.Infringement });
                Database.JobSubTypes.Add(new JobSubType { Id = JobSubType.UserManagementJobSubTypes.Contact, JobTypeId = JobType.JobTypeIds.UMgmt, Description = JobSubType.UserManagementJobSubTypes.Contact });
            }
            #endregion
            // End

            // 2025-07-11
            #region "User Management - BYOD" Added
            if (Database.JobSubTypes.Count(jst => jst.JobTypeId == JobType.JobTypeIds.UMgmt && jst.Id == JobSubType.UserManagementJobSubTypes.BYOD) == 0)
            {
                Database.JobSubTypes.Add(new JobSubType { Id = JobSubType.UserManagementJobSubTypes.BYOD, JobTypeId = JobType.JobTypeIds.UMgmt, Description = JobSubType.UserManagementJobSubTypes.BYOD });
            }
            #endregion
            // End

            // 2012-05-29 - Audits
            #region "Audit" Added
            if (Database.JobSubTypes.Count(jst => jst.JobTypeId == JobType.JobTypeIds.HMisc && jst.Id == "Audit") == 0)
                Database.JobSubTypes.Add(new JobSubType { Id = "Audit", JobTypeId = JobType.JobTypeIds.HMisc, Description = "Audit" });
            if (Database.JobSubTypes.Count(jst => jst.JobTypeId == JobType.JobTypeIds.SApp && jst.Id == "Audit") == 0)
                Database.JobSubTypes.Add(new JobSubType { Id = "Audit", JobTypeId = JobType.JobTypeIds.SApp, Description = "Audit" });
            #endregion
            // End

            // Feature Request 2012-06-16 by James: https://disco.uservoice.com/forums/159707-feedback/suggestions/3002911-add-in-microphone-in-hardware-warranty-and-non-war
            #region "Microphone" Added
            if (Database.JobSubTypes.Count(jst => jst.JobTypeId == JobType.JobTypeIds.HNWar && jst.Id == "Microphone") == 0)
                Database.JobSubTypes.Add(new JobSubType { Id = "Microphone", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Microphone" });
            if (Database.JobSubTypes.Count(jst => jst.JobTypeId == JobType.JobTypeIds.HWar && jst.Id == "Microphone") == 0)
                Database.JobSubTypes.Add(new JobSubType { Id = "Microphone", JobTypeId = JobType.JobTypeIds.HWar, Description = "Microphone" });
            #endregion
            // End Feature Request

            // Feature Request 2013-05-16 by Michael
            #region "Bezel - Case Bottom Load Cover" Added
            if (Database.JobSubTypes.Count(jst => jst.JobTypeId == JobType.JobTypeIds.HNWar && jst.Id == "BezelCaseBottomCover") == 0)
                Database.JobSubTypes.Add(new JobSubType { Id = "BezelCaseBottomCover", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Bezel - Case Bottom Load Cover" });
            if (Database.JobSubTypes.Count(jst => jst.JobTypeId == JobType.JobTypeIds.HWar && jst.Id == "BezelCaseBottomCover") == 0)
                Database.JobSubTypes.Add(new JobSubType { Id = "BezelCaseBottomCover", JobTypeId = JobType.JobTypeIds.HWar, Description = "Bezel - Case Bottom Load Cover" });
            #endregion
            // End Feature Request

            // Feature Request https://github.com/garysharp/Disco/issues/1
            #region "Device Stolen/Lost"
            if (Database.JobSubTypes.Count(jst => jst.JobTypeId == JobType.JobTypeIds.HNWar && jst.Id == "DeviceStolen") == 0)
                Database.JobSubTypes.Add(new JobSubType { Id = "DeviceStolen", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Device Stolen" });
            if (Database.JobSubTypes.Count(jst => jst.JobTypeId == JobType.JobTypeIds.HNWar && jst.Id == "DeviceLost") == 0)
                Database.JobSubTypes.Add(new JobSubType { Id = "DeviceLost", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Device Lost" });
            #endregion
            // End Feature Request
        }

        private static void UpdateDeviceModelConfiguration(this DiscoDataContext Database)
        {
            if (Database.ConfigurationItems.Where(c => c.Scope.StartsWith("DeviceProfile:")).Count() > 0)
            {
                var configurationItems = Database.ConfigurationItems.Where(c => c.Scope.StartsWith("DeviceProfile:")).ToList();

                var deviceProfiles = Database.DeviceProfiles.ToDictionary(dp => dp.Id);
                foreach (var configurationItem in configurationItems)
                {
                    int profileId = int.Parse(configurationItem.Scope.Substring(configurationItem.Scope.IndexOf(":") + 1));
                    if (deviceProfiles.TryGetValue(profileId, out var dp))
                    {
                        switch (configurationItem.Key)
                        {
                            case "ComputerNameTemplate":
                                dp.ComputerNameTemplate = configurationItem.Value;
                                break;
                            case "DistributionType":
                                dp.DistributionType = (DeviceProfile.DistributionTypes)int.Parse(configurationItem.Value);
                                break;
                            case "OrganisationalUnit":
                                dp.OrganisationalUnit = configurationItem.Value;
                                break;
                            default:
                                continue; // Unknown Configuration Item - Leave in DB & Ignore
                        }
                    }
                    // Remove from DB
                    Database.ConfigurationItems.Remove(configurationItem);
                }
            }

        }

        // Added: 2013-02-07 G#
        // Fix previous problem with duplicate device models being created if new devices enrol at the same time
        // http://www.discoict.com.au/forum/support/2013/2/duplicate-device-models.aspx
        // Thanks to Michael Vorster for reporting this problem.
        private static void UpdateDeviceModelDuplicates(this DiscoDataContext Database)
        {
            var deviceModels = Database.DeviceModels.ToList();
            var duplicateModels = deviceModels.GroupBy(g => $"{g.Manufacturer}|{g.Model}").Where(g => g.Count() > 1);
            foreach (var duplicateModel in duplicateModels)
            {
                var primaryModel = duplicateModel.OrderBy(dm => dm.Id).First();

                foreach (var redundantModel in duplicateModel.Where(m => m != primaryModel))
                {
                    foreach (var affectedDevice in Database.Devices.Where(d => d.DeviceModelId == redundantModel.Id))
                    {
                        affectedDevice.DeviceModelId = primaryModel.Id;
                    }
                    if (!redundantModel.Description.EndsWith("** REDUNDANT **"))
                    {
                        if (redundantModel.Description.Length > 484)
                            redundantModel.Description = $"{redundantModel.Description.Substring(0, 484)} ** REDUNDANT **";
                        else
                            redundantModel.Description = $"{redundantModel.Description} ** REDUNDANT **";
                    }
                }
            }
        }
        // End Added: 2013-02-07 G#

        #region Migrate Users SQL
        private const string MigratePreDomainUsers_Sql = @"INSERT INTO [Users] SELECT @IdNew, u.DisplayName, u.Surname, u.GivenName, u.PhoneNumber, u.EmailAddress FROM [Users] u WHERE [Id]=@IdExisting;

UPDATE [JobQueueJobs] SET [AddedUserId]=@IdNew WHERE [AddedUserId]=@IdExisting;
UPDATE [JobQueueJobs] SET [RemovedUserId]=@IdNew WHERE [RemovedUserId]=@IdExisting;

UPDATE [DeviceAttachments] SET [TechUserId]=@IdNew WHERE [TechUserId]=@IdExisting;

UPDATE [Devices] SET [AssignedUserId]=@IdNew WHERE [AssignedUserId]=@IdExisting;

UPDATE [DeviceUserAssignments] SET [AssignedUserId]=@IdNew WHERE [AssignedUserId]=@IdExisting;

UPDATE [JobAttachments] SET [TechUserId]=@IdNew WHERE [TechUserId]=@IdExisting;

UPDATE [JobComponents] SET [TechUserId]=@IdNew WHERE [TechUserId]=@IdExisting;

UPDATE [JobLogs] SET [TechUserId]=@IdNew WHERE [TechUserId]=@IdExisting;

UPDATE [JobMetaInsurances] SET [ClaimFormSentUserId]=@IdNew WHERE [ClaimFormSentUserId]=@IdExisting;

UPDATE [JobMetaNonWarranties] SET [AccountingChargeAddedUserId]=@IdNew WHERE [AccountingChargeAddedUserId]=@IdExisting;
UPDATE [JobMetaNonWarranties] SET [AccountingChargePaidUserId]=@IdNew WHERE [AccountingChargePaidUserId]=@IdExisting;
UPDATE [JobMetaNonWarranties] SET [AccountingChargeRequiredUserId]=@IdNew WHERE [AccountingChargeRequiredUserId]=@IdExisting;
UPDATE [JobMetaNonWarranties] SET [InvoiceReceivedUserId]=@IdNew WHERE [InvoiceReceivedUserId]=@IdExisting;
UPDATE [JobMetaNonWarranties] SET [PurchaseOrderRaisedUserId]=@IdNew WHERE [PurchaseOrderRaisedUserId]=@IdExisting;
UPDATE [JobMetaNonWarranties] SET [PurchaseOrderSentUserId]=@IdNew WHERE [PurchaseOrderSentUserId]=@IdExisting;

UPDATE [Jobs] SET [ClosedTechUserId]=@IdNew WHERE [ClosedTechUserId]=@IdExisting;
UPDATE [Jobs] SET [DeviceHeldTechUserId]=@IdNew WHERE [DeviceHeldTechUserId]=@IdExisting;
UPDATE [Jobs] SET [DeviceReadyForReturnTechUserId]=@IdNew WHERE [DeviceReadyForReturnTechUserId]=@IdExisting;
UPDATE [Jobs] SET [DeviceReturnedTechUserId]=@IdNew WHERE [DeviceReturnedTechUserId]=@IdExisting;
UPDATE [Jobs] SET [OpenedTechUserId]=@IdNew WHERE [OpenedTechUserId]=@IdExisting;
UPDATE [Jobs] SET [UserId]=@IdNew WHERE [UserId]=@IdExisting;

UPDATE [UserAttachments] SET [TechUserId]=@IdNew WHERE [TechUserId]=@IdExisting;
UPDATE [UserAttachments] SET [UserId]=@IdNew WHERE [UserId]=@IdExisting;

DELETE [Users] WHERE [Id]=@IdExisting;";
        #endregion
        public static void MigratePreDomainObjects(this DiscoDataContext Database)
        {
            if (Database.Users.Count(u => !u.UserId.Contains(@"\")) > 0)
            {
                // Determine Computer Domain
                string netBiosName = null;
                string defaultNamingContext;
                using (Domain d = Domain.GetComputerDomain())
                {
                    string ldapPath = $"LDAP://{d.Name}/";
                    string configurationNamingContext;

                    using (var adRootDSE = new DirectoryEntry(ldapPath + "RootDSE"))
                    {
                        defaultNamingContext = adRootDSE.Properties["defaultNamingContext"][0].ToString();
                        configurationNamingContext = adRootDSE.Properties["configurationNamingContext"][0].ToString();
                    }

                    using (var configSearchRoot = new DirectoryEntry(ldapPath + "CN=Partitions," + configurationNamingContext))
                    {
                        var configSearchFilter = $"(&(objectcategory=Crossref)(dnsRoot={d.Name})(netBIOSName=*))";
                        var configSearchLoadProperites = new string[] { "NetBIOSName" };

                        using (var configSearcher = new DirectorySearcher(configSearchRoot, configSearchFilter, configSearchLoadProperites, SearchScope.OneLevel))
                        {
                            SearchResult configResult = configSearcher.FindOne();

                            if (configResult != null)
                                netBiosName = configResult.Properties["NetBIOSName"][0].ToString();
                            else
                                netBiosName = null;
                        }
                    }
                }
                if (string.IsNullOrWhiteSpace(netBiosName))
                    throw new InvalidOperationException("Unable to determine the Domains NetBIOS Name");

                // MIGRATE SETTINGS

                // Authorization Roles
                foreach (var authRole in Database.AuthorizationRoles.Where(ar => ar.SubjectIds != null).ToList())
                {
                    var ids = string.Join(",", authRole.SubjectIds.Split(',').Select(id => id.Contains('\\') ? id : $@"{netBiosName}\{id}"));
                    if (ids != authRole.SubjectIds)
                        authRole.SubjectIds = ids;
                }
                // Job Queues
                foreach (var jobQueue in Database.JobQueues.Where(jq => jq.SubjectIds != null).ToList())
                {
                    var ids = string.Join(",", jobQueue.SubjectIds.Split(',').Select(id => id.Contains('\\') ? id : $@"{netBiosName}\{id}"));
                    if (ids != jobQueue.SubjectIds)
                        jobQueue.SubjectIds = ids;
                }
                // Device Profiles - OU
                foreach (var deviceProfile in Database.DeviceProfiles.Where(dp => dp.OrganisationalUnit == null || !dp.OrganisationalUnit.Contains(@"DC=")).ToList())
                {
                    if (string.IsNullOrWhiteSpace(deviceProfile.OrganisationalUnit))
                        deviceProfile.OrganisationalUnit = $"CN=Computers,{defaultNamingContext}";
                    else
                        deviceProfile.OrganisationalUnit = $"{deviceProfile.OrganisationalUnit},{defaultNamingContext}";
                }
                Database.SaveChanges();

                // MIGRATE Document Templates
                var dataStoreLocation = Database.ConfigurationItems.Where(ci => ci.Scope == "System" && ci.Key == "DataStoreLocation").Select(ci => ci.Value).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(dataStoreLocation) && System.IO.Directory.Exists(dataStoreLocation))
                {
                    string filePrefix = $"{netBiosName}_";

                    var userAttachmentsDirectory = System.IO.Path.Combine(dataStoreLocation, "UserAttachments");
                    if (System.IO.Directory.Exists(userAttachmentsDirectory))
                    {
                        var files = System.IO.Directory.EnumerateFiles(userAttachmentsDirectory, "*.*", System.IO.SearchOption.AllDirectories)
                            .Where(p => !p.StartsWith(filePrefix, StringComparison.OrdinalIgnoreCase) && (p.EndsWith("_thumb.jpg") || p.EndsWith("_file"))).ToList();

                        foreach (var file in files)
                        {
                            try
                            {
                                var renameFile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(file), string.Concat(filePrefix, System.IO.Path.GetFileName(file)));
                                System.IO.File.Move(file, renameFile);
                            }
                            catch (Exception) { /* Ignore Errors */ }
                        }
                    }
                }

                // MIGRATE DEVICES
                foreach (var device in Database.Devices.Where(d => d.DeviceDomainId != null && !d.DeviceDomainId.Contains(@"\")).ToList())
                {
                    device.DeviceDomainId = $@"{netBiosName}\{device.DeviceDomainId}";
                }
                Database.SaveChanges();

                // MIGRATE USERS
                foreach (var user in Database.Users.Where(u => !u.UserId.Contains(@"\")).ToList())
                {
                    SqlParameter idExisting = new SqlParameter("@IdExisting", System.Data.SqlDbType.NVarChar, 50);
                    idExisting.Value = user.UserId;

                    SqlParameter idNew = new SqlParameter("@IdNew", System.Data.SqlDbType.NVarChar, 50);
                    idNew.Value = $@"{netBiosName}\{user.UserId}";

                    Database.Database.ExecuteSqlCommand(MigratePreDomainUsers_Sql, idExisting, idNew);
                }
            }
        }
    }
}
