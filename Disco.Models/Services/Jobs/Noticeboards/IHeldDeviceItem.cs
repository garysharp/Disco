using System;
using System.Collections.Generic;

namespace Disco.Models.Services.Jobs.Noticeboards
{
    public interface IHeldDeviceItem
    {
        int JobId { get; }

        string DeviceSerialNumber { get; }
        string DeviceComputerNameFriendly { get; }
        string DeviceComputerName { get; }
        string DeviceName { get; }

        string DeviceLocation { get; }
        string DeviceDescription { get; }

        int DeviceProfileId { get; }
        int? DeviceAddressId { get; }
        string DeviceAddressShortName { get; }
        IEnumerable<int> JobQueueIds { get; }

        string UserId { get; }
        string UserIdFriendly { get; }
        string UserDisplayName { get; }

        bool WaitingForUserAction { get; }
        DateTime? WaitingForUserActionSince { get; }
        long? WaitingForUserActionSinceUnixEpoc { get; }

        bool ReadyForReturn { get; }
        DateTime? EstimatedReturnTime { get; }
        long? EstimatedReturnTimeUnixEpoc { get; }
        DateTime? ReadyForReturnSince { get; }
        long? ReadyForReturnSinceUnixEpoc { get; }

        bool IsAlert { get; }
    }
}
