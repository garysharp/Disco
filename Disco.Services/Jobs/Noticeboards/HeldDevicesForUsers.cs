using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Jobs.Noticeboards;
using Disco.Services.Interop.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services.Jobs.Noticeboards
{
    public class HeldDevicesForUsers
    {
        public const string Name = "HeldDevicesForUsers";

        // NOTE: Calculation for updates is performed by HeldDevices to avoid duplication

        internal static void BroadcastUpdates(DiscoDataContext Database, List<string> UserIds)
        {
            var jobs = Database.Devices.Where(d => UserIds.Contains(d.AssignedUserId)).SelectMany(d => d.Jobs);

            var items = GetHeldDevicesForUsers(jobs).ToDictionary(i => i.UserId, StringComparer.OrdinalIgnoreCase);

            for (int skipAmount = 0; skipAmount < UserIds.Count; skipAmount = skipAmount + 30)
            {
                var updates = UserIds
                    .Skip(skipAmount).Take(30)
                    .ToDictionary(userId => userId,
                    userId =>
                    {
                        IHeldDeviceItem item;
                        items.TryGetValue(userId, out item);
                        return item;
                    });

                NoticeboardUpdatesHub.HubContext.Clients
                    .Group(HeldDevicesForUsers.Name)
                    .updateHeldDeviceForUser(updates);
            }
        }

        private static IEnumerable<IHeldDeviceItem> GetHeldDevicesForUsers(IQueryable<Job> query)
        {
            var jobs = query
                .Where(j =>
                    !j.ClosedDate.HasValue &&
                    j.DeviceSerialNumber != null &&
                    j.Device.AssignedUserId != null &&
                    ((j.DeviceHeld.HasValue && !j.DeviceReturnedDate.HasValue) || j.WaitingForUserAction.HasValue)
                    )
                .SelectHeldDeviceItems()
                .GroupBy(j => j.UserId);

            foreach (var job in jobs.ToList())
            {
                if (job.Any(j => j.WaitingForUserAction))
                {
                    var item = job.Where(j => j.WaitingForUserAction).OrderBy(j => j.WaitingForUserActionSince).First();

                    yield return item;
                }
                else
                {
                    if (job.All(j => j.ReadyForReturn))
                    {
                        var item = job.OrderByDescending(j => j.ReadyForReturnSince).First();

                        yield return item;
                    }
                    else
                    {
                        var item = job.Where(j => !j.ReadyForReturn).OrderByDescending(j => j.EstimatedReturnTime).First();

                        yield return item;
                    }
                }
            }
        }

        public static IEnumerable<IHeldDeviceItem> GetHeldDevicesForUsers(DiscoDataContext Database)
        {
            return GetHeldDevicesForUsers(Database.Jobs);
        }
        public static IHeldDeviceItem GetHeldDeviceForUsers(DiscoDataContext Database, string UserId)
        {
            UserId = ActiveDirectory.ParseDomainAccountId(UserId);

            return GetHeldDevicesForUsers(Database.Devices.Where(d => d.AssignedUserId == UserId).SelectMany(d => d.Jobs)).FirstOrDefault();
        }
    }
}
