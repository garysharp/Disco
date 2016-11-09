using Disco.Data.Repository;
using Disco.Models.Services.Job;
using Disco.Models.UI.Config.JobPreferences;
using Disco.Services.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Disco.Web.Areas.Config.Models.JobPreferences
{
    public class IndexModel : ConfigJobPreferencesIndexModel
    {
        public int LongRunningJobDaysThreshold { get; set; }
        public int StaleJobMinutesThreshold { get; set; }
        public string DefaultNoticeboardTheme { get; set; }
        public LocationModes LocationMode { get; set; }
        public List<string> LocationList { get; set; }

        [DataType(DataType.MultilineText)]
        public string OnCreateExpression { get; set; }
        [DataType(DataType.MultilineText)]
        public string OnCloseExpression { get; set; }

        public List<KeyValuePair<string, string>> DefaultNoticeboardThemeOptions()
        {
            return UIHelpers.NoticeboardThemes.ToList();
        }

        public Lazy<List<Disco.Models.Repository.DeviceProfile>> DeviceProfiles = new Lazy<List<Disco.Models.Repository.DeviceProfile>>(() =>
        {
            using (var database = new DiscoDataContext())
            {
                return database.DeviceProfiles.OrderBy(a => a.Description).ToList();
            }
        });

        public Lazy<List<Disco.Models.BI.Config.OrganisationAddress>> OrganisationAddresses = new Lazy<List<Disco.Models.BI.Config.OrganisationAddress>>(() =>
        {
            using (var database = new DiscoDataContext())
            {
                return database.DiscoConfiguration.OrganisationAddresses.Addresses.OrderBy(a => a.Name).ToList();
            }
        });

        public List<KeyValuePair<int, string>> LongRunningJobDaysThresholdOptions()
        {
            var options = new List<KeyValuePair<int, string>>() {
                new  KeyValuePair<int, string>(1, "1 Day"),
                new  KeyValuePair<int, string>(2, "2 Days"),
                new  KeyValuePair<int, string>(3, "3 Days"),
                new  KeyValuePair<int, string>(4, "4 Days"),
                new  KeyValuePair<int, string>(5, "5 Days"),
                new  KeyValuePair<int, string>(6, "6 Days"),
                new  KeyValuePair<int, string>(7, "1 Week"),
                new  KeyValuePair<int, string>(14, "2 Weeks"),
                new  KeyValuePair<int, string>(21, "3 Weeks"),
                new  KeyValuePair<int, string>(28, "4 Weeks"),
                new  KeyValuePair<int, string>(35, "5 Weeks"),
                new  KeyValuePair<int, string>(42, "6 Weeks"),
                new  KeyValuePair<int, string>(49, "7 Weeks"),
                new  KeyValuePair<int, string>(56, "8 Weeks")
            };

            var current = this.LongRunningJobDaysThreshold;
            if (!options.Any(o => o.Key == current))
            {
                options.Add(new KeyValuePair<int, string>(current, string.Format("{0} Days", current)));
                options = options.OrderBy(o => o.Key).ToList();
            }

            return options;
        }

        public List<KeyValuePair<int, string>> StaleJobMinutesThresholdOptions()
        {
            var options = new List<KeyValuePair<int, string>>() {
                new KeyValuePair<int, string>(0, "<None>"),
                new KeyValuePair<int, string>(15, "15 minutes"),
                new KeyValuePair<int, string>(30, "30 minutes"),
                new KeyValuePair<int, string>(60, "1 hour"),
                new KeyValuePair<int, string>(60 * 2, "2 hours"),
                new KeyValuePair<int, string>(60 * 4, "4 hours"),
                new KeyValuePair<int, string>(60 * 8, "8 hours"),
                new KeyValuePair<int, string>(60 * 24, "1 day"),
                new KeyValuePair<int, string>(60 * 24 * 2, "2 days"),
                new KeyValuePair<int, string>(60 * 24 * 3, "3 days"),
                new KeyValuePair<int, string>(60 * 24 * 4, "4 days"),
                new KeyValuePair<int, string>(60 * 24 * 5, "5 days"),
                new KeyValuePair<int, string>(60 * 24 * 6, "6 days"),
                new KeyValuePair<int, string>(60 * 24 * 7, "1 week"),
                new KeyValuePair<int, string>(60 * 24 * 7 * 2, "2 weeks"),
                new KeyValuePair<int, string>(60 * 24 * 7 * 3, "3 weeks"),
                new KeyValuePair<int, string>(60 * 24 * 7 * 4, "4 weeks"),
                new KeyValuePair<int, string>(60 * 24 * 7 * 5, "5 weeks"),
                new KeyValuePair<int, string>(60 * 24 * 7 * 6, "6 weeks"),
                new KeyValuePair<int, string>(60 * 24 * 7 * 7, "7 weeks"),
                new KeyValuePair<int, string>(60 * 24 * 7 * 8, "8 weeks")
            };

            var current = this.StaleJobMinutesThreshold;
            if (!options.Any(o => o.Key == current))
            {
                options.Add(new KeyValuePair<int, string>(current, string.Format("{0} Minutes", current)));
                options = options.OrderBy(o => o.Key).ToList();
            }

            return options;
        }

        public List<KeyValuePair<string, string>> LocationModeOptions()
        {
            var type = typeof(LocationModes);
            var names = Enum.GetNames(type);

            return names.Select(n =>
            {
                var at = type.GetMember(n)[0].GetCustomAttributes(typeof(DisplayAttribute), false);
                if (at != null && at.Length > 0)
                    return new KeyValuePair<string, string>(n, ((DisplayAttribute)at[0]).Name);
                else
                    return new KeyValuePair<string, string>(n, n);
            }).OrderBy(i => i.Key).ToList();
        }
    }
}