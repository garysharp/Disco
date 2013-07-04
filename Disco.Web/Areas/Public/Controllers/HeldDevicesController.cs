using Disco.Data.Repository;
using Disco.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Disco.Web.Areas.Public.Controllers
{
    public partial class HeldDevicesController : dbController
    {
        #region Helpers

        private List<Models.HeldDevices.HeldDeviceModel> GetHeldDevices(IQueryable<Job> query)
        {
            var jobs = query.Where(j =>
                !j.ClosedDate.HasValue &&
                j.DeviceSerialNumber != null &&
                ((j.DeviceHeld.HasValue && !j.DeviceReturnedDate.HasValue) || j.WaitingForUserAction.HasValue)
                ).Select(j => new Models.HeldDevices.HeldDeviceQueryModel
                {
                    JobId = j.Id,
                    DeviceSerialNumber = j.DeviceSerialNumber,
                    DeviceComputerName = j.Device.ComputerName,
                    DeviceLocation = j.Device.Location,
                    DeviceProfileId = j.Device.DeviceProfileId,
                    DeviceAddressId = j.Device.DeviceProfile.DefaultOrganisationAddress,
                    UserId = j.Device.AssignedUserId,
                    UserDisplayName = j.Device.AssignedUser.DisplayName,
                    WaitingForUserAction = j.WaitingForUserAction.HasValue || ((j.JobMetaNonWarranty.AccountingChargeRequiredDate.HasValue || j.JobMetaNonWarranty.AccountingChargeAddedDate.HasValue) && !j.JobMetaNonWarranty.AccountingChargePaidDate.HasValue),
                    WaitingForUserActionSince = j.WaitingForUserAction.HasValue ? j.WaitingForUserAction : (j.JobMetaNonWarranty.AccountingChargeRequiredDate.HasValue ? j.JobMetaNonWarranty.AccountingChargeRequiredDate : j.JobMetaNonWarranty.AccountingChargeAddedDate),
                    ReadyForReturn = j.DeviceReadyForReturn.HasValue,
                    EstimatedReturnTime = j.ExpectedClosedDate,
                    ReadyForReturnSince = j.DeviceReadyForReturn
                }).GroupBy(j => j.DeviceSerialNumber);

            var thd = new List<Models.HeldDevices.HeldDeviceModel>();
            foreach (var job in jobs)
            {
                if (job.Any(j => j.WaitingForUserAction))
                {
                    thd.Add(job.Where(j => j.WaitingForUserAction).OrderBy(j => j.WaitingForUserActionSince).FirstOrDefault().ToUserHeldDeviceModel(dbContext));
                }
                else
                {
                    if (job.All(j => j.ReadyForReturn))
                    {
                        thd.Add(job.FirstOrDefault().ToUserHeldDeviceModel(dbContext));
                    }
                    else
                    {
                        thd.Add(job.Where(j => !j.ReadyForReturn).OrderByDescending(j => j.EstimatedReturnTime).FirstOrDefault().ToUserHeldDeviceModel(dbContext));
                    }
                }
            }
            return thd;
        }
        
        private List<Models.HeldDevices.HeldDeviceModel> GetHeldDevices()
        {
            return GetHeldDevices(dbContext.Jobs);
        }
        private Models.HeldDevices.HeldDeviceModel GetHeldDevice(string DeviceSerialNumber)
        {
            return GetHeldDevices(dbContext.Jobs.Where(j => j.DeviceSerialNumber == DeviceSerialNumber)).FirstOrDefault();
        }
        #endregion

        public virtual ActionResult Index()
        {
            return View(GetHeldDevices());
        }

        public virtual ActionResult ReadyForReturnXml()
        {
            var readyForReturn = GetHeldDevices().Where(j => j.ReadyForReturn && !j.WaitingForUserAction).ToArray();
            return new Extensions.XmlResult(readyForReturn);
        }
        public virtual ActionResult WaitingForUserActionXml()
        {
            var waitingForUserAction = GetHeldDevices().Where(j => j.WaitingForUserAction).ToArray();
            return new Extensions.XmlResult(waitingForUserAction);
        }
        public virtual ActionResult HeldDevicesXml()
        {
            var heldDevices = GetHeldDevices().Where(j => !j.ReadyForReturn && !j.WaitingForUserAction).ToArray();
            return new Extensions.XmlResult(heldDevices);
        }

        public virtual ActionResult Noticeboard()
        {
            return View();
        }

        public virtual ActionResult HeldDevice(string id)
        {
            var uhd = GetHeldDevice(id);
            return Json(uhd, JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult HeldDevices()
        {
            var uhd = GetHeldDevices();
            return Json(uhd, JsonRequestBehavior.AllowGet);
        }
    }
}
