using Disco.Services.Jobs.Noticeboards;
using Disco.Services.Web;
using Disco.Web.Areas.Public.Models.UserHeldDevices;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.Public.Controllers
{
    public partial class UserHeldDevicesController : DatabaseController
    {
        public virtual ActionResult Index(string DeviceProfileInclude, string DeviceProfileExclude, string DeviceAddressInclude, string DeviceAddressExclude, string JobQueueInclude, string JobQueueExclude)
        {
            var query = HeldDevicesController.FilterJobs(Database.Jobs, Database, DeviceProfileInclude, DeviceProfileExclude, DeviceAddressInclude, DeviceAddressExclude, JobQueueInclude, JobQueueExclude);

            var m = HeldDevicesForUsers.GetHeldDevicesForUsers(query);

            return View(m);
        }

        public virtual ActionResult ReadyForReturnXml()
        {
            var readyForReturn = HeldDevicesForUsers.GetHeldDevicesForUsers(Database)
                .Where(j => j.ReadyForReturn && !j.WaitingForUserAction).Cast<HeldDeviceItem>().ToArray();

            return new Extensions.XmlResult(readyForReturn);
        }
        public virtual ActionResult WaitingForUserActionXml()
        {
            var userHeldDevices = HeldDevicesForUsers.GetHeldDevicesForUsers(Database)
                .Where(j => j.WaitingForUserAction).Cast<HeldDeviceItem>().ToArray();

            return new Extensions.XmlResult(userHeldDevices);
        }
        public virtual ActionResult UserHeldDevicesXml()
        {
            var userHeldDevices = HeldDevicesForUsers.GetHeldDevicesForUsers(Database)
                .Where(j => !j.ReadyForReturn && !j.WaitingForUserAction).Cast<HeldDeviceItem>().ToArray();

            return new Extensions.XmlResult(userHeldDevices);
        }

        public virtual ActionResult Noticeboard()
        {
            var model = new NoticeboardModel()
            {
                DefaultTheme = Database.DiscoConfiguration.JobPreferences.DefaultNoticeboardTheme
            };

            return View(model);
        }

        public virtual ActionResult UserHeldDevice(string id)
        {
            var m = HeldDevicesForUsers.GetHeldDeviceForUsers(Database, id);

            return Json(m, JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult UserHeldDevices()
        {
            var m = HeldDevicesForUsers.GetHeldDevicesForUsers(Database);

            return Json(m, JsonRequestBehavior.AllowGet);
        }
    }
}
