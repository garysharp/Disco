using Disco.Services.Jobs.Noticeboards;
using Disco.Services.Web;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.Public.Controllers
{
    public partial class UserHeldDevicesController : DatabaseController
    {
        public virtual ActionResult Index()
        {
            var m = Disco.Services.Jobs.Noticeboards.HeldDevicesForUsers.GetHeldDevicesForUsers(Database);

            return View(m);
        }

        public virtual ActionResult ReadyForReturnXml()
        {
            var readyForReturn = Disco.Services.Jobs.Noticeboards.HeldDevicesForUsers.GetHeldDevicesForUsers(Database)
                .Where(j => j.ReadyForReturn && !j.WaitingForUserAction).Cast<HeldDeviceItem>().ToArray();

            return new Extensions.XmlResult(readyForReturn);
        }
        public virtual ActionResult WaitingForUserActionXml()
        {
            var userHeldDevices = Disco.Services.Jobs.Noticeboards.HeldDevicesForUsers.GetHeldDevicesForUsers(Database)
                .Where(j => j.WaitingForUserAction).Cast<HeldDeviceItem>().ToArray();

            return new Extensions.XmlResult(userHeldDevices);
        }
        public virtual ActionResult UserHeldDevicesXml()
        {
            var userHeldDevices = Disco.Services.Jobs.Noticeboards.HeldDevicesForUsers.GetHeldDevicesForUsers(Database)
                .Where(j => !j.ReadyForReturn && !j.WaitingForUserAction).Cast<HeldDeviceItem>().ToArray();

            return new Extensions.XmlResult(userHeldDevices);
        }

        public virtual ActionResult Noticeboard()
        {
            return View();
        }

        public virtual ActionResult UserHeldDevice(string id)
        {
            var m = Disco.Services.Jobs.Noticeboards.HeldDevicesForUsers.GetHeldDeviceForUsers(Database, id);

            return Json(m, JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult UserHeldDevices()
        {
            var m = Disco.Services.Jobs.Noticeboards.HeldDevicesForUsers.GetHeldDevicesForUsers(Database);

            return Json(m, JsonRequestBehavior.AllowGet);
        }
    }
}
