using Newtonsoft.Json;
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
        public string VicEduDeptWanId { get; set; }

        public List<StatisticInt> Stat_JobCounts { get; set; }
        public List<StatisticInt> Stat_OpenJobCounts { get; set; }
        public List<StatisticInt> Stat_ActiveDeviceModelCounts { get; set; }
        public List<StatisticInt> Stat_DeviceModelCounts { get; set; }
        public List<StatisticInt> Stat_UserCounts { get; set; }

        public List<StatisticString> InstalledPlugins { get; set; }

        public List<StatisticIntPair> Stat_JobIdentifiers { get; set; }
        public List<StatisticJob> Stat_Jobs { get; set; }

        public class StatisticIntPair
        {
            [JsonProperty("B")]
            public int Begin;
            [JsonProperty("E")]
            public int End;
        }

        public class StatisticInt
        {
            [JsonProperty("K")]
            public string Key;
            [JsonProperty("V")]
            public int Value;
        }

        public class StatisticString
        {
            [JsonProperty("K")]
            public string Key;
            [JsonProperty("V")]
            public string Value;
        }

        public class StatisticJob
        {
            /// <summary>
            /// Job Identifier
            /// </summary>
            [JsonProperty("I")]
            public int Identifier { get; set; }

            /// <summary>
            /// Opened Date
            /// </summary>
            [JsonProperty("OD")]
            public DateTime OpenedDate { get; set; }

            /// <summary>
            /// Closed Date
            /// </summary>
            [JsonProperty("CD", NullValueHandling = NullValueHandling.Ignore)]
            public DateTime? ClosedDate { get; set; }

            /// <summary>
            /// Job Type
            /// </summary>
            [JsonProperty("T")]
            public string Type { get; set; }

            /// <summary>
            /// Job Sub Types (Semicolon Separated)
            /// </summary>
            [JsonProperty("ST")]
            public string SubTypes { get; set; }

            /// <summary>
            /// Deployment-Unique Device Serial Identifier (Device Serial Number anonymized via hashing salted with Deployment Secret)
            /// </summary>
            [JsonProperty("D", NullValueHandling = NullValueHandling.Ignore)]
            public string DeviceIdentifier { get; set; }

            /// <summary>
            /// Deployment-Unique Job User Identifier (Job User Id anonymized via hashing salted with Deployment Secret)
            /// </summary>
            [JsonProperty("U", NullValueHandling = NullValueHandling.Ignore)]
            public string UserIdentifier { get; set; }

            /// <summary>
            /// Deployment-Unique Job Technician Identifier (Job Technician Id anonymized via hashing salted with Deployment Secret)
            /// </summary>
            [JsonProperty("TI")]
            public string TechnicianIdentifier { get; set; }

            /// <summary>
            /// Device Model
            /// </summary>
            [JsonProperty("DM", NullValueHandling = NullValueHandling.Ignore)]
            public string DeviceModel { get; set; }

            /// <summary>
            /// External Repairer
            /// </summary>
            [JsonProperty("R", NullValueHandling = NullValueHandling.Ignore)]
            public string Repairer { get; set; }

            /// <summary>
            /// External Repairer Logged
            /// </summary>
            [JsonProperty("RL", NullValueHandling = NullValueHandling.Ignore)]
            public DateTime? RepairerLogged { get; set; }

            /// <summary>
            /// External Repairer Completed
            /// </summary>
            [JsonProperty("RC", NullValueHandling = NullValueHandling.Ignore)]
            public DateTime? RepairerCompleted { get; set; }
        }
    }
}
