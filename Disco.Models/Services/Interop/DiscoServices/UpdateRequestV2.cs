using System;
using System.Collections.Generic;

namespace Disco.Models.Services.Interop.DiscoServices
{
    public class UpdateRequestV2
    {
        public Guid DeploymentId { get; set; }
        public DateTime RequestDate { get; set; }
        public string VersionCurrent { get; set; }
        public bool IsBetaDeployment { get; set; }

        public string OrganisationName { get; set; }
        public string BroadbandDoeWanId { get; set; }

        public List<StatisticInt> Stat_JobCounts { get; set; }
        public List<StatisticInt> Stat_OpenJobCounts { get; set; }
        public List<StatisticInt> Stat_ActiveDeviceModelCounts { get; set; }
        public List<StatisticInt> Stat_DeviceModelCounts { get; set; }
        public List<StatisticInt> Stat_UserCounts { get; set; }

        public List<StatisticString> InstalledPlugins { get; set; }

        public List<StatisticJob> Stat_Jobs { get; set; }

        public class StatisticInt
        {
            public string K;
            public int V;
        }

        public class StatisticString
        {
            public string K;
            public string V;
        }

        public class StatisticJob
        {
            /// <summary>
            /// Job Identifier
            /// </summary>
            public int I { get; set; }

            /// <summary>
            /// Opened Date
            /// </summary>
            public DateTime OD { get; set; }

            /// <summary>
            /// Closed Date
            /// </summary>
            public DateTime? CD { get; set; }

            /// <summary>
            /// Job Type
            /// </summary>
            public string T { get; set; }

            /// <summary>
            /// Job Sub Types (Semicolon Separated)
            /// </summary>
            public string ST { get; set; }

            /// <summary>
            /// Deployment-Unique Device Serial Identifier (Device Serial Number anonymized via hashing salted with Deployment Secret)
            /// </summary>
            public string D { get; set; }

            /// <summary>
            /// Deployment-Unique Job User Identifier (Job User Id anonymized via hashing salted with Deployment Secret)
            /// </summary>
            public string U { get; set; }

            /// <summary>
            /// Deployment-Unique Job Technician Identifier (Job Technician Id anonymized via hashing salted with Deployment Secret)
            /// </summary>
            public string TI { get; set; }

            /// <summary>
            /// Device Model
            /// </summary>
            public string DM { get; set; }

            /// <summary>
            /// External Repairer
            /// </summary>
            public string R { get; set; }

            /// <summary>
            /// External Repairer Logged
            /// </summary>
            public DateTime? RL { get; set; }

            /// <summary>
            /// External Repairer Completed
            /// </summary>
            public DateTime? RC { get; set; }
        }
    }
}
