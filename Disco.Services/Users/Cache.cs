using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Interop.ActiveDirectory;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Disco.Services.Users
{
    internal static class Cache
    {
        private static readonly ConcurrentDictionary<string, CacheRecord> cache = new ConcurrentDictionary<string, CacheRecord>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, string> upnCache = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        internal static bool TryGet(string userId, DiscoDataContext db, bool forceRefresh, out CacheRecord record)
        {
            if (!forceRefresh && TryCache(userId, out record))
                return true;

            if (UserService.TryImportUser(db, userId, out var user, out var authorizationToken))
            {
                record = SetCache(user, authorizationToken);
                return true;
            }

            record = CacheRecord.Empty;
            return false;
        }

        internal static bool TryGet(string userId, bool forceRefresh, out CacheRecord record)
        {
            if (!forceRefresh && TryCache(userId, out record))
                return true;

            using (var db = new DiscoDataContext())
                return TryGet(userId, db, true, out record);
        }

        internal static bool TryGet(string userId, DiscoDataContext db, out CacheRecord record)
            => TryGet(userId, db, false, out record);

        internal static bool TryGet(string userId, out CacheRecord record)
            => TryGet(userId, false, out record);

        internal static bool TryGetByUserPrincipalName(string userPrincipalName, DiscoDataContext db, bool forceRefresh, out CacheRecord record)
        {
            if (upnCache.TryGetValue(userPrincipalName, out var userId))
                return TryGet(userId, db, forceRefresh, out record);

            var adUserAccount = ActiveDirectory.RetrieveADUserAccountByUserPrincipalName(userPrincipalName);

            if (adUserAccount != null && UserService.TryImportUser(db, adUserAccount, out var user, out var authorizationToken))
            {
                record = SetCache(user, authorizationToken);
                return true;
            }

            record = CacheRecord.Empty;
            return false;
        }

        private static bool TryCache(string userId, out CacheRecord record)
        {
            if (cache.TryGetValue(userId, out record))
            {
                if (record.Expiration > DateTime.Now)
                    return true;
                else
                {
                    cache.TryRemove(userId, out _);
                    record = CacheRecord.Empty;
                }
            }
            return false;
        }

        private static CacheRecord SetCache(User user, AuthorizationToken authToken)
        {
            var record = new CacheRecord(user, authToken, DateTime.Now.AddMinutes(10));

            upnCache.AddOrUpdate(record.User.UserPrincipalName, user.UserId, (upn, existing) => user.UserId);
            cache.AddOrUpdate(user.UserId, record, (id, existing) => record);

            return record;
        }

        internal static bool InvalidateRecord(string userId)
        {
            return cache.TryRemove(userId, out _);
        }

        internal static void CleanStaleCache()
        {
            var userIds = cache.Keys.ToArray();
            foreach (string userId in userIds)
            {
                if (cache.TryGetValue(userId, out var record))
                {
                    if (record.Expiration <= DateTime.Now)
                        cache.TryRemove(userId, out _);
                }
            }
        }

        internal static void FlushCache()
        {
            cache.Clear();
        }

        internal readonly struct CacheRecord
        {
            public static readonly CacheRecord Empty = new CacheRecord(null, null, DateTime.MinValue);

            public readonly User User;
            public readonly AuthorizationToken AuthToken;
            public readonly DateTime Expiration;

            public CacheRecord(User user, AuthorizationToken authToken, DateTime expiration)
            {
                User = user;
                AuthToken = authToken;
                Expiration = expiration;
            }
        }
    }
}
