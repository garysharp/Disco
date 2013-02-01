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

        public static IndexModel FromConfiguration(ConfigurationContext config)
        {
            return new IndexModel()
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
        }

        public void ToConfiguration(DiscoDataContext db)
        {
            ConfigurationContext config = db.DiscoConfiguration;
            //config.DataStoreLocation = DataStoreLocation;
            config.ProxyAddress = ProxyAddress;
            config.ProxyPort = ProxyPort;
            config.ProxyUsername = ProxyUsername;
            config.ProxyPassword = ProxyPassword;
            DiscoApplication.SetGlobalProxy(ProxyAddress, ProxyPort, ProxyUsername, ProxyPassword);

            db.SaveChanges();

            // Try and check for updates if needed - After Proxy Changed
            if (db.DiscoConfiguration.UpdateLastCheck == null
                || db.DiscoConfiguration.UpdateLastCheck.ResponseTimestamp < DateTime.Now.AddDays(-1))
            {
                Disco.BI.Interop.Community.UpdateCheckTask.ScheduleNow();
            }
        }

    }
}