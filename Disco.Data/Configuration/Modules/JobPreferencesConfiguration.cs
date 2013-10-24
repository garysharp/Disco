using Disco.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
