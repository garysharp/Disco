using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.Repository;
using Disco.Data.Repository;
using Disco.Models.BI.Job;

namespace Disco.BI.JobBI
{
    public static class Utilities
    {
        public static Job Create(DiscoDataContext dbContext, Device device, User user, JobType type, List<JobSubType> subTypes, User initialTech)
        {
            Job j = new Job()
            {
                JobType = type,
                OpenedTechUserId = initialTech.Id,
                OpenedTechUser = initialTech,
                OpenedDate = DateTime.Now
            };

            // Device
            if (device != null)
            {
                j.Device = device;
                j.DeviceSerialNumber = device.SerialNumber;
            }

            // User
            if (user != null)
            {
                j.User = user;
                j.UserId = user.Id;
            }

            // Sub Types
            List<JobSubType> jobSubTypes = subTypes.ToList();
            j.JobSubTypes = jobSubTypes;

            dbContext.Jobs.Add(j);

            switch (type.Id)
            {
                case JobType.JobTypeIds.HWar:
                    dbContext.JobMetaWarranties.Add(new JobMetaWarranty() { Job = j });
                    break;
                case JobType.JobTypeIds.HNWar:
                    dbContext.JobMetaNonWarranties.Add(new JobMetaNonWarranty() { Job = j });
                    if (device != null)
                    {
                        // Add Job Components
                        var components = dbContext.DeviceComponents.Include("JobSubTypes").Where(c => !c.DeviceModelId.HasValue || c.DeviceModelId == j.Device.DeviceModelId);
                        var addedComponents = new List<DeviceComponent>();
                        foreach (var c in components)
                        {
                            if (c.JobSubTypes.Count == 0)
                            { // No Filter
                                addedComponents.Add(c);
                            }
                            else
                            {
                                foreach (var st in c.JobSubTypes)
                                {
                                    foreach (var jst in jobSubTypes)
                                    {
                                        if (st.JobTypeId == jst.JobTypeId && st.Id == jst.Id)
                                        {
                                            addedComponents.Add(c);
                                            break;
                                        }
                                    }
                                    if (addedComponents.Contains(c))
                                        break;
                                }
                            }
                        }
                        foreach (var c in addedComponents)
                            dbContext.JobComponents.Add(new JobComponent()
                            {
                                Job = j,
                                TechUserId = initialTech.Id,
                                Cost = c.Cost,
                                Description = c.Description
                            });
                    }
                    break;
            }

            return j;
        }

        public static string JobStatusDescription(string StatusId, Job j = null)
        {
            switch (StatusId)
            {
                case Job.JobStatusIds.Open:
                    return "Open";
                case Job.JobStatusIds.Closed:
                    return "Closed";
                case Job.JobStatusIds.AwaitingWarrantyRepair:
                    if (j == null)
                        return "Awaiting Warranty Repair";
                    else
                        if (j.DeviceHeld.HasValue)
                            return string.Format("Awaiting Warranty Repair ({0})", j.JobMetaWarranty.ExternalName);
                        else
                            return string.Format("Awaiting Warranty Repair - Not Held ({0})", j.JobMetaWarranty.ExternalName);
                case Job.JobStatusIds.AwaitingRepairs:
                    if (j == null)
                        return "Awaiting Repairs";
                    else
                        if (j.DeviceHeld.HasValue)
                            return string.Format("Awaiting Repairs ({0})", j.JobMetaNonWarranty.RepairerName);
                        else
                            return string.Format("Awaiting Repairs - Not Held ({0})", j.JobMetaNonWarranty.RepairerName);
                case Job.JobStatusIds.AwaitingDeviceReturn:
                    return "Awaiting Device Return";
                case Job.JobStatusIds.AwaitingUserAction:
                    return "Awaiting User Action";
                case Job.JobStatusIds.AwaitingAccountingPayment:
                    return "Awaiting Accounting Payment";
                case Job.JobStatusIds.AwaitingAccountingCharge:
                    return "Awaiting Accounting Charge";
                case Job.JobStatusIds.AwaitingInsuranceProcessing:
                    return "Awaiting Insurance Processing";
                default:
                    return "Unknown";
            }
        }
        public static string JobStatusDescription(string StatusId, JobTableModel.JobTableItemModelIncludeStatus j = null)
        {
            switch (StatusId)
            {
                case Job.JobStatusIds.Open:
                    return "Open";
                case Job.JobStatusIds.Closed:
                    return "Closed";
                case Job.JobStatusIds.AwaitingWarrantyRepair:
                    if (j == null)
                        return "Awaiting Warranty Repair";
                    else
                        if (j.DeviceHeld.HasValue)
                            return string.Format("Awaiting Warranty Repair ({0})", j.JobMetaWarranty_ExternalName);
                        else
                            return string.Format("Awaiting Warranty Repair - Not Held ({0})", j.JobMetaWarranty_ExternalName);
                case Job.JobStatusIds.AwaitingRepairs:
                    if (j == null)
                        return "Awaiting Repairs";
                    else
                        if (j.DeviceHeld.HasValue)
                            return string.Format("Awaiting Repairs ({0})", j.JobMetaNonWarranty_RepairerName);
                        else
                            return string.Format("Awaiting Repairs - Not Held ({0})", j.JobMetaNonWarranty_RepairerName);
                case Job.JobStatusIds.AwaitingDeviceReturn:
                    return "Awaiting Device Return";
                case Job.JobStatusIds.AwaitingUserAction:
                    return "Awaiting User Action";
                case Job.JobStatusIds.AwaitingAccountingPayment:
                    return "Awaiting Accounting Payment";
                case Job.JobStatusIds.AwaitingAccountingCharge:
                    return "Awaiting Accounting Charge";
                case Job.JobStatusIds.AwaitingInsuranceProcessing:
                    return "Awaiting Insurance Processing";
                default:
                    return "Unknown";
            }
        }

    }
}
