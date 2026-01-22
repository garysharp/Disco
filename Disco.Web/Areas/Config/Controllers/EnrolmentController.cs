using Disco.Models.UI.Config.Enrolment;
using Disco.Services.Authorization;
using Disco.Services.Devices.Enrolment;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Interop.DNS;
using Disco.Services.Interop.VicEduDept;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Web;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class EnrolmentController : AuthorizedDatabaseController
    {
        [DiscoAuthorize(Claims.Config.Enrolment.Show)]
        public virtual ActionResult Index()
        {
            var serverUrl = Request.Url;
            if ((serverUrl.HostNameType == UriHostNameType.Dns && serverUrl.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)) ||
                serverUrl.HostNameType == UriHostNameType.IPv4 || serverUrl.HostNameType == UriHostNameType.IPv6)
            {
                serverUrl = new UriBuilder(serverUrl)
                {
                    Host = Environment.MachineName
                }.Uri;
            }

            var srvRecord = DnsService.Query<SrvDnsRecord>(WindowsDeviceEnrolment.GetDnsServiceLocationRecordName(), true).FirstOrDefault();
            var srvValue = srvRecord == null ? null : (srvRecord.Port == 443 ? srvRecord.Target : $"{srvRecord.Target}:{srvRecord.Port}");

            var m = new Models.Enrolment.IndexModel()
            {
                MacSshUsername = Database.DiscoConfiguration.Bootstrapper.MacSshUsername,
                PendingTimeoutMinutes = (int)Database.DiscoConfiguration.Bootstrapper.PendingTimeout.TotalMinutes,
                MacEnrolUrl = new Uri(serverUrl, Url.Action(MVC.Services.Client.Unauthenticated("MacSecureEnrol"))),
                HostingPluginInstalled = Plugins.PluginInstalled("Hosting"),
                IsServicesEducationVicGovAuDomain = ActiveDirectory.Context.PrimaryDomain.Name.Equals("services.education.vic.gov.au", StringComparison.OrdinalIgnoreCase),
                IsVicSmartDeployment = VicSmart.IsVicSmartDeployment(),
                DnsSrvRecordName = WindowsDeviceEnrolment.GetDnsServiceLocationRecordName(),
                DnsSrvRecordValue = srvValue,
                LegacyDiscoveryEnabled = !Database.DiscoConfiguration.Devices.EnrollmentLegacyDiscoveryDisabled,
            };

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigEnrolmentIndexModel>(ControllerContext, m);

            return View(m);
        }

        [DiscoAuthorize(Claims.Config.Enrolment.ShowStatus)]
        public virtual ActionResult Status()
        {
            var m = new Models.Enrolment.StatusModel();

            m.DefaultDeviceProfileId = Database.DiscoConfiguration.DeviceProfiles.DefaultDeviceProfileId;
            m.DeviceProfiles = Database.DeviceProfiles.ToList();
            m.DeviceBatches = Database.DeviceBatches.ToList();

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigEnrolmentStatusModel>(ControllerContext, m);

            return View(m);
        }

    }
}
