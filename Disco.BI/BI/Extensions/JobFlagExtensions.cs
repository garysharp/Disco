using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.Repository;
using System.ComponentModel.DataAnnotations;

namespace Disco.BI.Extensions
{
    public static class JobFlagExtensions
    {

        private static Dictionary<string, Dictionary<long, string>> allFlags;
        private static void CacheAllFlags()
        {
            if (allFlags == null)
            {
                var fType = typeof(Job.UserManagementFlags);
                var fMembers = fType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                var flags = new Dictionary<string, Dictionary<long, string>>();
                foreach (var f in fMembers)
                {
                    DisplayAttribute display = (DisplayAttribute)(f.GetCustomAttributes(typeof(DisplayAttribute), false)[0]);
                    string gn = display.GroupName;
                    Dictionary<long, string> g;
                    if (!flags.TryGetValue(gn, out g))
                    {
                        g = new Dictionary<long, string>();
                        flags.Add(gn, g);
                    }
                    g[(long)f.GetRawConstantValue()] = display.Name;
                }
                allFlags = flags;
            }
        }

        public static Dictionary<string, List<Tuple<long, string, bool>>> ValidFlagsGrouped(this Job j)
        {
            Dictionary<string, List<Tuple<long, string, bool>>> validFlags = new Dictionary<string, List<Tuple<long, string, bool>>>();

            CacheAllFlags();

            var currentFlags = j.Flags ?? 0;

            foreach (var jt in j.JobSubTypes)
            {
                Dictionary<long, string> g;
                if (allFlags.TryGetValue(jt.Id, out g))
                {
                    validFlags[jt.Id] = g.Select(f => new Tuple<long, string, bool>(f.Key, f.Value, ((currentFlags & f.Key) == f.Key))).ToList();
                }
                else
                {
                    validFlags[jt.Id] = null;
                }
            }
            return validFlags;
        }
        public static Dictionary<long, Tuple<string, bool>> ValidFlags(this Job j)
        {
            Dictionary<long, Tuple<string, bool>> validFlags = new Dictionary<long, Tuple<string, bool>>();

            CacheAllFlags();

            var currentFlags = j.Flags ?? 0;

            foreach (var jt in j.JobSubTypes)
            {
                Dictionary<long, string> g;
                if (allFlags.TryGetValue(jt.Id, out g))
                {
                    foreach (var f in g)
                        validFlags[f.Key] = new Tuple<string, bool>(string.Format("{0}: {1}", jt.Description, f.Value), ((currentFlags & f.Key) == f.Key));
                }
            }
            return validFlags;
        }
    }
}
