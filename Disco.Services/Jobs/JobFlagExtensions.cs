using Disco.Models.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Disco.Services
{
    public static class JobFlagExtensions
    {
        private static Dictionary<string, Dictionary<long, string>> cache;

        private static Dictionary<string, Dictionary<long, string>> GetAllFlags()
        {
            if (cache == null)
            {
                var fType = typeof(Job.UserManagementFlags);
                var fMembers = fType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                var flags = new Dictionary<string, Dictionary<long, string>>();
                foreach (var f in fMembers)
                {
                    var display = (DisplayAttribute)f.GetCustomAttributes(typeof(DisplayAttribute), false)[0];

                    if (!flags.TryGetValue(display.GroupName, out var group))
                    {
                        group = new Dictionary<long, string>();
                        flags.Add(display.GroupName, group);
                    }
                    group[(long)f.GetRawConstantValue()] = display.Name;
                }
                cache = flags;
            }
            return cache;
        }

        public static Dictionary<string, List<Tuple<long, string, bool>>> ValidFlagsGrouped(this Job job)
        {
            var validFlags = new Dictionary<string, List<Tuple<long, string, bool>>>();

            var allFlags = GetAllFlags();

            var currentFlags = (long)(job.Flags ?? 0);

            foreach (var jobSubType in job.JobSubTypes)
            {
                if (allFlags.TryGetValue(jobSubType.Id, out var group))
                {
                    validFlags[jobSubType.Id] = group.Select(o => Tuple.Create(o.Key, o.Value, (currentFlags & o.Key) == o.Key)).ToList();
                }
            }
            return validFlags;
        }

        public static Dictionary<long, Tuple<string, bool>> ValidFlags(this Job job)
        {
            var validFlags = new Dictionary<long, Tuple<string, bool>>();

            var allFlags = GetAllFlags();

            var currentFlags = (long)(job.Flags ?? 0);

            foreach (var jobSubType in job.JobSubTypes)
            {
                if (allFlags.TryGetValue(jobSubType.Id, out var group))
                {
                    foreach (var option in group)
                        validFlags[option.Key] = Tuple.Create($"{jobSubType.Description}: {option.Value}", (currentFlags & option.Key) == option.Key);
                }
            }
            return validFlags;
        }
    }
}
