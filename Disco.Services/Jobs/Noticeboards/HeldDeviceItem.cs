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
            get
            {
                return DeviceComputerName == null ? null : UserExtensions.FriendlyUserId(DeviceComputerName);
            }
        }
        public string DeviceComputerName { get; set; }

        public string DeviceLocation { get; set; }
        public string DeviceDescription
        {
            get
            {
                StringBuilder sb = new StringBuilder(this.DeviceComputerNameFriendly);

                if (UserId != null)
                    sb.Append(" - ").Append(this.UserDisplayName).Append(" (").Append(this.UserIdFriendly).Append(")");

                if (!string.IsNullOrWhiteSpace(this.DeviceLocation))
                    sb.Append(" - ").Append(this.DeviceLocation);
                else if (UserId == null)
                    sb.Append(" - ").Append(this.DeviceSerialNumber);

                return sb.ToString();
            }
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
                    var address = config.GetAddress(DeviceAddressId.Value);
                    if (address != null)
                        return address.ShortName;
                }

                return null;
            }
        }

        public string UserId { get; set; }
        public string UserIdFriendly
        {
            get
            {
                return UserId == null ? null : UserExtensions.FriendlyUserId(UserId);
            }
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
        }

        public bool ReadyForReturn { get; set; }
        public DateTime? EstimatedReturnTime { get; set; }
        public long? EstimatedReturnTimeUnixEpoc
        {
            get
            {
                return EstimatedReturnTime.ToUnixEpoc();
            }
        }

        public DateTime? ReadyForReturnSince { get; set; }
        public long? ReadyForReturnSinceUnixEpoc
        {
            get
            {
                return ReadyForReturnSince.ToUnixEpoc();
            }
        }

        public bool IsAlert
        {
            get
            {
                if (this.ReadyForReturn && (this.ReadyForReturnSince.Value < DateTime.Now.AddDays(-3)))
                    return true;

                if (this.WaitingForUserAction && (this.WaitingForUserActionSince.Value < DateTime.Now.AddDays(-6)))
                    return true;

                return false;
            }
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
