using Disco.Services.Jobs.Noticeboards;
using Disco.Services.Web;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.Public.Controllers
{
    public partial class HeldDevicesController : DatabaseController
    {
        public virtual ActionResult Index()
        {
            var m = Disco.Services.Jobs.Noticeboards.HeldDevices.GetHeldDevices(Database);

            return View(m);
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
            return View();
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
