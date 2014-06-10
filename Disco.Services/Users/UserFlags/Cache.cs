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
            this._Cache = new ConcurrentDictionary<int, UserFlag>(flags.Select(f => new KeyValuePair<int, UserFlag>(f.Id, f)));
        }

        public UserFlag GetUserFlag(int UserFlagId)
        {
            UserFlag item;
            if (_Cache.TryGetValue(UserFlagId, out item))
                return item;
            else
                return null;
        }
        public List<UserFlag> GetUserFlags()
        {
            return _Cache.Values.ToList();
        }

        public UserFlag Update(UserFlag UserFlag)
        {
            UserFlag existingItem;

            if (_Cache.TryGetValue(UserFlag.Id, out existingItem))
            {
                if (_Cache.TryUpdate(UserFlag.Id, UserFlag, existingItem))
                {
                    return UserFlag;
                }
                else
                    return null;
            }
            else
            {
                if (_Cache.TryAdd(UserFlag.Id, UserFlag))
                {
                    return UserFlag;
                }
                else
                    return null;
            }
        }

        public UserFlag Remove(int UserFlagId)
        {
            UserFlag item;
            if (_Cache.TryRemove(UserFlagId, out item))
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
