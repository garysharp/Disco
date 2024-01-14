using Disco.Data.Repository;
using Disco.Models.Repository;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services.Devices.DeviceFlags
{
    internal class Cache
    {
        private ConcurrentDictionary<int, DeviceFlag> _Cache;

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
            _Cache = new ConcurrentDictionary<int, DeviceFlag>(flags.Select(f => new KeyValuePair<int, DeviceFlag>(f.Id, f)));
        }

        public DeviceFlag GetDeviceFlag(int deviceFlagId)
        {
            if (_Cache.TryGetValue(deviceFlagId, out var item))
                return item;
            else
                return null;
        }
        public List<DeviceFlag> GetDeviceFlags()
        {
            return _Cache.Values.ToList();
        }

        public void AddOrUpdate(DeviceFlag flag)
        {
            _Cache.AddOrUpdate(flag.Id, flag, (key, existingItem) => flag);
        }

        public DeviceFlag Remove(int deviceFlagId)
        {
            if (_Cache.TryRemove(deviceFlagId, out var item))
                return item;
            else
                return null;
        }
        public DeviceFlag Remove(DeviceFlag deviceFlag)
        {
            return Remove(deviceFlag.Id);
        }
    }
}
