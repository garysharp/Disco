using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.BI;
using Disco.BI.Extensions;
using Disco.Models.Repository;
using Disco.Web.Extensions;
using System.Drawing;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class DeviceBatchController : dbAdminController
    {

        const string pName = "name";
        const string pPurchaseDate = "purchasedate";
        const string pSupplier = "supplier";
        const string pPurchaseDetails = "purchasedetails";
        const string pUnitCost = "unitcost";
        const string pUnitQuantity = "unitquantity";
        const string pDefaultDeviceModelId = "defaultdevicemodelid";
        const string pWarrantyValidUntil = "warrantyvaliduntil";
        const string pWarrantyDetails = "warrantydetails";
        const string pInsuredDate = "insureddate";
        const string pInsuranceSupplier = "insurancesupplier";
        const string pInsuredUntil = "insureduntil";
        const string pInsuranceDetails = "insurancedetails";
        const string pComments = "comments";

        public virtual ActionResult Update(int id, string key, string value = null, bool redirect = false)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");
                var deviceBatch = dbContext.DeviceBatches.Find(id);
                if (deviceBatch != null)
                {
                    switch (key.ToLower())
                    {
                        case pName:
                            UpdateName(deviceBatch, value);
                            break;
                        case pPurchaseDate:
                            UpdatePurchaseDate(deviceBatch, value);
                            break;
                        case pSupplier:
                            UpdateSupplier(deviceBatch, value);
                            break;
                        case pPurchaseDetails:
                            UpdatePurchaseDetails(deviceBatch, value);
                            break;
                        case pUnitCost:
                            UpdateUnitCost(deviceBatch, value);
                            break;
                        case pUnitQuantity:
                            UpdateUnitQuantity(deviceBatch, value);
                            break;
                        case pDefaultDeviceModelId:
                            UpdateDefaultDeviceModelId(deviceBatch, value);
                            break;
                        case pWarrantyValidUntil:
                            UpdateWarrantyValidUntil(deviceBatch, value);
                            break;
                        case pWarrantyDetails:
                            UpdateWarrantyDetails(deviceBatch, value);
                            break;
                        case pInsuredDate:
                            UpdateInsuredDate(deviceBatch, value);
                            break;
                        case pInsuranceSupplier:
                            UpdateInsuranceSupplier(deviceBatch, value);
                            break;
                        case pInsuredUntil:
                            UpdateInsuredUntil(deviceBatch, value);
                            break;
                        case pInsuranceDetails:
                            UpdateInsuranceDetails(deviceBatch, value);
                            break;
                        case pComments:
                            UpdateComments(deviceBatch, value);
                            break;
                        default:
                            throw new Exception("Invalid Update Key");
                    }
                }
                else
                {
                    return Json("Invalid Device Batch Id", JsonRequestBehavior.AllowGet);
                }
                if (redirect)
                    return RedirectToAction(MVC.Config.DeviceBatch.Index(deviceBatch.Id));
                else
                    return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (redirect)
                    throw;
                else
                    return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        #region Update Shortcut Methods
        public virtual ActionResult UpdateName(int id, string BatchName = null, bool redirect = false)
        {
            return Update(id, pName, BatchName, redirect);
        }
        public virtual ActionResult UpdatePurchaseDate(int id, string PurchaseDate = null, bool redirect = false)
        {
            return Update(id, pPurchaseDate, PurchaseDate, redirect);
        }
        public virtual ActionResult UpdateSupplier(int id, string Supplier = null, bool redirect = false)
        {
            return Update(id, pSupplier, Supplier, redirect);
        }
        [ValidateInput(false)]
        public virtual ActionResult UpdatePurchaseDetails(int id, string PurchaseDetails = null, bool redirect = false)
        {
            return Update(id, pPurchaseDetails, PurchaseDetails, redirect);
        }
        public virtual ActionResult UpdateUnitCost(int id, string UnitCost = null, bool redirect = false)
        {
            return Update(id, pUnitCost, UnitCost, redirect);
        }
        public virtual ActionResult UpdateUnitQuantity(int id, string UnitQuantity = null, bool redirect = false)
        {
            return Update(id, pUnitQuantity, UnitQuantity, redirect);
        }
        public virtual ActionResult UpdateDefaultDeviceModelId(int id, string DefaultDeviceModelId = null, bool redirect = false)
        {
            return Update(id, pDefaultDeviceModelId, DefaultDeviceModelId, redirect);
        }
        public virtual ActionResult UpdateWarrantyValidUntil(int id, string WarrantyValidUntil = null, bool redirect = false)
        {
            return Update(id, pWarrantyValidUntil, WarrantyValidUntil, redirect);
        }
        [ValidateInput(false)]
        public virtual ActionResult UpdateWarrantyDetails(int id, string WarrantyDetails = null, bool redirect = false)
        {
            return Update(id, pWarrantyDetails, WarrantyDetails, redirect);
        }
        public virtual ActionResult UpdateInsuredDate(int id, string InsuredDate = null, bool redirect = false)
        {
            return Update(id, pInsuredDate, InsuredDate, redirect);
        }
        public virtual ActionResult UpdateInsuranceSupplier(int id, string InsuranceSupplier = null, bool redirect = false)
        {
            return Update(id, pInsuranceSupplier, InsuranceSupplier, redirect);
        }
        public virtual ActionResult UpdateInsuredUntil(int id, string InsuredUntil = null, bool redirect = false)
        {
            return Update(id, pInsuredUntil, InsuredUntil, redirect);
        }
        [ValidateInput(false)]
        public virtual ActionResult UpdateInsuranceDetails(int id, string InsuranceDetails = null, bool redirect = false)
        {
            return Update(id, pInsuranceDetails, InsuranceDetails, redirect);
        }
        [ValidateInput(false)]
        public virtual ActionResult UpdateComments(int id, string Comments = null, bool redirect = false)
        {
            return Update(id, pComments, Comments, redirect);
        }
        #endregion

        #region Update Properties
        private void UpdateName(DeviceBatch deviceBatch, string Name)
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentNullException("Name", "Device Batch Name is required");
            else
            {
                // Check for Duplicates
                var d = dbContext.DeviceBatches.Where(db => db.Id != deviceBatch.Id && db.Name == Name).Count();
                if (d > 0)
                {
                    throw new Exception("A Device Batch with that name already exists");
                }
                deviceBatch.Name = Name;
            }
            dbContext.SaveChanges();
        }
        private void UpdatePurchaseDate(DeviceBatch deviceBatch, string PurchaseDate)
        {
            if (string.IsNullOrEmpty(PurchaseDate))
                throw new ArgumentNullException("PurchaseDate", "A Device Batch Purchase Date is required");
            else
            {
                DateTime ecd;
                if (DateTime.TryParse(PurchaseDate, out ecd))
                {
                    deviceBatch.PurchaseDate = ecd.Date;
                }
                else
                {
                    throw new Exception("Invalid Date Format");
                }
            }
            dbContext.SaveChanges();
        }
        private void UpdateSupplier(DeviceBatch deviceBatch, string Supplier)
        {
            if (string.IsNullOrWhiteSpace(Supplier))
                deviceBatch.Supplier = null;
            else
                deviceBatch.Supplier = Supplier;
            dbContext.SaveChanges();
        }
        private void UpdatePurchaseDetails(DeviceBatch deviceBatch, string PurchaseDetails)
        {
            if (string.IsNullOrWhiteSpace(PurchaseDetails))
                deviceBatch.PurchaseDetails = null;
            else
                deviceBatch.PurchaseDetails = PurchaseDetails;
            dbContext.SaveChanges();
        }
        private void UpdateUnitCost(DeviceBatch deviceBatch, string UnitCost)
        {
            if (string.IsNullOrWhiteSpace(UnitCost))
                deviceBatch.UnitCost = null;
            else
            {
                decimal unitCost;
                if (decimal.TryParse(UnitCost, out unitCost))
                {
                    deviceBatch.UnitCost = unitCost;
                }
                else
                {
                    throw new Exception("Invalid Currency Format");
                }
            }
            dbContext.SaveChanges();
        }
        private void UpdateUnitQuantity(DeviceBatch deviceBatch, string UnitQuantity)
        {
            if (string.IsNullOrWhiteSpace(UnitQuantity))
                deviceBatch.UnitQuantity = null;
            else
            {
                int unitQuantity;
                if (int.TryParse(UnitQuantity, out unitQuantity))
                {
                    deviceBatch.UnitQuantity = unitQuantity;
                }
                else
                {
                    throw new Exception("Invalid Number");
                }
            }
            dbContext.SaveChanges();
        }
        private void UpdateDefaultDeviceModelId(DeviceBatch deviceBatch, string DefaultDeviceModelId)
        {
            if (!string.IsNullOrEmpty(DefaultDeviceModelId))
            {
                int bId;
                if (int.TryParse(DefaultDeviceModelId, out bId))
                {
                    var dm = dbContext.DeviceModels.Find(bId);
                    if (dm != null)
                    {
                        deviceBatch.DefaultDeviceModelId = dm.Id;
                        deviceBatch.DefaultDeviceModel = dm;

                        dbContext.SaveChanges();
                        return;
                    }
                }
            }
            else
            {
                // Null Id - No Batch
                deviceBatch.DefaultDeviceModelId = null;
                deviceBatch.DefaultDeviceModel = null;

                dbContext.SaveChanges();
                return;
            }
            throw new Exception("Invalid Device Model Id");
        }
        private void UpdateWarrantyValidUntil(DeviceBatch deviceBatch, string WarrantyValidUntil)
        {
            if (string.IsNullOrEmpty(WarrantyValidUntil))
                deviceBatch.WarrantyValidUntil = null;
            else
            {
                DateTime ecd;
                if (DateTime.TryParse(WarrantyValidUntil, out ecd))
                {
                    deviceBatch.WarrantyValidUntil = ecd.Date;
                }
                else
                {
                    throw new Exception("Invalid Date Format");
                }
            }
            dbContext.SaveChanges();
        }
        private void UpdateWarrantyDetails(DeviceBatch deviceBatch, string WarrantyDetails)
        {
            if (string.IsNullOrWhiteSpace(WarrantyDetails))
                deviceBatch.WarrantyDetails = null;
            else
                deviceBatch.WarrantyDetails = WarrantyDetails;
            dbContext.SaveChanges();
        }
        private void UpdateInsuredDate(DeviceBatch deviceBatch, string InsuredDate)
        {
            if (string.IsNullOrEmpty(InsuredDate))
                deviceBatch.InsuredDate = null;
            else
            {
                DateTime ecd;
                if (DateTime.TryParse(InsuredDate, out ecd))
                {
                    deviceBatch.InsuredDate = ecd.Date;
                }
                else
                {
                    throw new Exception("Invalid Date Format");
                }
            }
            dbContext.SaveChanges();
        }
        private void UpdateInsuranceSupplier(DeviceBatch deviceBatch, string InsuranceSupplier)
        {
            if (string.IsNullOrWhiteSpace(InsuranceSupplier))
                deviceBatch.InsuranceSupplier = null;
            else
                deviceBatch.InsuranceSupplier = InsuranceSupplier;
            dbContext.SaveChanges();
        }
        private void UpdateInsuredUntil(DeviceBatch deviceBatch, string InsuredUntil)
        {
            if (string.IsNullOrEmpty(InsuredUntil))
                deviceBatch.InsuredUntil = null;
            else
            {
                DateTime ecd;
                if (DateTime.TryParse(InsuredUntil, out ecd))
                {
                    deviceBatch.InsuredUntil = ecd.Date;
                }
                else
                {
                    throw new Exception("Invalid Date Format");
                }
            }
            dbContext.SaveChanges();
        }
        private void UpdateInsuranceDetails(DeviceBatch deviceBatch, string InsuranceDetails)
        {
            if (string.IsNullOrWhiteSpace(InsuranceDetails))
                deviceBatch.InsuranceDetails = null;
            else
                deviceBatch.InsuranceDetails = InsuranceDetails;
            dbContext.SaveChanges();
        }
        private void UpdateComments(DeviceBatch deviceBatch, string Comments)
        {
            if (string.IsNullOrWhiteSpace(Comments))
                deviceBatch.Comments = null;
            else
                deviceBatch.Comments = Comments;
            dbContext.SaveChanges();
        }
        #endregion

        #region Actions

        public virtual ActionResult Delete(int id, Nullable<bool> redirect = false)
        {
            try
            {
                var db = dbContext.DeviceBatches.Find(id);
                if (db != null)
                {
                    db.Delete(dbContext);
                    dbContext.SaveChanges();
                    if (redirect.HasValue && redirect.Value)
                        return RedirectToAction(MVC.Config.DeviceBatch.Index(null));
                    else
                        return Json("OK", JsonRequestBehavior.AllowGet);
                }
                throw new Exception("Invalid Device Batch Number");
            }
            catch (Exception ex)
            {
                if (redirect.HasValue && redirect.Value)
                    throw;
                else
                    return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Index
        public virtual ActionResult Index(int? id)
        {
            if (id.HasValue)
            {
                dbContext.Configuration.ProxyCreationEnabled = false;
                DeviceBatch deviceBatch = dbContext.DeviceBatches.FirstOrDefault(db => db.Id == id);
                return Json(deviceBatch, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var deviceBatches = dbContext.DeviceBatches.ToArray();
                return Json(deviceBatches, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Timeline
        public virtual ActionResult Timeline()
        {

            var batchesInformation = dbContext.DeviceBatches.Select(db => new
            {
                Name = db.Name,
                Comments = db.Comments,
                PurchaseDate = db.PurchaseDate,
                WarrantyValidUntil = db.WarrantyValidUntil,
                DeviceCount = db.Devices.Count(),
                DefaultModelId = db.DefaultDeviceModelId,
                DefaultModelDescription = db.DefaultDeviceModel.Description
            }).ToArray();

            Color warningColour = Color.FromArgb(225, 70, 0);
            Color normalColour = Color.FromArgb(88, 160, 220);
            Color highlightColour = Color.FromArgb(0, 140, 0);

            double mostDevices = batchesInformation.Max(bi => bi.DeviceCount);
            var events = new List<Models.DeviceBatch.DeviceBatchTimelineEvent>();

            foreach (var bi in batchesInformation)
            {
                var color = warningColour; // No Devices
                if (bi.DeviceCount > 0)
                    color = normalColour.InterpolateColours(highlightColour, bi.DeviceCount / mostDevices);

                events.Add(new Models.DeviceBatch.DeviceBatchTimelineEvent()
                {
                    start = bi.PurchaseDate,
                    end = bi.WarrantyValidUntil,
                    caption = bi.DefaultModelDescription,
                    title = string.Format("{0} [{1} x{2}]", bi.Name, bi.DefaultModelDescription, bi.DeviceCount),
                    textColor = "#000",
                    description = bi.Comments ?? string.Empty,
                    color = ColorTranslator.ToHtml(color),
                    image = Url.Action(MVC.API.DeviceModel.Image(bi.DefaultModelId)),
                    link = Url.Action(MVC.Config.DeviceBatch.Index(bi.DefaultModelId))
                });
            }

            return this.JsonNet(new Models.DeviceBatch.DeviceBatchTimelineEventSource() { events = events.ToArray() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}
