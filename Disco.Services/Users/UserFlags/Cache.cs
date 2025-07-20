using Disco.Data.Repository;
using Disco.Models.Repository;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services.Users.UserFlags
{
    internal class Cache
    {
        private ConcurrentDictionary<int, (UserFlag flag, FlagPermission permission)> cache;

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
            var flags = Database.UserFlags.ToList();

            // Add Queues to In-Memory Cache
            cache = new ConcurrentDictionary<int, (UserFlag, FlagPermission)>(flags.Select(f => new KeyValuePair<int, (UserFlag, FlagPermission)>(f.Id, (f, f.Permissions))));
        }

        public (UserFlag flag, FlagPermission permission) GetUserFlag(int UserFlagId)
        {
            if (cache.TryGetValue(UserFlagId, out var item))
                return item;
            else
                return (null, null);
        }
        public List<(UserFlag flag, FlagPermission permission)> GetUserFlags()
        {
            return cache.Values.ToList();
        }

        public void AddOrUpdate(UserFlag UserFlag)
        {
            var value = (UserFlag, UserFlag.Permissions);
            cache.AddOrUpdate(UserFlag.Id, value, (key, existingItem) => value);
        }

        public void Remove(int UserFlagId)
        {
            cache.TryRemove(UserFlagId, out _);
        }
    }
}
