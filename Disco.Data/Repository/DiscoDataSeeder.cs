using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.Repository;

namespace Disco.Data.Repository
{
    public static class DiscoDataSeeder
    {
        public static void SeedDatabase(this DiscoDataContext context)
        {
            context.SeedDeploymentId();
            context.SeedDeviceModels();
            context.SeedDeviceProfiles();
            context.SeedJobTypes();
            context.SeedJobSubTypes();
        }

        public static void SeedDeploymentId(this DiscoDataContext context)
        {
            if (context.ConfigurationItems.Count(ci => ci.Scope == "System" && ci.Key == "DeploymentId") == 0)
            {
                var deploymentId = Guid.NewGuid().ToString("D");
                context.ConfigurationItems.Add(new ConfigurationItem { Scope = "System", Key = "DeploymentId", Value = deploymentId });
            }
        }
        public static void SeedJobTypes(this DiscoDataContext context)
        {
            if (context.JobTypes.Count() == 0)
            {
                context.JobTypes.Add(new JobType { Id = JobType.JobTypeIds.HWar, Description = "Hardware - Warranty" });
                context.JobTypes.Add(new JobType { Id = JobType.JobTypeIds.HNWar, Description = "Hardware - Non-Warranty" });
                context.JobTypes.Add(new JobType { Id = JobType.JobTypeIds.HMisc, Description = "Hardware - Misc" });
                context.JobTypes.Add(new JobType { Id = JobType.JobTypeIds.SImg, Description = "Software - Reimage" });
                context.JobTypes.Add(new JobType { Id = JobType.JobTypeIds.SApp, Description = "Software - Application" });
                context.JobTypes.Add(new JobType { Id = JobType.JobTypeIds.SOS, Description = "Software - Operating System" });
            }
            // 2012-05-22
            #region "User Management" Added
            if (context.JobTypes.Count(jt => jt.Id == JobType.JobTypeIds.UMgmt) == 0)
                context.JobTypes.Add(new JobType { Id = JobType.JobTypeIds.UMgmt, Description = "User - Management" });
            #endregion
            // End
        }
        public static void SeedDeviceModels(this DiscoDataContext context)
        {
            if (context.DeviceModels.Count() == 0)
            {
                context.DeviceModels.Add(new DeviceModel { Manufacturer = "Unknown", Model = "Unknown", Description = "Unknown Device Model" });
            }
            UpdateDeviceModelConfiguration(context);
            // Removed: 2013-01-14 G#
            //UpdateDeviceModelImageStorage(context);

            // Added: 2013-02-07 G#
            UpdateDeviceModelDuplicates(context);
        }
        public static void SeedDeviceProfiles(this DiscoDataContext context)
        {
            if (context.DeviceProfiles.Count() == 0)
            {
                context.DeviceProfiles.Add(new DeviceProfile { ShortName = "WS", Name = "Default", Description = "Initial Default Workstation Profile", ComputerNameTemplate = "DeviceProfile.ShortName + ''-'' + SerialNumber" });
            }
        }
        public static void SeedJobSubTypes(this DiscoDataContext context)
        {
            if (context.JobSubTypes.Count() == 0)
            {
                context.JobSubTypes.Add(new JobSubType { Id = "Bag", JobTypeId = JobType.JobTypeIds.HWar, Description = "Bag" });
                context.JobSubTypes.Add(new JobSubType { Id = "Battery", JobTypeId = JobType.JobTypeIds.HWar, Description = "Battery" });
                context.JobSubTypes.Add(new JobSubType { Id = "BezelCaseBottom", JobTypeId = JobType.JobTypeIds.HWar, Description = "Bezel - Case Bottom" });
                context.JobSubTypes.Add(new JobSubType { Id = "BezelCaseTop", JobTypeId = JobType.JobTypeIds.HWar, Description = "Bezel - Case Top" });
                context.JobSubTypes.Add(new JobSubType { Id = "BezelScreenInner", JobTypeId = JobType.JobTypeIds.HWar, Description = "Bezel - Screen Inner" });
                context.JobSubTypes.Add(new JobSubType { Id = "BezelScreenTop", JobTypeId = JobType.JobTypeIds.HWar, Description = "Bezel - Screen Top" });
                context.JobSubTypes.Add(new JobSubType { Id = "BluetoothAdapter", JobTypeId = JobType.JobTypeIds.HWar, Description = "Bluetooth Adapter" });
                context.JobSubTypes.Add(new JobSubType { Id = "CPU", JobTypeId = JobType.JobTypeIds.HWar, Description = "CPU" });
                context.JobSubTypes.Add(new JobSubType { Id = "HardDrive", JobTypeId = JobType.JobTypeIds.HWar, Description = "Hard Drive" });
                context.JobSubTypes.Add(new JobSubType { Id = "Keyboard", JobTypeId = JobType.JobTypeIds.HWar, Description = "Keyboard" });
                context.JobSubTypes.Add(new JobSubType { Id = "Motherboard", JobTypeId = JobType.JobTypeIds.HWar, Description = "Motherboard" });
                context.JobSubTypes.Add(new JobSubType { Id = "Mouse", JobTypeId = JobType.JobTypeIds.HWar, Description = "Mouse/Track Pad" });
                context.JobSubTypes.Add(new JobSubType { Id = "PowerAdapter", JobTypeId = JobType.JobTypeIds.HWar, Description = "Power Adapter" });
                context.JobSubTypes.Add(new JobSubType { Id = "PowerCord", JobTypeId = JobType.JobTypeIds.HWar, Description = "Power Cord/Socket" });
                context.JobSubTypes.Add(new JobSubType { Id = "RAM", JobTypeId = JobType.JobTypeIds.HWar, Description = "RAM" });
                context.JobSubTypes.Add(new JobSubType { Id = "ReplacementDevice", JobTypeId = JobType.JobTypeIds.HWar, Description = "Replacement Device" });
                context.JobSubTypes.Add(new JobSubType { Id = "Screen", JobTypeId = JobType.JobTypeIds.HWar, Description = "Screen" });
                context.JobSubTypes.Add(new JobSubType { Id = "Speakers", JobTypeId = JobType.JobTypeIds.HWar, Description = "Speakers" });
                context.JobSubTypes.Add(new JobSubType { Id = "WebCamera", JobTypeId = JobType.JobTypeIds.HWar, Description = "Web Camera" });
                context.JobSubTypes.Add(new JobSubType { Id = "WirelessAdapter", JobTypeId = JobType.JobTypeIds.HWar, Description = "Wireless Adapter" });
                context.JobSubTypes.Add(new JobSubType { Id = "Other", JobTypeId = JobType.JobTypeIds.HWar, Description = "Other" });
                context.JobSubTypes.Add(new JobSubType { Id = "Bag", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Bag" });
                context.JobSubTypes.Add(new JobSubType { Id = "Battery", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Battery" });
                context.JobSubTypes.Add(new JobSubType { Id = "BezelCaseBottom", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Bezel - Case Bottom" });
                context.JobSubTypes.Add(new JobSubType { Id = "BezelCaseTop", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Bezel - Case Top" });
                context.JobSubTypes.Add(new JobSubType { Id = "BezelScreenInner", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Bezel - Screen Inner" });
                context.JobSubTypes.Add(new JobSubType { Id = "BezelScreenTop", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Bezel - Screen Top" });
                context.JobSubTypes.Add(new JobSubType { Id = "BluetoothAdapter", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Bluetooth Adapter" });
                context.JobSubTypes.Add(new JobSubType { Id = "CPU", JobTypeId = JobType.JobTypeIds.HNWar, Description = "CPU" });
                context.JobSubTypes.Add(new JobSubType { Id = "HardDrive", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Hard Drive" });
                context.JobSubTypes.Add(new JobSubType { Id = "Keyboard", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Keyboard" });
                context.JobSubTypes.Add(new JobSubType { Id = "Motherboard", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Motherboard" });
                context.JobSubTypes.Add(new JobSubType { Id = "Mouse", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Mouse/Track Pad" });
                context.JobSubTypes.Add(new JobSubType { Id = "PowerAdapter", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Power Adapter" });
                context.JobSubTypes.Add(new JobSubType { Id = "PowerCord", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Power Cord/Socket" });
                context.JobSubTypes.Add(new JobSubType { Id = "RAM", JobTypeId = JobType.JobTypeIds.HNWar, Description = "RAM" });
                context.JobSubTypes.Add(new JobSubType { Id = "ReplacementDevice", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Replacement Device" });
                context.JobSubTypes.Add(new JobSubType { Id = "Screen", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Screen" });
                context.JobSubTypes.Add(new JobSubType { Id = "Speakers", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Speakers" });
                context.JobSubTypes.Add(new JobSubType { Id = "WebCamera", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Web Camera" });
                context.JobSubTypes.Add(new JobSubType { Id = "WirelessAdapter", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Wireless Adapter" });
                context.JobSubTypes.Add(new JobSubType { Id = "Other", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Other" });
                context.JobSubTypes.Add(new JobSubType { Id = "ExternalHardDrive", JobTypeId = JobType.JobTypeIds.HMisc, Description = "External Hard Drive" });
                context.JobSubTypes.Add(new JobSubType { Id = "IWB", JobTypeId = JobType.JobTypeIds.HMisc, Description = "Interactive Whiteboard" });
                context.JobSubTypes.Add(new JobSubType { Id = "InternetDongle", JobTypeId = JobType.JobTypeIds.HMisc, Description = "Internet Dongle" });
                context.JobSubTypes.Add(new JobSubType { Id = "Keyboard", JobTypeId = JobType.JobTypeIds.HMisc, Description = "External Keyboard" });
                context.JobSubTypes.Add(new JobSubType { Id = "MobilePhone", JobTypeId = JobType.JobTypeIds.HMisc, Description = "Mobile Phone" });
                context.JobSubTypes.Add(new JobSubType { Id = "Mouse", JobTypeId = JobType.JobTypeIds.HMisc, Description = "External Mouse" });
                context.JobSubTypes.Add(new JobSubType { Id = "MP3Player", JobTypeId = JobType.JobTypeIds.HMisc, Description = "MP3 Player" });
                context.JobSubTypes.Add(new JobSubType { Id = "PrinterScanner", JobTypeId = JobType.JobTypeIds.HMisc, Description = "Printer/Scanner" });
                context.JobSubTypes.Add(new JobSubType { Id = "Projector", JobTypeId = JobType.JobTypeIds.HMisc, Description = "Projector" });
                context.JobSubTypes.Add(new JobSubType { Id = "USBFlashDrive", JobTypeId = JobType.JobTypeIds.HMisc, Description = "USB Flash Drive" });
                context.JobSubTypes.Add(new JobSubType { Id = "WebCamera", JobTypeId = JobType.JobTypeIds.HMisc, Description = "Web Camera" });
                context.JobSubTypes.Add(new JobSubType { Id = "Other", JobTypeId = JobType.JobTypeIds.HMisc, Description = "Other" });
                context.JobSubTypes.Add(new JobSubType { Id = "ContentIllegal", JobTypeId = JobType.JobTypeIds.SImg, Description = "Content - Illegal" });
                context.JobSubTypes.Add(new JobSubType { Id = "ContentInappropriate", JobTypeId = JobType.JobTypeIds.SImg, Description = "Content - Inappropriate" });
                context.JobSubTypes.Add(new JobSubType { Id = "CorruptOS", JobTypeId = JobType.JobTypeIds.SImg, Description = "Corrupt Operating System" });
                context.JobSubTypes.Add(new JobSubType { Id = "HardwareChanges", JobTypeId = JobType.JobTypeIds.SImg, Description = "Hardware Changes" });
                context.JobSubTypes.Add(new JobSubType { Id = "Malware", JobTypeId = JobType.JobTypeIds.SImg, Description = "Malware" });
                context.JobSubTypes.Add(new JobSubType { Id = "Performance", JobTypeId = JobType.JobTypeIds.SImg, Description = "Performance" });
                context.JobSubTypes.Add(new JobSubType { Id = "UserRequest", JobTypeId = JobType.JobTypeIds.SImg, Description = "User Request" });
                context.JobSubTypes.Add(new JobSubType { Id = "UpdatedImage", JobTypeId = JobType.JobTypeIds.SImg, Description = "Updated Image" });
                context.JobSubTypes.Add(new JobSubType { Id = "Other", JobTypeId = JobType.JobTypeIds.SImg, Description = "Other" });
                context.JobSubTypes.Add(new JobSubType { Id = "CurriculumTool", JobTypeId = JobType.JobTypeIds.SApp, Description = "Curriculum Tool" });
                context.JobSubTypes.Add(new JobSubType { Id = "GamingEntertainment", JobTypeId = JobType.JobTypeIds.SApp, Description = "Gaming/Entertainment" });
                context.JobSubTypes.Add(new JobSubType { Id = "ImageManipulation", JobTypeId = JobType.JobTypeIds.SApp, Description = "Image Manipulation" });
                context.JobSubTypes.Add(new JobSubType { Id = "MultimediaPlayback", JobTypeId = JobType.JobTypeIds.SApp, Description = "Multimedia Playback" });
                context.JobSubTypes.Add(new JobSubType { Id = "Presentation", JobTypeId = JobType.JobTypeIds.SApp, Description = "Presentation" });
                context.JobSubTypes.Add(new JobSubType { Id = "Spreadsheet", JobTypeId = JobType.JobTypeIds.SApp, Description = "Spreadsheet" });
                context.JobSubTypes.Add(new JobSubType { Id = "StaffTool", JobTypeId = JobType.JobTypeIds.SApp, Description = "Staff Tool" });
                context.JobSubTypes.Add(new JobSubType { Id = "StudentReporting", JobTypeId = JobType.JobTypeIds.SApp, Description = "Student Reporting" });
                context.JobSubTypes.Add(new JobSubType { Id = "VideoEditing", JobTypeId = JobType.JobTypeIds.SApp, Description = "Video Editing" });
                context.JobSubTypes.Add(new JobSubType { Id = "WebBrowser", JobTypeId = JobType.JobTypeIds.SApp, Description = "Web Browser" });
                context.JobSubTypes.Add(new JobSubType { Id = "WebBrowserPlugin", JobTypeId = JobType.JobTypeIds.SApp, Description = "Web Browser Plugin" });
                context.JobSubTypes.Add(new JobSubType { Id = "WordProcessing", JobTypeId = JobType.JobTypeIds.SApp, Description = "Word Processing" });
                context.JobSubTypes.Add(new JobSubType { Id = "Other", JobTypeId = JobType.JobTypeIds.SApp, Description = "Other" });
                context.JobSubTypes.Add(new JobSubType { Id = "Appearance", JobTypeId = JobType.JobTypeIds.SOS, Description = "Appearance & Personalisation" });
                context.JobSubTypes.Add(new JobSubType { Id = "DisplaySettings", JobTypeId = JobType.JobTypeIds.SOS, Description = "Display Settings" });
                context.JobSubTypes.Add(new JobSubType { Id = "DriversIWB", JobTypeId = JobType.JobTypeIds.SOS, Description = "Drivers - Interactive Whiteboard" });
                context.JobSubTypes.Add(new JobSubType { Id = "DriversPrintScan", JobTypeId = JobType.JobTypeIds.SOS, Description = "Drivers - Printers/Scanners" });
                context.JobSubTypes.Add(new JobSubType { Id = "DriversSystem", JobTypeId = JobType.JobTypeIds.SOS, Description = "Drivers - System" });
                context.JobSubTypes.Add(new JobSubType { Id = "DriversOther", JobTypeId = JobType.JobTypeIds.SOS, Description = "Drivers - Other" });
                context.JobSubTypes.Add(new JobSubType { Id = "Group Policy", JobTypeId = JobType.JobTypeIds.SOS, Description = "Group Policy" });
                context.JobSubTypes.Add(new JobSubType { Id = "KeyboardMouseConfig", JobTypeId = JobType.JobTypeIds.SOS, Description = "Keyboard/Mouse Configuration" });
                context.JobSubTypes.Add(new JobSubType { Id = "MalwareProtection", JobTypeId = JobType.JobTypeIds.SOS, Description = "Malware Protection" });
                context.JobSubTypes.Add(new JobSubType { Id = "NetworkDrives", JobTypeId = JobType.JobTypeIds.SOS, Description = "Network Drives" });
                context.JobSubTypes.Add(new JobSubType { Id = "NetworkWired", JobTypeId = JobType.JobTypeIds.SOS, Description = "Network - Wired" });
                context.JobSubTypes.Add(new JobSubType { Id = "NetworkWireless", JobTypeId = JobType.JobTypeIds.SOS, Description = "Network - Wireless" });
                context.JobSubTypes.Add(new JobSubType { Id = "NetworkOther", JobTypeId = JobType.JobTypeIds.SOS, Description = "Network - Other" });
                context.JobSubTypes.Add(new JobSubType { Id = "PatchUpdate", JobTypeId = JobType.JobTypeIds.SOS, Description = "Patches/Updates" });
                context.JobSubTypes.Add(new JobSubType { Id = "PowerManagement", JobTypeId = JobType.JobTypeIds.SOS, Description = "Power Management" });
                context.JobSubTypes.Add(new JobSubType { Id = "PrintersScanners", JobTypeId = JobType.JobTypeIds.SOS, Description = "Printers/Scanners" });
                context.JobSubTypes.Add(new JobSubType { Id = "SoundConfig", JobTypeId = JobType.JobTypeIds.SOS, Description = "Sound Configuration" });
                context.JobSubTypes.Add(new JobSubType { Id = "Other", JobTypeId = JobType.JobTypeIds.SOS, Description = "Other" });
            }

            // Feature Request 2012-04-27 by Elijah: https://disco.uservoice.com/forums/159707-feedback/suggestions/2803945-customisable-job-sub-types
            #region "Optical Drive" Added
            if (context.JobSubTypes.Count(jst => jst.JobTypeId == JobType.JobTypeIds.HNWar && jst.Id == "OpticalDrive") == 0)
                context.JobSubTypes.Add(new JobSubType { Id = "OpticalDrive", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Optical Drive" });
            if (context.JobSubTypes.Count(jst => jst.JobTypeId == JobType.JobTypeIds.HWar && jst.Id == "OpticalDrive") == 0)
                context.JobSubTypes.Add(new JobSubType { Id = "OpticalDrive", JobTypeId = JobType.JobTypeIds.HWar, Description = "Optical Drive" });
            #endregion
            // End Feature Request

            // 2012-05-22
            #region "User Management" Added
            if (context.JobSubTypes.Count(jst => jst.JobTypeId == JobType.JobTypeIds.UMgmt) == 0)
            {
                context.JobSubTypes.Add(new JobSubType { Id = JobSubType.UserManagementJobSubTypes.Infringement, JobTypeId = JobType.JobTypeIds.UMgmt, Description = JobSubType.UserManagementJobSubTypes.Infringement });
                context.JobSubTypes.Add(new JobSubType { Id = JobSubType.UserManagementJobSubTypes.Contact, JobTypeId = JobType.JobTypeIds.UMgmt, Description = JobSubType.UserManagementJobSubTypes.Contact });
            }
            #endregion
            // End

            // 2012-05-29 - Audits
            #region "Audit" Added
            if (context.JobSubTypes.Count(jst => jst.JobTypeId == JobType.JobTypeIds.HMisc && jst.Id == "Audit") == 0)
                context.JobSubTypes.Add(new JobSubType { Id = "Audit", JobTypeId = JobType.JobTypeIds.HMisc, Description = "Audit" });
            if (context.JobSubTypes.Count(jst => jst.JobTypeId == JobType.JobTypeIds.SApp && jst.Id == "Audit") == 0)
                context.JobSubTypes.Add(new JobSubType { Id = "Audit", JobTypeId = JobType.JobTypeIds.SApp, Description = "Audit" });
            #endregion
            // End

            // Feature Request 2012-06-16 by James: https://disco.uservoice.com/forums/159707-feedback/suggestions/3002911-add-in-microphone-in-hardware-warranty-and-non-war
            #region "Microphone" Added
            if (context.JobSubTypes.Count(jst => jst.JobTypeId == JobType.JobTypeIds.HNWar && jst.Id == "Microphone") == 0)
                context.JobSubTypes.Add(new JobSubType { Id = "Microphone", JobTypeId = JobType.JobTypeIds.HNWar, Description = "Microphone" });
            if (context.JobSubTypes.Count(jst => jst.JobTypeId == JobType.JobTypeIds.HWar && jst.Id == "Microphone") == 0)
                context.JobSubTypes.Add(new JobSubType { Id = "Microphone", JobTypeId = JobType.JobTypeIds.HWar, Description = "Microphone" });
            #endregion
            // End Feature Request
        }

        private static void UpdateDeviceModelConfiguration(this DiscoDataContext context)
        {
            if (context.ConfigurationItems.Where(c => c.Scope.StartsWith("DeviceProfile:")).Count() > 0)
            {
                var configurationItems = context.ConfigurationItems.Where(c => c.Scope.StartsWith("DeviceProfile:")).ToList();

                var deviceProfiles = context.DeviceProfiles.ToDictionary(dp => dp.Id);
                foreach (var configurationItem in configurationItems)
                {
                    int profileId = int.Parse(configurationItem.Scope.Substring(configurationItem.Scope.IndexOf(":") + 1));
                    DeviceProfile dp;
                    if (deviceProfiles.TryGetValue(profileId, out dp))
                    {
                        switch (configurationItem.Key)
                        {
                            case "ComputerNameTemplate":
                                dp.ComputerNameTemplate = configurationItem.Value;
                                break;
                            case "DistributionType":
                                dp.DistributionType = (DeviceProfile.DistributionTypes)(int.Parse(configurationItem.Value));
                                break;
                            case "OrganisationalUnit":
                                dp.OrganisationalUnit = configurationItem.Value;
                                break;
                            case "AllocateWirelessCertificate":
                                if (bool.Parse(configurationItem.Value))
                                    dp.CertificateProviderId = "";
                                else
                                    dp.CertificateProviderId = null;
                                break;
                            default:
                                continue; // Unknown Configuration Item - Leave in DB & Ignore
                        }
                    }
                    // Remove from DB
                    context.ConfigurationItems.Remove(configurationItem);
                }
            }

        }

        // Removed: 2013-01-14 G#
        //        private static void UpdateDeviceModelImageStorage(this DiscoDataContext dbContext)
        //        {
        //#pragma warning disable 0618
        //            var updateModels = dbContext.DeviceModels.Where(dm => dm.Image != null);
        //            if (updateModels.Count() > 0)
        //            {
        //                var dataStoreLocation = dbContext.ConfigurationItems.Where(ci => ci.Scope == "System" && ci.Key == "DataStoreLocation").Select(ci => ci.Value).FirstOrDefault();
        //                if (!string.IsNullOrEmpty(dataStoreLocation) && System.IO.Directory.Exists(dataStoreLocation))
        //                {
        //                    var deviceModelImagesLocation = System.IO.Path.Combine(dataStoreLocation, "DeviceModelImages");
        //                    if (!System.IO.Directory.Exists(deviceModelImagesLocation))
        //                        System.IO.Directory.CreateDirectory(deviceModelImagesLocation);
        //                    foreach (var model in updateModels)
        //                    {
        //                        var modelpath = System.IO.Path.Combine(deviceModelImagesLocation, string.Format("{0}.png", model.Id));

        //                        if (model.Image != null && model.Image.Length > 0)
        //                        {
        //                            System.IO.File.WriteAllBytes(modelpath, model.Image);
        //                        }
        //                        model.Image = null;
        //                    }
        //                }
        //            }
        //#pragma warning restore 0618
        //        }

        // Added: 2013-02-07 G#
        // Fix previous problem with duplicate device models being created if new devices enrol at the same time
        // http://www.discoict.com.au/forum/support/2013/2/duplicate-device-models.aspx
        // Thanks to Michael Vorster for reporting this problem.
        private static void UpdateDeviceModelDuplicates(this DiscoDataContext dbContext)
        {
            var deviceModels = dbContext.DeviceModels.ToList();
            var duplicateModels = deviceModels.GroupBy(g => string.Format("{0}|{1}", g.Manufacturer, g.Model)).Where(g => g.Count() > 1);
            foreach (var duplicateModel in duplicateModels)
            {
                var primaryModel = duplicateModel.OrderBy(dm => dm.Id).First();

                foreach (var redundantModel in duplicateModel.Where(m => m != primaryModel))
                {
                    foreach (var affectedDevice in dbContext.Devices.Where(d => d.DeviceModelId == redundantModel.Id))
                    {
                        affectedDevice.DeviceModelId = primaryModel.Id;
                    }
                    if (!redundantModel.Description.EndsWith("** REDUNDANT **"))
                    {
                        if (redundantModel.Description.Length > 484)
                            redundantModel.Description = string.Format("{0} ** REDUNDANT **", redundantModel.Description.Substring(0, 484));
                        else
                            redundantModel.Description = string.Format("{0} ** REDUNDANT **", redundantModel.Description);
                    }
                }
            }
        }
        // End Added: 2013-02-07 G#
    }
}
