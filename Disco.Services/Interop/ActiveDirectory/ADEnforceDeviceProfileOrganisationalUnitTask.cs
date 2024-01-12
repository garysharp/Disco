using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services.Interop.ActiveDirectory
{
    public class ADEnforceDeviceProfileOrganisationalUnitTask : ScheduledTask
    {
        public override string TaskName => "Active Directory - Enforce Device Profile Organisational Unit";
        public override bool CancelInitiallySupported => false;
        public override bool SingleInstanceTask => false;

        public static ScheduledTaskStatus EnforceDeviceProfileOrganisationalUnit(int deviceProfileId)
        {
            JobDataMap taskData = new JobDataMap() {
                {nameof(DeviceProfile), deviceProfileId }
            };

            var instance = new ADEnforceDeviceProfileOrganisationalUnitTask();
            return instance.ScheduleTask(taskData);
        }

        protected override void ExecuteTask()
        {
            var deviceProfileId = (int)ExecutionContext.JobDetail.JobDataMap[nameof(DeviceProfile)];

            DeviceProfile deviceProfile;
            List<Device> devices;

            using (var database = new DiscoDataContext())
            {
                deviceProfile = database.DeviceProfiles.FirstOrDefault(p => p.Id == deviceProfileId);
                if (deviceProfile == null)
                {
                    Status.Finished("Device Profile not found");
                    return;
                }
                Status.UpdateStatus(0, $"Enforcing '{deviceProfile.Name}' Organisational Unit", "Loading devices");
                devices = database.Devices.Where(d => d.DeviceProfileId == deviceProfileId).ToList();
                if (devices.Count == 0)
                {
                    Status.Finished("No Devices found for Device Profile");
                    return;
                }
            }

            var organisationalUnit = deviceProfile.OrganisationalUnit;
            if (string.IsNullOrWhiteSpace(organisationalUnit))
                organisationalUnit = ActiveDirectory.Context.PrimaryDomain.DefaultComputerContainer;

            var domain = ActiveDirectory.Context.GetDomainFromDistinguishedName(organisationalUnit);
            var domainController = domain.GetAvailableDomainController(RequireWritable: true);

            using (var containerEntry = domainController.RetrieveDirectoryEntry(organisationalUnit, new[] { "objectCategory", "distinguishedName" }))
            {
                // validate the container
                var containerCategory = (string)containerEntry.Entry.Properties["objectCategory"].Value;
                if (!string.Equals(containerCategory, $"CN=Organizational-Unit,CN=Schema,{domain.ConfigurationNamingContext}", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(containerCategory, $"CN=Container,CN=Schema,{domain.ConfigurationNamingContext}", StringComparison.OrdinalIgnoreCase))
                {
                    Status.Finished("Organisational Unit is not a valid target container");
                    return;
                }

                var deviceCount = 0d;
                Status.ProgressMultiplier = 100d / devices.Count;
                foreach (var device in devices)
                {
                    Status.UpdateStatus(deviceCount++, $"Processing: {device.SerialNumber} [{device.DeviceDomainId}]");

                    if (ActiveDirectory.IsValidDomainAccountId(device.DeviceDomainId, out var computerName, out var deviceDomain))
                    {
                        if (deviceDomain != domain)
                        {
                            Status.LogWarning($"Device '{device.SerialNumber}' [{device.DeviceDomainId}] is not in the same domain as the Organisational Unit and cannot be moved");
                        }
                        else
                        {
                            var deviceAccount = domainController.RetrieveADMachineAccount(device.DeviceDomainId);

                            if (deviceAccount == null)
                            {
                                Status.LogWarning($"Device {device.SerialNumber}' [{device.DeviceDomainId}] was not found on the domain controller");
                            }
                            else
                            {
                                if (string.Equals(deviceAccount.ParentDistinguishedName, organisationalUnit, StringComparison.OrdinalIgnoreCase))
                                {
                                    Status.UpdateStatus($"Device '{device.SerialNumber}' [{device.DeviceDomainId}] is already in the correct Organisational Unit");
                                }
                                else
                                {
                                    Status.UpdateStatus($"Moving Device '{device.SerialNumber}' [{device.DeviceDomainId}] from '{deviceAccount.ParentDistinguishedName}'");
                                    try
                                    {
                                        var existingOu = deviceAccount.ParentDistinguishedName;
                                        deviceAccount.MoveOrganisationalUnit(domainController, organisationalUnit);
                                        Status.LogInformation($"Moved Device '{device.SerialNumber}' [{device.DeviceDomainId}] from '{existingOu}' to '{organisationalUnit}'");
                                    }
                                    catch (Exception ex)
                                    {
                                        Status.LogWarning($"Failed to Moved Device '{device.SerialNumber}' [{device.DeviceDomainId}] from '{deviceAccount.ParentDistinguishedName}' to '{organisationalUnit}'; {ex.Message} [{ex.GetType().Name}]");
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }
    }
}
