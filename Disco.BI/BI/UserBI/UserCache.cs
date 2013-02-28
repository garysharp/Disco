using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using Disco.Models.Repository;
using Disco.Data.Repository;
using System.Web;
using Quartz;
using Quartz.Impl;
using Disco.Services.Tasks;

namespace Disco.BI.UserBI
{
    public class UserCache : ScheduledTask
    {
        private static ConcurrentDictionary<string, Tuple<User, DateTime>> _Cache = new ConcurrentDictionary<string, Tuple<User, DateTime>>();
        private const long CacheTimeoutTicks = 6000000000; // 10 Minutes
        private const string CacheHttpRequestKey = "Disco_CurrentUser";

        public static User CurrentUser
        {
            get
            {
                string username = null;
                User user;

                // Check for ASP.NET
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Request.IsAuthenticated)
                    {
                        user = (User)HttpContext.Current.Items[CacheHttpRequestKey];
                        if (user != null)
                            return user;

                        username = HttpContext.Current.User.Identity.Name;
                    }
                    else
                    {
                        return null;
                        //throw new PlatformNotSupportedException("ASP.NET Authentication is not correctly configured");
                    }
                }

                // User default User
                if (username == null)
                {
                    username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                }

                user = GetUser(username);

                if (HttpContext.Current != null && HttpContext.Current.Request.IsAuthenticated)
                {
                    // Cache in current request
                    HttpContext.Current.Items[CacheHttpRequestKey] = user;
                }

                return user;
            }
        }

        public static User GetUser(string Username)
        {
            // Check Cache
            User u = TryUserCache(Username);

            if (u == null)
            {
                // Load from Repository
                using (DiscoDataContext dbContext = new DiscoDataContext())
                {
                    u = GetUser(Username, dbContext, true);
                }
            }
            return u;
        }

        public static User GetUser(string Username, DiscoDataContext dbContext, bool ForceRefresh = false)
        {
            User u = null;

            // Check Cache
            if (!ForceRefresh)
                u = TryUserCache(Username);

            if (u == null)
            {
                string username = Username.ToLower();
                u = UserBI.Utilities.LoadUser(dbContext, username);
                SetValue(username, u);
            }
            return u;
        }

        private static User TryUserCache(string Username)
        {
            string username = Username.ToLower();
            Tuple<User, DateTime> userRecord;
            if (_Cache.TryGetValue(username, out userRecord))
            {
                if (userRecord.Item2 > DateTime.Now)
                    return userRecord.Item1;
                else
                    _Cache.TryRemove(username, out userRecord);
            }
            return null;
        }

        public static bool InvalidateValue(string Key)
        {
            Tuple<User, DateTime> userRecord;
            return _Cache.TryRemove(Key.ToLower(), out userRecord);
        }

        private static bool SetValue(string Key, User User)
        {
            string key = Key.ToLower();
            Tuple<User, DateTime> userRecord = new Tuple<User, DateTime>(User, DateTime.Now.AddTicks(CacheTimeoutTicks));
            if (_Cache.ContainsKey(key))
            {
                Tuple<User, DateTime> oldUser;
                if (_Cache.TryGetValue(key, out oldUser))
                {
                    return _Cache.TryUpdate(key, userRecord, oldUser);
                }
            }
            return _Cache.TryAdd(key, userRecord);
        }

        private static void CleanStaleCache()
        {
            var usernames = _Cache.Keys.ToArray();
            foreach (string username in usernames)
            {
                Tuple<User, DateTime> userRecord;
                if (_Cache.TryGetValue(username, out userRecord))
                {
                    if (userRecord.Item2 <= DateTime.Now)
                        _Cache.TryRemove(username, out userRecord);
                }   
            }
        }

        //public void InitalizeScheduledTask(DiscoDataContext dbContext, IScheduler Scheduler)
        //{
        //    // Run @ every 15mins

        //    // Next 15min interval
        //    DateTime now = DateTime.Now;
        //    int mins = (15 - (now.Minute % 15));
        //    if (mins < 10)
        //        mins += 15;
        //    DateTimeOffset startAt = new DateTimeOffset(now).AddMinutes(mins).AddSeconds(now.Second * -1).AddMilliseconds(now.Millisecond * -1);

        //    IJobDetail jobDetail = new JobDetailImpl("UserCache_CleanStaleCache", typeof(UserCache));
        //    ITrigger trigger = TriggerBuilder.Create().
        //        WithIdentity("UserCache_CleanStaleCacheTrigger").StartAt(startAt).
        //        WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForever(15)).
        //        Build();
        //    Scheduler.ScheduleJob(jobDetail, trigger);
        //}

        public override string TaskName { get { return "User Cache - Clean Stale Cache"; } }

        public override bool SingleInstanceTask { get { return true; } }
        public override bool CancelInitiallySupported { get { return false; } }
        public override bool LogExceptionsOnly { get { return true; } }

        public override void InitalizeScheduledTask(DiscoDataContext dbContext)
        {
            // Run @ every 15mins

            // Next 15min interval
            DateTime now = DateTime.Now;
            int mins = (15 - (now.Minute % 15));
            if (mins < 10)
                mins += 15;
            DateTimeOffset startAt = new DateTimeOffset(now).AddMinutes(mins).AddSeconds(now.Second * -1).AddMilliseconds(now.Millisecond * -1);

            TriggerBuilder triggerBuilder = TriggerBuilder.Create().StartAt(startAt).
                WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForever(15));

            this.ScheduleTask(triggerBuilder);
        }

        protected override void ExecuteTask()
        {
            CleanStaleCache();
        }
    }
}
