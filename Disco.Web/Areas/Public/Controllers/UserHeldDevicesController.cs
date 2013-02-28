using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.BI;
using Disco.BI.Extensions;

namespace Disco.Web.Areas.Public.Controllers
{
    public partial class UserHeldDevicesController : dbController
    {
        private List<Models.UserHeldDevices.UserHeldDeviceModel> GetUserHeldDevices()
        {
            var usersJobs = dbContext.Jobs.Where(j =>
                !j.ClosedDate.HasValue && j.UserId != null &&
                j.DeviceSerialNumber != null &&
                ((j.DeviceHeld.HasValue && !j.DeviceReturnedDate.HasValue) || j.WaitingForUserAction.HasValue)
                ).Select(j => new Models.UserHeldDevices.HeldJobDeviceModel
            {
                JobId = j.Id,
                WaitingForUserAction = j.WaitingForUserAction.HasValue || ((j.JobMetaNonWarranty.AccountingChargeRequiredDate.HasValue || j.JobMetaNonWarranty.AccountingChargeAddedDate.HasValue) && !j.JobMetaNonWarranty.AccountingChargePaidDate.HasValue),
                WaitingForUserActionSince = j.WaitingForUserAction.HasValue ? j.WaitingForUserAction : (j.JobMetaNonWarranty.AccountingChargeRequiredDate.HasValue ? j.JobMetaNonWarranty.AccountingChargeRequiredDate : j.JobMetaNonWarranty.AccountingChargeAddedDate),
                ReadyForReturn = j.DeviceReadyForReturn.HasValue,
                EstimatedReturnTime = j.ExpectedClosedDate,
                ReadyForReturnSince = j.DeviceReadyForReturn,
                UserDisplayName = j.User.DisplayName,
                UserId = j.UserId,
                DeviceProfileId = j.Device.DeviceProfileId,
                DeviceAddressId = j.Device.DeviceProfile.DefaultOrganisationAddress
            }).GroupBy(j => j.UserId);

            var thd = new List<Models.UserHeldDevices.UserHeldDeviceModel>();
            foreach (var userJobs in usersJobs)
            {
                if (userJobs.Any(j => j.WaitingForUserAction))
                {
                    thd.Add(userJobs.Where(j => j.WaitingForUserAction).OrderBy(j => j.WaitingForUserActionSince).FirstOrDefault().ToUserHeldDeviceModel(dbContext));
                }
                else
                {
                    if (userJobs.All(j => j.ReadyForReturn))
                    {
                        thd.Add(userJobs.FirstOrDefault().ToUserHeldDeviceModel(dbContext));
                    }
                    else
                    {
                        thd.Add(userJobs.Where(j => !j.ReadyForReturn).OrderByDescending(j => j.EstimatedReturnTime).FirstOrDefault().ToUserHeldDeviceModel(dbContext));
                    }
                }
            }
            return thd;
        }
        private Models.UserHeldDevices.UserHeldDeviceModel GetUserHeldDevice(string userId)
        {
            var userJobs = dbContext.Jobs.Where(j => !j.ClosedDate.HasValue && j.UserId == userId && j.DeviceSerialNumber != null && ((j.DeviceHeld.HasValue && !j.DeviceReturnedDate.HasValue) || j.WaitingForUserAction.HasValue)).Select(j => new Models.UserHeldDevices.HeldJobDeviceModel
            {
                JobId = j.Id,
                WaitingForUserAction = j.WaitingForUserAction.HasValue,
                WaitingForUserActionSince = j.WaitingForUserAction,
                ReadyForReturn = j.DeviceReadyForReturn.HasValue,
                EstimatedReturnTime = j.ExpectedClosedDate,
                ReadyForReturnSince = j.DeviceReadyForReturn,
                UserDisplayName = j.User.DisplayName,
                UserId = j.UserId,
                DeviceProfileId = j.Device.DeviceProfileId,
                DeviceAddressId = j.Device.DeviceProfile.DefaultOrganisationAddress
            }).ToList();

            if (userJobs == null || userJobs.Count == 0)
            {
                return null;
            }
            else
            {
                if (userJobs.Any(j => j.WaitingForUserAction))
                {
                    return userJobs.Where(j => j.WaitingForUserAction).OrderBy(j => j.WaitingForUserActionSince).FirstOrDefault().ToUserHeldDeviceModel(dbContext);
                }
                else
                {
                    if (userJobs.All(j => j.ReadyForReturn))
                    {
                        return userJobs.FirstOrDefault().ToUserHeldDeviceModel(dbContext);
                    }
                    else
                    {
                        return userJobs.Where(j => !j.ReadyForReturn).OrderByDescending(j => j.EstimatedReturnTime).FirstOrDefault().ToUserHeldDeviceModel(dbContext);
                    }
                }
            }
        }

        public virtual ActionResult Index()
        {
            return View(GetUserHeldDevices());
        }

        public virtual ActionResult ReadyForReturnXml()
        {
            var readyForReturn = GetUserHeldDevices().Where(j => j.ReadyForReturn && !j.WaitingForUserAction).ToArray();
            return new Extensions.XmlResult(readyForReturn);
        }
        public virtual ActionResult WaitingForUserActionXml()
        {
            var userHeldDevices = GetUserHeldDevices().Where(j => j.WaitingForUserAction).ToArray();
            return new Extensions.XmlResult(userHeldDevices);
        }
        public virtual ActionResult UserHeldDevicesXml()
        {
            var userHeldDevices = GetUserHeldDevices().Where(j => !j.ReadyForReturn && !j.WaitingForUserAction).ToArray();
            return new Extensions.XmlResult(userHeldDevices);
        }

        public virtual ActionResult Noticeboard()
        {
            return View();
        }

        public virtual ActionResult UserHeldDevice(string id)
        {
            var uhd = GetUserHeldDevice(id);
            return Json(uhd, JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult UserHeldDevices()
        {
            var uhd = GetUserHeldDevices();
            return Json(uhd, JsonRequestBehavior.AllowGet);
        }

    }
}
