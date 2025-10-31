using Disco.Data.Configuration.Modules;
using Disco.Models.Repository;
using Disco.Models.Services.Jobs.Noticeboards;
using Disco.Services.Interop.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disco.Services.Jobs.Noticeboards
{
    public class HeldDeviceItem : IHeldDeviceItem
    {
        public int JobId { get; set; }

        public string DeviceSerialNumber { get; set; }
        public string DeviceComputerNameFriendly
        {
            get => DeviceComputerName == null ? null : ActiveDirectory.FriendlyAccountId(DeviceComputerName);
            set { } // for XML Serialization
        }
        public string DeviceComputerName { get; set; }
        public string DeviceName
        {
            get => DeviceComputerNameFriendly ?? DeviceSerialNumber;
            set { }
        }

        public string DeviceLocation { get; set; }
        public string DeviceDescription
        {
            get
            {
                StringBuilder sb = new StringBuilder(DeviceName);

                if (UserId != null)
                    sb.Append(" - ").Append(UserDisplayName).Append(" (").Append(UserIdFriendly).Append(")");

                if (!string.IsNullOrWhiteSpace(DeviceLocation))
                    sb.Append(" - ").Append(DeviceLocation);
                else if (UserId == null)
                    sb.Append(" - ").Append(DeviceSerialNumber);

                return sb.ToString();
            }
            set { } // for XML Serialization
        }

        public int DeviceProfileId { get; set; }
        public int? DeviceAddressId { get; set; }
        public string DeviceAddressShortName
        {
            get
            {
                if (DeviceAddressId.HasValue)
                {
                    var config = new OrganisationAddressesConfiguration(null);
                    return config.GetAddress(DeviceAddressId.Value)?.ShortName;
                }

                return null;
            }
            set { } // for XML Serialization
        }
        public IEnumerable<int> JobQueueIds { get; set; }

        public string UserId { get; set; }
        public string UserIdFriendly
        {
            get
            {
                return UserId == null ? null : ActiveDirectory.FriendlyAccountId(UserId);
            }
            set { } // for XML Serialization
        }
        public string UserDisplayName { get; set; }

        public bool WaitingForUserAction { get; set; }
        public DateTime? WaitingForUserActionSince { get; set; }
        public long? WaitingForUserActionSinceUnixEpoc
        {
            get
            {
                return WaitingForUserActionSince.ToUnixEpoc();
            }
            set { } // for XML Serialization
        }

        public bool ReadyForReturn { get; set; }
        public DateTime? EstimatedReturnTime { get; set; }
        public long? EstimatedReturnTimeUnixEpoc
        {
            get
            {
                return EstimatedReturnTime.ToUnixEpoc();
            }
            set { } // for XML Serialization
        }

        public DateTime? ReadyForReturnSince { get; set; }
        public long? ReadyForReturnSinceUnixEpoc
        {
            get
            {
                return ReadyForReturnSince.ToUnixEpoc();
            }
            set { } // for XML Serialization
        }

        public bool IsAlert
        {
            get
            {
                if (ReadyForReturn && (ReadyForReturnSince.Value < DateTime.Now.AddDays(-3)))
                    return true;

                if (WaitingForUserAction && (WaitingForUserActionSince.Value < DateTime.Now.AddDays(-6)))
                    return true;

                return false;
            }
            set { } // for XML Serialization
        }

        internal static IEnumerable<HeldDeviceItem> FromJobs(IQueryable<Job> jobs)
        {
            return jobs.Select(j =>
                new HeldDeviceItem
                {
                    JobId = j.Id,
                    DeviceSerialNumber = j.DeviceSerialNumber,
                    DeviceComputerName = j.Device.DeviceDomainId,
                    DeviceLocation = j.Device.Location,
                    DeviceProfileId = j.Device.DeviceProfileId,
                    DeviceAddressId = j.Device.DeviceProfile.DefaultOrganisationAddress,
                    JobQueueIds = j.JobQueues.Where(q => q.RemovedDate == null).Select(q => q.JobQueueId),
                    UserId = j.Device.AssignedUserId,
                    UserDisplayName = j.Device.AssignedUser.DisplayName,
                    WaitingForUserAction = j.WaitingForUserAction.HasValue || ((j.JobMetaNonWarranty.AccountingChargeRequiredDate.HasValue || j.JobMetaNonWarranty.AccountingChargeAddedDate.HasValue) && !j.JobMetaNonWarranty.AccountingChargePaidDate.HasValue),
                    WaitingForUserActionSince = j.WaitingForUserAction.HasValue ? j.WaitingForUserAction : (j.JobMetaNonWarranty.AccountingChargeRequiredDate.HasValue ? j.JobMetaNonWarranty.AccountingChargeRequiredDate : j.JobMetaNonWarranty.AccountingChargeAddedDate),
                    ReadyForReturn = j.DeviceReadyForReturn.HasValue && !j.DeviceReturnedDate.HasValue,
                    EstimatedReturnTime = j.ExpectedClosedDate,
                    ReadyForReturnSince = j.DeviceReadyForReturn
                });
        }
    }
}
