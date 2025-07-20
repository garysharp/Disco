using Disco.Models.Repository;
using Disco.Services.Jobs.Noticeboards;
using Disco.Services.Web;
using Disco.Web.Areas.Public.Models.UserHeldDevices;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.Public.Controllers
{
    public partial class UserHeldDevicesController : DatabaseController
    {
        public virtual ActionResult Index(List<int?> DeviceProfileInclude, List<int?> DeviceProfileExclude, List<string> DeviceAddressInclude, List<string> DeviceAddressExclude)
        {
            IQueryable<Job> query = Database.Jobs;

            if (DeviceProfileInclude != null)
                query = query.Where(j => DeviceProfileInclude.Contains(j.Device.DeviceProfileId));
            if (DeviceProfileExclude != null)
                query = query.Where(j => !DeviceProfileExclude.Contains(j.Device.DeviceProfileId));
            if (DeviceAddressInclude != null && DeviceAddressInclude.Count > 0)
            {
                var addressIds = Database.DiscoConfiguration.OrganisationAddresses.Addresses.Where(a => DeviceAddressInclude.Contains(a.ShortName)).Select(a => a.Id).ToList();
                query = query.Where(j => addressIds.Contains(j.Device.DeviceProfile.DefaultOrganisationAddress));
            }
            if (DeviceAddressExclude != null && DeviceAddressExclude.Count > 0)
            {
                var addressIds = Database.DiscoConfiguration.OrganisationAddresses.Addresses.Where(a => DeviceAddressExclude.Contains(a.ShortName)).Select(a => (int?)a.Id).ToList();
                query = query.Where(j => j.Device.DeviceProfile.DefaultOrganisationAddress == null || !addressIds.Contains(j.Device.DeviceProfile.DefaultOrganisationAddress));
            }

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
