using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Users
{
    internal static class Cache
    {
        private static ConcurrentDictionary<string, Tuple<User, AuthorizationToken, DateTime>> _Cache = new ConcurrentDictionary<string, Tuple<User, AuthorizationToken, DateTime>>();
        private const long CacheTimeoutTicks = 6000000000; // 10 Minutes

        internal static AuthorizationToken GetAuthorization(string UserId, DiscoDataContext Database, bool ForceRefresh)
        {
            Tuple<User, AuthorizationToken, DateTime> record = Get(UserId, Database, ForceRefresh);

            if (record == null)
                return null;
            else
                return record.Item2;
        }
        internal static AuthorizationToken GetAuthorization(string UserId, bool ForceRefresh)
        {
            Tuple<User, AuthorizationToken, DateTime> record = Get(UserId, ForceRefresh);

            if (record == null)
                return null;
            else
                return record.Item2;
        }
        internal static AuthorizationToken GetAuthorization(string UserId, DiscoDataContext Database)
        {
            return GetAuthorization(UserId, Database, false);
        }
        internal static AuthorizationToken GetAuthorization(string UserId)
        {
            return GetAuthorization(UserId, false);
        }

        internal static User GetUser(string UserId, DiscoDataContext Database, bool ForceRefresh)
        {
            Tuple<User, AuthorizationToken, DateTime> record = Get(UserId, Database, ForceRefresh);

            if (record == null)
                return null;
            else
                return record.Item1;
        }
        internal static User GetUser(string UserId, bool ForceRefresh)
        {
            Tuple<User, AuthorizationToken, DateTime> record = Get(UserId, ForceRefresh);

            if (record == null)
                return null;
            else
                return record.Item1;
        }
        internal static User GetUser(string UserId, DiscoDataContext Database)
        {
            return GetUser(UserId, Database, false);
        }
        internal static User GetUser(string UserId)
        {
            return GetUser(UserId, false);
        }

        internal static Tuple<User, AuthorizationToken, DateTime> Get(string UserId, DiscoDataContext Database, bool ForceRefresh)
        {
            Tuple<User, AuthorizationToken, DateTime> record = null;

            // Check Cache
            if (!ForceRefresh)
                record = TryUserCache(UserId);

            if (record == null)
            {
                var importedUser = UserService.ImportUser(Database, UserId);
                record = SetValue(UserId, importedUser);
            }

            return record;
        }
        internal static Tuple<User, AuthorizationToken, DateTime> Get(string UserId, DiscoDataContext Database)
        {
            return Get(UserId, Database, false);
        }
        internal static Tuple<User, AuthorizationToken, DateTime> Get(string UserId, bool ForceRefresh)
        {
            // Check Cache
            Tuple<User, AuthorizationToken, DateTime> record = null;

            if (!ForceRefresh)
                record = TryUserCache(UserId);

            if (record == null)
            {
                // Load from Repository
                using (DiscoDataContext database = new DiscoDataContext())
                {
                    record = Get(UserId, database, true);
                }
            }
            return record;
        }
        internal static Tuple<User, AuthorizationToken, DateTime> Get(string UserId)
        {
            return Get(UserId, false);
        }

        internal static Tuple<User, AuthorizationToken, DateTime> TryUserCache(string UserId)
        {
            var cache = _Cache;

            string userId = UserId.ToLower();
            Tuple<User, AuthorizationToken, DateTime> record;
            if (cache.TryGetValue(userId, out record))
            {
                if (record.Item3 > DateTime.Now)
                    return record;
                else
                    cache.TryRemove(userId, out record);
            }
            return null;
        }

        internal static Tuple<User, AuthorizationToken, DateTime> SetValue(string UserId, Tuple<User, AuthorizationToken> Record)
        {
            var cache = _Cache;

            string userId = UserId.ToLower();
            Tuple<User, AuthorizationToken, DateTime> record = new Tuple<User, AuthorizationToken, DateTime>(Record.Item1, Record.Item2, DateTime.Now.AddTicks(CacheTimeoutTicks));
            if (cache.ContainsKey(userId))
            {
                Tuple<User, AuthorizationToken, DateTime> oldRecord;
                if (cache.TryGetValue(userId, out oldRecord))
                {
                    cache.TryUpdate(userId, record, oldRecord);
                    return record;
                }
            }
            cache.TryAdd(userId, record);
            return record;
        }

        internal static bool InvalidateRecord(string UserId)
        {
            Tuple<User, AuthorizationToken, DateTime> userRecord;
            return _Cache.TryRemove(UserId, out userRecord);
        }

        internal static void CleanStaleCache()
        {
            var cache = _Cache;

            var userIds = cache.Keys.ToArray();
            foreach (string userId in userIds)
            {
                Tuple<User, AuthorizationToken, DateTime> record;
                if (cache.TryGetValue(userId, out record))
                {
                    if (record.Item3 <= DateTime.Now)
                        cache.TryRemove(userId, out record);
                }
            }
        }
        internal static void FlushCache()
        {
            _Cache = new ConcurrentDictionary<string, Tuple<User, AuthorizationToken, DateTime>>();
        }
    }
}
