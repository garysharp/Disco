using Disco.Data.Repository;
using Disco.Models.Services.Jobs;
using System;
using System.Collections.Generic;

namespace Disco.Data.Configuration.Modules
{
    public class JobPreferencesConfiguration : ConfigurationBase
    {
        public JobPreferencesConfiguration(DiscoDataContext Database) : base(Database) { }

        public override string Scope { get; } = "JobPreferences";

        /// <summary>
        /// Initial comments template for new jobs
        /// </summary>
        public string InitialCommentsTemplate
        {
            get => Get<string>(null);
            set => Set(value);
        }

        /// <summary>
        /// Number of days a job is open before it is considered 'Long Running'
        /// </summary>
        public int LongRunningJobDaysThreshold
        {
            get => Get(7);
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "The Long Running Job Days Threshold cannot be less than zero");

                Set(value);
            }
        }

        /// <summary>
        /// Number of minutes since the last recorded action is performed on a job before it is considered 'Stale'
        /// </summary>
        public int StaleJobMinutesThreshold
        {
            get => Get(60 * 24 * 2);  // Default to 48 Hours (2 days)
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "The Stale Job Minutes Threshold cannot be less than zero");

                Set(value);
            }
        }

        public bool LodgmentIncludeAllAttachmentsByDefault
        {
            get => Get(false);
            set => Set(value);
        }

        /// <summary>
        /// Theme used in noticeboards by default.
        /// <see cref="Disco.Services.Extensions.UIHelpers.NoticeboardThemes"/>
        /// </summary>
        public string DefaultNoticeboardTheme
        {
            get => Get("default");
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(DefaultNoticeboardTheme));

                Set(value);
            }
        }

        public LocationModes LocationMode
        {
            get => Get(LocationModes.Unrestricted);
            set => Set(value);
        }

        public List<string> LocationList
        {
            get => Get(new List<string>());
            set => Set(value);
        }

        public string OnCreateExpression
        {
            get => Get<string>(null);
            set => Set(value);
        }

        public string OnDeviceReadyForReturnExpression
        {
            get => Get<string>(null);
            set => Set(value);
        }

        public string OnCloseExpression
        {
            get => Get<string>(null);
            set => Set(value);
        }

        public JobExportOptions LastExportOptions
        {
            get => Get(JobExportOptions.DefaultOptions());
            set
            {
                Set(value);
                LastExportDate = DateTime.Now;
            }
        }

        public DateTime? LastExportDate
        {
            get => Get<DateTime?>(null);
            set => Set(value);
        }
    }
}
