using Disco.Data.Repository;
using Disco.Models.BI.Job;
using System;
using System.Collections.Generic;

namespace Disco.Data.Configuration.Modules
{
    public class JobPreferencesConfiguration : ConfigurationBase
    {
        public JobPreferencesConfiguration(DiscoDataContext Database) : base(Database) { }

        public override string Scope { get { return "JobPreferences"; } }

        /// <summary>
        /// Number of days a job is open before it is considered 'Long Running'
        /// </summary>
        public int LongRunningJobDaysThreshold
        {
            get { return Get<int>(7); }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "The Long Running Job Days Threshold cannot be less than zero");

                Set(value);
            }
        }

        /// <summary>
        /// Number of minutes since the last recorded action is performed on a job before it is considered 'Stale'
        /// </summary>
        public int StaleJobMinutesThreshold
        {
            get { return Get<int>(60 * 24 * 2); } // Default to 48 Hours (2 days)
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "The Stale Job Minutes Threshold cannot be less than zero");

                Set(value);
            }
        }

        public LocationModes LocationMode
        {
            get { return Get<LocationModes>(LocationModes.Unrestricted); }
            set { Set(value); }
        }

        public List<string> LocationList
        {
            get { return Get<List<string>>(new List<string>()); }
            set { Set(value); }
        }
    }
}
