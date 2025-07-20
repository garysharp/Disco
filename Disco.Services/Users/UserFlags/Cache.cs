using Disco.Data.Repository;
using Disco.Models.Repository;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services.Users.UserFlags
{
    internal class Cache
    {
        private ConcurrentDictionary<int, UserFlag> _Cache;

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
            _Cache = new ConcurrentDictionary<int, UserFlag>(flags.Select(f => new KeyValuePair<int, UserFlag>(f.Id, f)));
        }

        public UserFlag GetUserFlag(int UserFlagId)
        {
            if (_Cache.TryGetValue(UserFlagId, out var item))
                return item;
            else
                return null;
        }
        public List<UserFlag> GetUserFlags()
        {
            return _Cache.Values.ToList();
        }

        public void AddOrUpdate(UserFlag UserFlag)
        {
            _Cache.AddOrUpdate(UserFlag.Id, UserFlag, (key, existingItem) => UserFlag);
        }

        public UserFlag Remove(int UserFlagId)
        {
            if (_Cache.TryRemove(UserFlagId, out var item))
                return item;
            else
                return null;
        }
        public UserFlag Remove(UserFlag UserFlag)
        {
            return Remove(UserFlag.Id);
        }
    }
}
