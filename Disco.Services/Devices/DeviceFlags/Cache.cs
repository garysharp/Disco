using Disco.Data.Repository;
using Disco.Models.Repository;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services.Devices.DeviceFlags
{
    internal class Cache
    {
        private ConcurrentDictionary<int, (DeviceFlag, FlagPermission permission)> cache;

        public Cache(DiscoDataContext Database)
        {
            Initialize(Database);
        }

        public void ReInitialize(DiscoDataContext Database)
        {
            Initialize(Database);
        }

        private void Initialize(DiscoDataContext Database)
        {
            // Queues from Database
            var flags = Database.DeviceFlags.ToList();

            // Add Queues to In-Memory Cache
            cache = new ConcurrentDictionary<int, (DeviceFlag, FlagPermission permission)>(flags.Select(f => new KeyValuePair<int, (DeviceFlag, FlagPermission permission)>(f.Id, (f, f.Permissions))));
        }

        public (DeviceFlag flag, FlagPermission permission) GetDeviceFlag(int deviceFlagId)
        {
            if (cache.TryGetValue(deviceFlagId, out var item))
                return item;
            else
                return (null, null);
        }
        public List<(DeviceFlag flag, FlagPermission permission)> GetDeviceFlags()
        {
            return cache.Values.ToList();
        }

        public void AddOrUpdate(DeviceFlag flag)
        {
            var value = (flag, flag.Permissions);
            cache.AddOrUpdate(flag.Id, value, (key, existingItem) => value);
        }

        public void Remove(int deviceFlagId)
        {
            cache.TryRemove(deviceFlagId, out _);
        }
    }
}
