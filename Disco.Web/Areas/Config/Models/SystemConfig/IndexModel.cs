﻿using Disco.Data.Configuration;
using Disco.Models.Services.Interop.DiscoServices;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Interop.DiscoServices;
using Disco.Services.Messaging;
using Disco.Services.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;

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
                        return new DateTime(2000 + (v.Build / 1000), 1, 1, v.Revision / 100, v.Revision % 100, 0, DateTimeKind.Utc)
                            .AddDays((v.Build % 1000) - 1)
                            .ToLocalTime();
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
                return DatabaseConnectionString.Value.DataSource;
            }
        }
        public string DatabaseName
        {
            get
            {
                return DatabaseConnectionString.Value.InitialCatalog;
            }
        }
        public string DatabaseAuthentication
        {
            get
            {
                return DatabaseConnectionString.Value.IntegratedSecurity ? "Integrated Authentication" : "SQL Authentication";
            }
        }
        public string DatabaseSqlAuthUsername
        {
            get
            {
                return DatabaseConnectionString.Value.IntegratedSecurity ? null : DatabaseConnectionString.Value.UserID;
            }
        }
        #endregion

        #region Active Directory

        [Display(Name="Search All Servers")]
        public bool ADSearchAllServers { get; set; }

        public List<ADDomain> ADDomains { get; set; }
        public ADDomain ADPrimaryDomain { get; set; }
        public ADSite ADSite { get; set; }
        public List<ADDomainController> ADServers { get; set; }
        public List<Tuple<string, ADDomain, string>> ADSearchContainers { get; set; }
        [Display(Name = "Search With Suffix Wildcard Only")]
        public bool ADSearchWildcardSuffixOnly { get; set; }
        public List<string> ADAllServers { get; set; }

        #endregion

        #region Proxy
        public string ProxyAddress { get; set; }
        public int ProxyPort { get; set; }
        public string ProxyUsername { get; set; }
        [DataType(DataType.Password)]
        public string ProxyPassword { get; set; }
        #endregion

        #region Email
        public string EmailSmtpServer { get; set; }
        public int EmailSmtpPort { get; set; }
        public string EmailFromAddress { get; set; }
        public string EmailReplyToAddress { get; set; }
        [Display(Name = "Enable SSL")]
        public bool EmailEnableSsl { get; set; }
        public string EmailUsername { get; set; }
        [DataType(DataType.Password)]
        public string EmailPassword { get; set; }
        public bool EmailIsConfigured { get; set; }
        #endregion

        public ScheduledTaskStatus LicenseValidationRunningStatus { get; set; }
        public string License { get; set; }
        public DateTime? LicenseExpires { get; set; }
        public string LicenseError { get; set; }

        public bool IsActivated { get; set; }
        public DateTime? ActivatedOn { get; set; }
        public string ActivatedBy { get; set; }
        public OnlineServicesConnect.ConnectionState OnlineServicesState { get; set; }

        public ScheduledTaskStatus UpdateRunningStatus { get; set; }
        public DateTime? UpdateNextScheduled { get; set; }
        public UpdateResponseV2 UpdateLatestResponse { get; set; }
        public bool UpdateAvailable { get; set; }
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
                EmailSmtpServer = config.EmailSmtpServer,
                EmailSmtpPort = config.EmailSmtpPort,
                EmailFromAddress = config.EmailFromAddress,
                EmailReplyToAddress = config.EmailReplyToAddress,
                EmailEnableSsl = config.EmailEnableSsl,
                EmailUsername = config.EmailUsername,
                EmailPassword = null,
                EmailIsConfigured = EmailService.IsConfigured,
                License = config.LicenseKey,
                LicenseExpires = config.LicenseExpiresOn,
                LicenseError = config.LicenseError,
                LicenseValidationRunningStatus = LicenseValidationTask.RunningStatus,
                IsActivated = config.IsActivated,
                ActivatedBy = config.ActivatedBy,
                ActivatedOn = config.ActivatedOn,
                OnlineServicesState = OnlineServicesConnect.State,
                UpdateLatestResponse = config.UpdateLastCheckResponse,
                UpdateRunningStatus = UpdateQueryTask.RunningStatus,
                UpdateNextScheduled = UpdateQueryTask.NextScheduled,
                UpdateBetaDeployment = config.UpdateBetaDeployment,
            };

            // Is an update available?
            m.UpdateAvailable = m.UpdateLatestResponse != null && (Version.Parse(m.UpdateLatestResponse.LatestVersion) > m.DiscoVersion);

            // AD
            m.ADDomains = ActiveDirectory.Context.Domains.ToList();
            m.ADPrimaryDomain = ActiveDirectory.Context.PrimaryDomain;
            m.ADSite = ActiveDirectory.Context.Site;
            m.ADServers = ActiveDirectory.Context.Domains.SelectMany(d => d.DomainControllers).ToList();
            m.ADSearchWildcardSuffixOnly = config.ActiveDirectory.SearchWildcardSuffixOnly;
            var configSearchContainers = config.ActiveDirectory.SearchContainers;
            m.ADSearchContainers = configSearchContainers == null ? null : configSearchContainers.SelectMany(d => d.Value, (k, c) =>
            {
                var domain = ActiveDirectory.Context.GetDomainByName(k.Key);
                return Tuple.Create(c, domain, domain.FriendlyDistinguishedNamePath(c));
            }).ToList();

            var loadAllServersTask = ADDiscoverServers.LoadAllServersAsync();
            if (loadAllServersTask.Wait(TimeSpan.FromSeconds(1)))
            {
                m.ADAllServers = loadAllServersTask.Result;
                var configValue = config.ActiveDirectory.SearchAllServers ?? true;
                m.ADSearchAllServers = configValue && m.ADAllServers.Count <= ActiveDirectory.MaxAllServerSearch;
            }
            else
            {
                m.ADAllServers = null;
                m.ADSearchAllServers = config.ActiveDirectory.SearchAllServers ?? true;
            }

            return m;
        }
    }
}