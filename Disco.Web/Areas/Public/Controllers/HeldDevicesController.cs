using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Jobs.Noticeboards;
using Disco.Services.Web;
using Disco.Web.Areas.Public.Models.UserHeldDevices;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.Public.Controllers
{
    public partial class HeldDevicesController : DatabaseController
    {
        public virtual ActionResult Index(string DeviceProfileInclude, string DeviceProfileExclude, string DeviceAddressInclude, string DeviceAddressExclude, string JobQueueInclude, string JobQueueExclude)
        {
            var query = FilterJobs(Database.Jobs, Database, DeviceProfileInclude, DeviceProfileExclude, DeviceAddressInclude, DeviceAddressExclude, JobQueueInclude, JobQueueExclude);

            var m = Disco.Services.Jobs.Noticeboards.HeldDevices.GetHeldDevices(query);

            return View(m);
        }

        internal static IQueryable<Job> FilterJobs(IQueryable<Job> query, DiscoDataContext database, string deviceProfileInclude, string deviceProfileExclude, string deviceAddressInclude, string deviceAddressExclude, string jobQueueInclude, string jobQueueExclude)
        {
            if (!string.IsNullOrWhiteSpace(deviceProfileInclude))
            {
                var include = deviceProfileInclude.Split(',').Select(int.Parse).ToList();
                query = query.Where(j => include.Contains(j.Device.DeviceProfileId));
            }
            if (!string.IsNullOrWhiteSpace(deviceProfileExclude))
            {
                var exclude = deviceProfileExclude.Split(',').Select(int.Parse).ToList();
                query = query.Where(j => !exclude.Contains(j.Device.DeviceProfileId));
            }
            if (!string.IsNullOrWhiteSpace(deviceAddressInclude))
            {
                var include = deviceAddressInclude.Split(',');
                var addressIds = database.DiscoConfiguration.OrganisationAddresses.Addresses.Where(a => include.Contains(a.ShortName)).Select(a => a.Id).ToList();
                query = query.Where(j => addressIds.Contains(j.Device.DeviceProfile.DefaultOrganisationAddress));
            }
            if (!string.IsNullOrWhiteSpace(deviceAddressExclude))
            {
                var exclude = deviceAddressExclude.Split(',');
                var addressIds = database.DiscoConfiguration.OrganisationAddresses.Addresses.Where(a => exclude.Contains(a.ShortName)).Select(a => (int?)a.Id).ToList();
                query = query.Where(j => j.Device.DeviceProfile.DefaultOrganisationAddress == null || !addressIds.Contains(j.Device.DeviceProfile.DefaultOrganisationAddress));
            }
            if (jobQueueInclude != null)
            {
                var include = jobQueueInclude.Split(',').Select(int.Parse).ToList();
                query = query.Where(j => j.JobQueues.Any(q => q.RemovedDate == null && include.Contains(q.JobQueueId)));
            }
            if (jobQueueExclude != null)
            {
                var exclude = jobQueueExclude.Split(',').Select(int.Parse).ToList();
                query = query.Where(j => !j.JobQueues.Any(q => q.RemovedDate == null && exclude.Contains(q.JobQueueId)));
            }

            return query;
        }

        public virtual ActionResult ReadyForReturnXml()
        {
            var readyForReturn = Disco.Services.Jobs.Noticeboards.HeldDevices.GetHeldDevices(Database)
                .Where(j => j.ReadyForReturn && !j.WaitingForUserAction).Cast<HeldDeviceItem>().ToArray();

            return new Extensions.XmlResult(readyForReturn);
        }
        public virtual ActionResult WaitingForUserActionXml()
        {
            var waitingForUserAction = Disco.Services.Jobs.Noticeboards.HeldDevices.GetHeldDevices(Database)
                .Where(j => j.WaitingForUserAction).Cast<HeldDeviceItem>().ToArray();

            return new Extensions.XmlResult(waitingForUserAction);
        }
        public virtual ActionResult HeldDevicesXml()
        {
            var heldDevices = Disco.Services.Jobs.Noticeboards.HeldDevices.GetHeldDevices(Database)
                .Where(j => !j.ReadyForReturn && !j.WaitingForUserAction).Cast<HeldDeviceItem>().ToArray();

            return new Extensions.XmlResult(heldDevices);
        }

        public virtual ActionResult Noticeboard()
        {
            var model = new NoticeboardModel()
            {
                DefaultTheme = Database.DiscoConfiguration.JobPreferences.DefaultNoticeboardTheme
            };

            return View(model);
        }

        public virtual ActionResult HeldDevice(string id)
        {
            var m = Disco.Services.Jobs.Noticeboards.HeldDevices.GetHeldDevice(Database, id);

            return Json(m, JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult HeldDevices()
        {
            var m = Disco.Services.Jobs.Noticeboards.HeldDevices.GetHeldDevices(Database);

            return Json(m, JsonRequestBehavior.AllowGet);
        }
    }
}
