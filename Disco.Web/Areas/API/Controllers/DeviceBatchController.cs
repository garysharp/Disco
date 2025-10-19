using Disco.Models.Repository;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Devices.ManagedGroups;
using Disco.Services.Interop;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Tasks;
using Disco.Services.Web;
using Disco.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class DeviceBatchController : AuthorizedDatabaseController
    {
        private const string pName = "name";
        private const string pPurchaseDate = "purchasedate";
        private const string pSupplier = "supplier";
        private const string pPurchaseDetails = "purchasedetails";
        private const string pUnitCost = "unitcost";
        private const string pUnitQuantity = "unitquantity";
        private const string pDefaultDeviceModelId = "defaultdevicemodelid";
        private const string pWarrantyValidUntil = "warrantyvaliduntil";
        private const string pWarrantyDetails = "warrantydetails";
        private const string pInsuredDate = "insureddate";
        private const string pInsuranceSupplier = "insurancesupplier";
        private const string pInsuredUntil = "insureduntil";
        private const string pInsuranceDetails = "insurancedetails";
        private const string pComments = "comments";
        private const string pDevicesLinkedGroup = "deviceslinkedgroup";
        private const string pAssignedUsersLinkedGroup = "assigneduserslinkedgroup";

        [DiscoAuthorize(Claims.Config.DeviceBatch.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Update(int id, string key, string value = null, bool redirect = false)
        {
            Authorization.Require(Claims.Config.DeviceBatch.Configure);

            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");
                var deviceBatch = Database.DeviceBatches.Find(id);
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
                        case pDevicesLinkedGroup:
                            UpdateDevicesLinkedGroup(deviceBatch, value);
                            break;
                        case pAssignedUsersLinkedGroup:
                            UpdateAssignedUsersLinkedGroup(deviceBatch, value);
                            break;
                        default:
                            throw new Exception("Invalid Update Key");
                    }
                }
                else
                {
                    return BadRequest("Invalid Device Batch Id");
                }
                if (redirect)
                    return RedirectToAction(MVC.Config.DeviceBatch.Index(deviceBatch.Id));
                else
                    return Ok();
            }
            catch (Exception ex)
            {
                if (redirect)
                    throw;
                else
                    return BadRequest(ex.Message);
            }
        }

        #region Update Shortcut Methods

        [DiscoAuthorize(Claims.Config.DeviceBatch.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateName(int id, string BatchName = null, bool redirect = false)
        {
            return Update(id, pName, BatchName, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceBatch.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdatePurchaseDate(int id, string PurchaseDate = null, bool redirect = false)
        {
            return Update(id, pPurchaseDate, PurchaseDate, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceBatch.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateSupplier(int id, string Supplier = null, bool redirect = false)
        {
            return Update(id, pSupplier, Supplier, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceBatch.Configure)]
        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public virtual ActionResult UpdatePurchaseDetails(int id, string purchaseDetails = null, bool redirect = false)
        {
            return Update(id, pPurchaseDetails, purchaseDetails, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceBatch.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateUnitCost(int id, string UnitCost = null, bool redirect = false)
        {
            return Update(id, pUnitCost, UnitCost, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceBatch.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateUnitQuantity(int id, string UnitQuantity = null, bool redirect = false)
        {
            return Update(id, pUnitQuantity, UnitQuantity, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceBatch.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateDefaultDeviceModelId(int id, string DefaultDeviceModelId = null, bool redirect = false)
        {
            return Update(id, pDefaultDeviceModelId, DefaultDeviceModelId, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceBatch.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateWarrantyValidUntil(int id, string WarrantyValidUntil = null, bool redirect = false)
        {
            return Update(id, pWarrantyValidUntil, WarrantyValidUntil, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceBatch.Configure), ValidateInput(false)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateWarrantyDetails(int id, string warrantyDetails = null, bool redirect = false)
        {
            return Update(id, pWarrantyDetails, warrantyDetails, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceBatch.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateInsuredDate(int id, string InsuredDate = null, bool redirect = false)
        {
            return Update(id, pInsuredDate, InsuredDate, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceBatch.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateInsuranceSupplier(int id, string InsuranceSupplier = null, bool redirect = false)
        {
            return Update(id, pInsuranceSupplier, InsuranceSupplier, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceBatch.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateInsuredUntil(int id, string InsuredUntil = null, bool redirect = false)
        {
            return Update(id, pInsuredUntil, InsuredUntil, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceBatch.Configure), ValidateInput(false)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateInsuranceDetails(int id, string insuranceDetails = null, bool redirect = false)
        {
            return Update(id, pInsuranceDetails, insuranceDetails, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceBatch.Configure), ValidateInput(false)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateComments(int id, string comments = null, bool redirect = false)
        {
            return Update(id, pComments, comments, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceBatch.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateDevicesLinkedGroup(int id, string GroupId = null, bool redirect = false)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");

                var deviceBatch = Database.DeviceBatches.Find(id);
                if (deviceBatch == null)
                    throw new ArgumentException("Invalid Device Batch Id", "id");

                var syncTaskStatus = UpdateDevicesLinkedGroup(deviceBatch, GroupId);
                if (redirect)
                    if (syncTaskStatus == null)
                        return RedirectToAction(MVC.Config.DeviceBatch.Index(deviceBatch.Id));
                    else
                    {
                        syncTaskStatus.SetFinishedUrl(Url.Action(MVC.Config.DeviceBatch.Index(deviceBatch.Id)));
                        return RedirectToAction(MVC.Config.Logging.TaskStatus(syncTaskStatus.SessionId));
                    }
                else
                    return Ok();
            }
            catch (Exception ex)
            {
                if (redirect)
                    throw;
                else
                    return BadRequest(ex.Message);
            }
        }
        [DiscoAuthorize(Claims.Config.DeviceBatch.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateAssignedUsersLinkedGroup(int id, string GroupId = null, bool redirect = false)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");

                var deviceBatch = Database.DeviceBatches.Find(id);
                if (deviceBatch == null)
                    throw new ArgumentException("Invalid Device Batch Id", "id");

                var syncTaskStatus = UpdateAssignedUsersLinkedGroup(deviceBatch, GroupId);
                if (redirect)
                    if (syncTaskStatus == null)
                        return RedirectToAction(MVC.Config.DeviceBatch.Index(deviceBatch.Id));
                    else
                    {
                        syncTaskStatus.SetFinishedUrl(Url.Action(MVC.Config.DeviceBatch.Index(deviceBatch.Id)));
                        return RedirectToAction(MVC.Config.Logging.TaskStatus(syncTaskStatus.SessionId));
                    }
                else
                    return Ok();
            }
            catch (Exception ex)
            {
                if (redirect)
                    throw;
                else
                    return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Update Properties
        private void UpdateName(DeviceBatch deviceBatch, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("Name", "Device Batch Name is required");
            else
            {
                // Check for Duplicates
                var d = Database.DeviceBatches.Where(db => db.Id != deviceBatch.Id && db.Name == name).Count();
                if (d > 0)
                {
                    throw new Exception("A Device Batch with that name already exists");
                }
                deviceBatch.Name = name;
            }
            Database.SaveChanges();
        }
        private void UpdatePurchaseDate(DeviceBatch deviceBatch, string purchaseDate)
        {
            if (string.IsNullOrEmpty(purchaseDate))
                throw new ArgumentNullException("PurchaseDate", "A Device Batch Purchase Date is required");
            else
            {
                if (DateTime.TryParse(purchaseDate, out var ecd))
                {
                    deviceBatch.PurchaseDate = ecd.Date;
                }
                else
                {
                    throw new Exception("Invalid Date Format");
                }
            }
            Database.SaveChanges();
        }
        private void UpdateSupplier(DeviceBatch deviceBatch, string supplier)
        {
            if (string.IsNullOrWhiteSpace(supplier))
                deviceBatch.Supplier = null;
            else
                deviceBatch.Supplier = supplier;
            Database.SaveChanges();
        }
        private void UpdatePurchaseDetails(DeviceBatch deviceBatch, string purchaseDetails)
        {
            if (string.IsNullOrWhiteSpace(purchaseDetails))
                deviceBatch.PurchaseDetails = null;
            else
                deviceBatch.PurchaseDetails = purchaseDetails;
            Database.SaveChanges();
        }
        private void UpdateUnitCost(DeviceBatch deviceBatch, string unitCost)
        {
            if (string.IsNullOrWhiteSpace(unitCost))
                deviceBatch.UnitCost = null;
            else
            {
                unitCost = unitCost.Trim();
                if (unitCost.StartsWith("$"))
                    unitCost = unitCost.Substring(1).Trim(); // Remove $ sign if present

                if (decimal.TryParse(unitCost, out var unitCostValue))
                    deviceBatch.UnitCost = unitCostValue;
                else
                    throw new Exception("Invalid Currency Format");
            }
            Database.SaveChanges();
        }
        private void UpdateUnitQuantity(DeviceBatch deviceBatch, string unitQuantity)
        {
            if (string.IsNullOrWhiteSpace(unitQuantity))
                deviceBatch.UnitQuantity = null;
            else
            {
                if (int.TryParse(unitQuantity, out var unitQuantityValue))
                {
                    deviceBatch.UnitQuantity = unitQuantityValue;
                }
                else
                {
                    throw new Exception("Invalid Number");
                }
            }
            Database.SaveChanges();
        }
        private void UpdateDefaultDeviceModelId(DeviceBatch deviceBatch, string defaultDeviceModelId)
        {
            if (!string.IsNullOrEmpty(defaultDeviceModelId))
            {
                if (int.TryParse(defaultDeviceModelId, out var bId))
                {
                    var dm = Database.DeviceModels.Find(bId);
                    if (dm != null)
                    {
                        deviceBatch.DefaultDeviceModelId = dm.Id;
                        deviceBatch.DefaultDeviceModel = dm;

                        Database.SaveChanges();
                        return;
                    }
                }
            }
            else
            {
                // Null Id - No Batch
                deviceBatch.DefaultDeviceModelId = null;
                deviceBatch.DefaultDeviceModel = null;

                Database.SaveChanges();
                return;
            }
            throw new Exception("Invalid Device Model Id");
        }
        private void UpdateWarrantyValidUntil(DeviceBatch deviceBatch, string warrantyValidUntil)
        {
            if (string.IsNullOrEmpty(warrantyValidUntil))
                deviceBatch.WarrantyValidUntil = null;
            else
            {
                if (DateTime.TryParse(warrantyValidUntil, out var ecd))
                {
                    deviceBatch.WarrantyValidUntil = ecd.Date;
                }
                else
                {
                    throw new Exception("Invalid Date Format");
                }
            }
            Database.SaveChanges();
        }
        private void UpdateWarrantyDetails(DeviceBatch deviceBatch, string warrantyDetails)
        {
            if (string.IsNullOrWhiteSpace(warrantyDetails))
                deviceBatch.WarrantyDetails = null;
            else
                deviceBatch.WarrantyDetails = warrantyDetails;
            Database.SaveChanges();
        }
        private void UpdateInsuredDate(DeviceBatch deviceBatch, string insuredDate)
        {
            if (string.IsNullOrEmpty(insuredDate))
                deviceBatch.InsuredDate = null;
            else
            {
                if (DateTime.TryParse(insuredDate, out var ecd))
                {
                    deviceBatch.InsuredDate = ecd.Date;
                }
                else
                {
                    throw new Exception("Invalid Date Format");
                }
            }
            Database.SaveChanges();
        }
        private void UpdateInsuranceSupplier(DeviceBatch deviceBatch, string insuranceSupplier)
        {
            if (string.IsNullOrWhiteSpace(insuranceSupplier))
                deviceBatch.InsuranceSupplier = null;
            else
                deviceBatch.InsuranceSupplier = insuranceSupplier;
            Database.SaveChanges();
        }
        private void UpdateInsuredUntil(DeviceBatch deviceBatch, string insuredUntil)
        {
            if (string.IsNullOrEmpty(insuredUntil))
                deviceBatch.InsuredUntil = null;
            else
            {
                if (DateTime.TryParse(insuredUntil, out var ecd))
                {
                    deviceBatch.InsuredUntil = ecd.Date;
                }
                else
                {
                    throw new Exception("Invalid Date Format");
                }
            }
            Database.SaveChanges();
        }
        private void UpdateInsuranceDetails(DeviceBatch deviceBatch, string insuranceDetails)
        {
            if (string.IsNullOrWhiteSpace(insuranceDetails))
                deviceBatch.InsuranceDetails = null;
            else
                deviceBatch.InsuranceDetails = insuranceDetails;
            Database.SaveChanges();
        }
        private void UpdateComments(DeviceBatch deviceBatch, string comments)
        {
            if (string.IsNullOrWhiteSpace(comments))
                deviceBatch.Comments = null;
            else
                deviceBatch.Comments = comments;
            Database.SaveChanges();
        }

        private ScheduledTaskStatus UpdateDevicesLinkedGroup(DeviceBatch DeviceBatch, string devicesLinkedGroup)
        {
            var configJson = ADManagedGroup.ValidConfigurationToJson(DeviceBatchDevicesManagedGroup.GetKey(DeviceBatch), devicesLinkedGroup, null);

            if (DeviceBatch.DevicesLinkedGroup != configJson)
            {
                DeviceBatch.DevicesLinkedGroup = configJson;
                Database.SaveChanges();

                var managedGroup = DeviceBatchDevicesManagedGroup.Initialize(DeviceBatch);
                if (managedGroup != null) // Sync Group
                    return ADManagedGroupsSyncTask.ScheduleSync(managedGroup);
            }

            return null;
        }

        private ScheduledTaskStatus UpdateAssignedUsersLinkedGroup(DeviceBatch DeviceBatch, string assignedUsersLinkedGroup)
        {
            var configJson = ADManagedGroup.ValidConfigurationToJson(DeviceBatchAssignedUsersManagedGroup.GetKey(DeviceBatch), assignedUsersLinkedGroup, null);

            if (DeviceBatch.AssignedUsersLinkedGroup != configJson)
            {
                DeviceBatch.AssignedUsersLinkedGroup = configJson;
                Database.SaveChanges();

                var managedGroup = DeviceBatchAssignedUsersManagedGroup.Initialize(DeviceBatch);
                if (managedGroup != null) // Sync Group
                    return ADManagedGroupsSyncTask.ScheduleSync(managedGroup);
            }

            return null;
        }
        #endregion

        #region Actions

        [DiscoAuthorize(Claims.Config.DeviceBatch.Delete)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Delete(int id, bool? redirect = false)
        {
            try
            {
                var db = Database.DeviceBatches.Find(id);
                if (db != null)
                {
                    db.Delete(Database);
                    Database.SaveChanges();
                    if (redirect.HasValue && redirect.Value)
                        return RedirectToAction(MVC.Config.DeviceBatch.Index(null));
                    else
                        return Ok();
                }
                throw new Exception("Invalid Device Batch Number");
            }
            catch (Exception ex)
            {
                if (redirect.HasValue && redirect.Value)
                    throw;
                else
                    return BadRequest(ex.Message);
            }
        }

        #endregion

        #region Index

        [DiscoAuthorize(Claims.Config.DeviceBatch.Show)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Index(int? id)
        {
            if (id.HasValue)
            {
                Database.Configuration.ProxyCreationEnabled = false;
                DeviceBatch deviceBatch = Database.DeviceBatches.FirstOrDefault(db => db.Id == id);
                return Json(deviceBatch, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var deviceBatches = Database.DeviceBatches.ToArray();
                return Json(deviceBatches, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Timeline

        [DiscoAuthorizeAll(Claims.Config.DeviceBatch.Show, Claims.Config.DeviceBatch.ShowTimeline)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Timeline()
        {

            var batchesInformation = Database.DeviceBatches.Select(db => new
            {
                Id = db.Id,
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
                    title = $"{bi.Name} [{bi.DefaultModelDescription} x{bi.DeviceCount}]",
                    textColor = "#000",
                    description = bi.Comments ?? string.Empty,
                    color = ColorTranslator.ToHtml(color),
                    image = Url.Action(MVC.API.DeviceModel.Image(bi.DefaultModelId)),
                    link = Url.Action(MVC.Config.DeviceBatch.Index(bi.Id))
                });
            }

            return this.JsonNet(new Models.DeviceBatch.DeviceBatchTimelineEventSource() { events = events.ToArray() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Attachments

        [DiscoAuthorize(Claims.Config.DeviceBatch.Show)]
        [OutputCache(Location = System.Web.UI.OutputCacheLocation.Client, Duration = 172800)]
        public virtual ActionResult AttachmentDownload(int id)
        {
            var attachment = Database.DeviceBatchAttachments.Find(id);
            if (attachment != null)
            {
                var filePath = attachment.RepositoryFilename(Database);
                if (System.IO.File.Exists(filePath))
                {
                    return File(filePath, attachment.MimeType, attachment.Filename);
                }
                else
                {
                    return HttpNotFound("Attachment reference exists, but file not found");
                }
            }
            return HttpNotFound("Invalid Attachment Number");
        }

        [DiscoAuthorize(Claims.Config.DeviceBatch.Show)]
        [OutputCache(Location = System.Web.UI.OutputCacheLocation.Client, Duration = 172800)]
        public virtual ActionResult AttachmentThumbnail(int id)
        {
            var attachment = Database.DeviceBatchAttachments.Find(id);
            if (attachment != null)
            {
                var thumbPath = attachment.RepositoryThumbnailFilename(Database);
                if (System.IO.File.Exists(thumbPath))
                {
                    if (thumbPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                        return File(thumbPath, "image/png");
                    else
                        return File(thumbPath, "image/jpeg");
                }
                else
                    return File(ClientSource.Style.Images.AttachmentTypes.MimeTypeIcons.Icon(attachment.MimeType), "image/png");
            }
            return HttpNotFound("Invalid Attachment Number");
        }

        [DiscoAuthorize(Claims.Config.DeviceBatch.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult AttachmentUpload(int id, string comments)
        {
            var batch = Database.DeviceBatches.Find(id);
            if (batch != null)
            {
                if (Request.Files.Count > 0)
                {
                    var file = Request.Files.Get(0);
                    if (file.ContentLength > 0)
                    {
                        var contentType = file.ContentType;
                        if (string.IsNullOrEmpty(contentType) || contentType.Equals("unknown/unknown", StringComparison.OrdinalIgnoreCase))
                            contentType = MimeTypes.ResolveMimeType(file.FileName);

                        if (string.IsNullOrWhiteSpace(comments))
                            comments = null;

                        var attachment = new DeviceBatchAttachment()
                        {
                            DeviceBatchId = batch.Id,
                            TechUserId = CurrentUser.UserId,
                            Filename = file.FileName,
                            MimeType = contentType,
                            Timestamp = DateTime.Now,
                            Comments = comments
                        };
                        Database.DeviceBatchAttachments.Add(attachment);
                        Database.SaveChanges();

                        attachment.SaveAttachment(Database, file.InputStream);

                        attachment.GenerateThumbnail(Database);

                        return Json(attachment.Id, JsonRequestBehavior.AllowGet);
                    }
                }
                throw new Exception("No Attachment Uploaded");
            }
            throw new Exception("Invalid Device Batch Id");
        }

        [DiscoAuthorize(Claims.Config.DeviceBatch.Show)]
        public virtual ActionResult Attachment(int id)
        {
            var attachment = Database.DeviceBatchAttachments.Include(a => a.TechUser).Where(m => m.Id == id).FirstOrDefault();
            if (attachment != null)
            {

                var m = new Models.Attachment.AttachmentModel()
                {
                    Attachment = Models.Attachment._AttachmentModel.FromAttachment(attachment),
                    Result = "OK"
                };

                return Json(m, JsonRequestBehavior.AllowGet);
            }
            return BadRequest("Invalid Attachment Number");
        }

        [DiscoAuthorize(Claims.Config.DeviceBatch.Show)]
        public virtual ActionResult Attachments(int id)
        {
            var batch = Database.DeviceBatches.Include(b => b.DeviceBatchAttachments.Select(a => a.TechUser)).Where(m => m.Id == id).FirstOrDefault();
            if (batch != null)
            {
                var m = new Models.Attachment.AttachmentsModel()
                {
                    Attachments = batch.DeviceBatchAttachments.Select(a => Models.Attachment._AttachmentModel.FromAttachment(a)).ToList(),
                    Result = "OK"
                };

                return Json(m, JsonRequestBehavior.AllowGet);
            }
            return BadRequest("Invalid Device Batch Id");
        }

        [DiscoAuthorize(Claims.Config.DeviceBatch.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult AttachmentRemove(int id)
        {
            var attachment = Database.DeviceBatchAttachments.Include(a => a.TechUser).Where(m => m.Id == id).FirstOrDefault();
            if (attachment != null)
            {
                attachment.OnDelete(Database);
                Database.SaveChanges();
                return Ok();
            }
            return BadRequest("Invalid Attachment Number");
        }

        #endregion

    }
}
