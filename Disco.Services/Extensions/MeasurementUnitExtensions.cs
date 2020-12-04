using Disco.Models.ClientServices.EnrolmentInformation;

namespace Disco.Services
{
    public static class MeasurementUnitExtensions
    {

        private const ulong kilobyte = 1024ul;
        private const ulong megabyte = kilobyte * 1024ul;
        private const ulong gigabyte = megabyte * 1024ul;
        private const ulong terabyte = gigabyte * 1024ul;

        private const ulong maxbit = 9223372036854775807ul;
        private const ulong kilobit = 1000ul;
        private const ulong megabit = kilobit * 1000ul;
        private const ulong gigabit = megabit * 1000ul;

        private const uint gigahertz = 1000u;

        public static string ByteSizeToFriendly(ulong bytes)
        {
            if (bytes >= terabyte)
            {
                return $"{(double)bytes / terabyte:N2} TB";
            }
            if (bytes >= gigabyte)
            {
                return $"{(double)bytes / gigabyte:N2} GB";
            }
            if (bytes >= megabyte)
            {
                return $"{(double)bytes / megabyte:N2} MB";
            }
            if (bytes >= kilobyte)
            {
                return $"{(double)bytes / kilobyte:N2} KB";
            }
            return $"{bytes:N0} B";
        }

        public static string SpeedPacketBitsToFriendly(ulong speed)
        {
            if (speed == maxbit || speed == 0)
                return "Unknown";
            if (speed >= gigabit)
            {
                if (speed % gigabit == 0)
                    return $"{(double)speed / gigabit:N0} Gbps";
                else
                    return $"{(double)speed / gigabit:N2} Gbps";
            }
            if (speed >= megabit)
            {
                if (speed % megabit == 0)
                    return $"{(double)speed / megabit:N0} Mbps";
                else
                    return $"{(double)speed / megabit:N2} Mbps";
            }
            if (speed >= kilobit)
            {
                if (speed % kilobit == 0)
                    return $"{(double)speed / kilobit:N0} Kbps";
                else
                    return $"{(double)speed / kilobit:N2} Kbps";
            }
            return $"{speed:N0} Bps";
        }

        public static string SpeedMegahertzToFriendly(uint speed)
        {
            if (speed >= gigahertz)
            {
                if (speed % gigahertz == 0)
                    return $"{(double)speed / gigahertz:N0} GHz";
                else
                    return $"{(double)speed / gigahertz:N2} GHz";
            }
            return $"{speed:N0} MHz";
        }

        public static string SizeFriendly(this DiskDrive diskDrive) => ByteSizeToFriendly(diskDrive.Size ?? 0);
        public static string SizeFriendly(this DiskDrivePartition partition) => ByteSizeToFriendly(partition.Size ?? 0);
        public static string SizeFriendly(this DiskLogical disk) => ByteSizeToFriendly(disk.Size ?? 0);
        public static string FreeSpaceFriendly(this DiskLogical disk) => ByteSizeToFriendly(disk.FreeSpace ?? 0);
        public static string SpeedFriendly(this NetworkAdapter networkAdapter) => SpeedPacketBitsToFriendly(networkAdapter.Speed);
        public static string CapacityFriendly(this PhysicalMemory physicalMemory) => ByteSizeToFriendly(physicalMemory.Capacity ?? 0);
        public static string MaxClockSpeedFriendly(this Processor processor) => SpeedMegahertzToFriendly(processor.MaxClockSpeed ?? 0);

    }
}
