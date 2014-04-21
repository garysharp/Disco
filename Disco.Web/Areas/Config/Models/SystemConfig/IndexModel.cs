using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.Data.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using Disco.Data.Repository;
using Disco.Models.BI.Interop.Community;
using Disco.Services.Tasks;
using System.DirectoryServices.ActiveDirectory;
using Disco.Services.Interop.ActiveDirectory;

namespace Disco.Web.Areas.Config.Models.SystemConfig
{
    public class IndexModel
    {
        public Version DiscoVersion { get; set; }
        public DateTime? DiscoVersionBuilt
        {
            get
            {
                var v = DiscoVersion;
                if (v != null)
                {
                    try
                    {
                        return new DateTime(v.Minor + 2011, v.Build / 100, v.Build % 100, v.Revision / 100, v.Revision % 100, 0);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                else
                    return null;
            }
        }

        public string DataStoreLocation { get; set; }

        #region Database Connection
        private Lazy<SqlConnectionStringBuilder> DatabaseConnectionString = new Lazy<SqlConnectionStringBuilder>(() =>
        {
            return new SqlConnectionStringBuilder(Disco.Data.Repository.DiscoDatabaseConnectionFactory.DiscoDataContextConnectionString);
        });
        public string DatabaseServer
        {
            get
            {
                return this.DatabaseConnectionString.Value.DataSource;
            }
        }
        public string DatabaseName
        {
            get
            {
                return this.DatabaseConnectionString.Value.InitialCatalog;
            }
        }
        public string DatabaseAuthentication
        {
            get
            {
                return this.DatabaseConnectionString.Value.IntegratedSecurity ? "Integrated Authentication" : "SQL Authentication";
            }
        }
        public string DatabaseSqlAuthUsername
        {
            get
            {
                return this.DatabaseConnectionString.Value.IntegratedSecurity ? null : this.DatabaseConnectionString.Value.UserID;
            }
        }
        #endregion

        #region Active Directory

        [Display(Name="Search All Forest Servers")]
        public bool ADSearchAllForestServers { get; set; }

        public List<ADDomain> ADDomains { get; set; }
        public ADDomain ADPrimaryDomain { get; set; }
        public ADSite ADSite { get; set; }
        public List<ADDomainController> ADServers { get; set; }
        public List<Tuple<string, ADDomain, string>> ADSearchContainers { get; set; }
        public List<string> ADForestServers { get; set; }

        #endregion

        #region Proxy
        public string ProxyAddress { get; set; }
        public int ProxyPort { get; set; }
        public string ProxyUsername { get; set; }
        [DataType(DataType.Password)]
        public string ProxyPassword { get; set; }
        #endregion

        public ScheduledTaskStatus UpdateRunningStatus { get; set; }
        public DateTime? UpdateNextScheduled { get; set; }
        public UpdateResponse UpdateLatestResponse { get; set; }
        public bool UpdateBetaDeployment { get; set; }

        public static IndexModel FromConfiguration(SystemConfiguration config)
        {
            var m = new IndexModel()
            {
                DiscoVersion = typeof(DiscoApplication).Assembly.GetName().Version,
                DataStoreLocation = config.DataStoreLocation,
                ProxyAddress = config.ProxyAddress,
                ProxyPort = config.ProxyPort,
                ProxyUsername = config.ProxyUsername,
                ProxyPassword = config.ProxyPassword,
                UpdateLatestResponse = config.UpdateLastCheck,
                UpdateRunningStatus = Disco.BI.Interop.Community.UpdateCheckTask.RunningStatus,
                UpdateNextScheduled = Disco.BI.Interop.Community.UpdateCheckTask.NextScheduled,
                UpdateBetaDeployment = config.UpdateBetaDeployment
            };

            // AD
            m.ADDomains = ActiveDirectory.Context.Domains.ToList();
            m.ADPrimaryDomain = ActiveDirectory.Context.PrimaryDomain;
            m.ADSite = ActiveDirectory.Context.Site;
            m.ADServers = ActiveDirectory.Context.Domains.SelectMany(d => d.DomainControllers).ToList();
            var configSearchContainers = config.ActiveDirectory.SearchContainers;
            m.ADSearchContainers = configSearchContainers == null ? null : configSearchContainers.SelectMany(d => d.Value, (k, c) =>
            {
                var domain = ActiveDirectory.Context.GetDomainByName(k.Key);
                return Tuple.Create(c, domain, domain.FriendlyDistinguishedNamePath(c));
            }).ToList();

            var loadForestServersTask = ADDiscoverForestServers.LoadForestServersAsync();
            if (loadForestServersTask.Wait(TimeSpan.FromSeconds(1)))
            {
                m.ADForestServers = loadForestServersTask.Result;
                var configValue = config.ActiveDirectory.SearchAllForestServers ?? true;
                m.ADSearchAllForestServers = configValue && m.ADForestServers.Count <= ActiveDirectory.MaxForestServerSearch;
            }
            else
            {
                m.ADForestServers = null;
                m.ADSearchAllForestServers = config.ActiveDirectory.SearchAllForestServers ?? true;
            }

            return m;
        }
    }
}