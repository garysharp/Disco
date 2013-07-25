using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.BI.Extensions;
using Disco.BI;
using Disco.BI.Interop.ActiveDirectory;
using System.IO;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class DeviceController : dbAdminController
    {

        const string pDeviceProfileId = "deviceprofileid";
        const string pDeviceBatchId = "devicebatchid";
        const string pAssetNumber = "assetnumber";
        const string pAssignedUserId = "assigneduserid";
        const string pLocation = "location";
        const string pAllowUnauthenticatedEnrol = "allowunauthenticatedenrol";

        public virtual ActionResult Update(string id, string key, string value = null, bool redirect = false)
        {
            dbContext.Configuration.LazyLoadingEnabled = true;

            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new ArgumentNullException("id");
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");
                var device = dbContext.Devices.Find(id);
                if (device != null)
                {
                    switch (key.ToLower())
                    {
                        case pDeviceProfileId:
                            UpdateDeviceProfileId(device, value);
                            break;
                        case pDeviceBatchId:
                            UpdateDeviceBatchId(device, value);
                            break;
                        case pAssetNumber:
                            UpdateAssetNumber(device, value);
                            break;
                        case pAssignedUserId:
                            UpdateAssignedUserId(device, value);
                            break;
                        case pLocation:
                            UpdateLocation(device, value);
                            break;
                        case pAllowUnauthenticatedEnrol:
                            UpdateAllowUnauthenticatedEnrol(device, value);
                            break;
                        default:
                            throw new Exception("Invalid Update Key");
                    }
                }
                else
                {
                    throw new Exception("Invalid Serial Number or Device Profile Id");
                }
                if (redirect)
                    return RedirectToAction(MVC.Device.Show(device.SerialNumber));
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
        public virtual ActionResult UpdateDeviceProfileId(string id, string DeviceProfileId = null, bool redirect = false)
        {
            return Update(id, pDeviceProfileId, DeviceProfileId, redirect);
        }
        public virtual ActionResult UpdateDeviceBatchId(string id, string DeviceBatchId = null, bool redirect = false)
        {
            return Update(id, pDeviceBatchId, DeviceBatchId, redirect);
        }
        public virtual ActionResult UpdateAssetNumber(string id, string AssetNumber = null, bool redirect = false)
        {
            return Update(id, pAssetNumber, AssetNumber, redirect);
        }
        public virtual ActionResult UpdateLocation(string id, string Location = null, bool redirect = false)
        {
            return Update(id, pLocation, Location, redirect);
        }
        public virtual ActionResult UpdateAssignedUserId(string id, string AssignedUserId = null, bool redirect = false)
        {
            return Update(id, pAssignedUserId, AssignedUserId, redirect);
        }
        public virtual ActionResult UpdateAllowUnauthenticatedEnrol(string id, string AllowUnauthenticatedEnrol = null, bool redirect = false)
        {
            return Update(id, pAllowUnauthenticatedEnrol, AllowUnauthenticatedEnrol, redirect);
        }
        #endregion

        #region Update Properties
        private void UpdateDeviceProfileId(Disco.Models.Repository.Device device, string DeviceProfileId)
        {
            if (!string.IsNullOrEmpty(DeviceProfileId))
            {
                int pId;
                if (int.TryParse(DeviceProfileId, out pId))
                {
                    var p = dbContext.DeviceProfiles.Find(pId);
                    if (p != null)
                    {
                        device.DeviceProfileId = p.Id;
                        device.DeviceProfile = p;

                        // Update AD Account
                        if (!string.IsNullOrEmpty(device.ComputerName) && device.ComputerName.Length <= 24)
                        {
                            var adMachineAccount = ActiveDirectory.GetMachineAccount(device.ComputerName);
                            if (adMachineAccount != null)
                                adMachineAccount.SetDescription(device);
                        }

                        dbContext.SaveChanges();
                        return;
                    }
                }
            }
            throw new Exception("Invalid Device Profile Id");
        }
        private void UpdateDeviceBatchId(Disco.Models.Repository.Device device, string DeviceBatchId)
        {
            if (!string.IsNullOrEmpty(DeviceBatchId))
            {
                int bId;
                if (int.TryParse(DeviceBatchId, out bId))
                {
                    var b = dbContext.DeviceBatches.Find(bId);
                    if (b != null)
                    {
                        device.DeviceBatchId = b.Id;
                        device.DeviceBatch = b;

                        dbContext.SaveChanges();
                        return;
                    }
                }
            }
            else
            {
                // Null Id - No Batch
                device.DeviceBatchId = null;
                device.DeviceBatch = null;

                dbContext.SaveChanges();
                return;
            }
            throw new Exception("Invalid Device Batch Id");
        }
        private void UpdateAssetNumber(Disco.Models.Repository.Device device, string AssetNumber)
        {
            if (string.IsNullOrWhiteSpace(AssetNumber))
                device.AssetNumber = null;
            else
                device.AssetNumber = AssetNumber.Trim();
            dbContext.SaveChanges();
        }
        private void UpdateLocation(Disco.Models.Repository.Device device, string Location)
        {
            if (string.IsNullOrWhiteSpace(Location))
                device.Location = null;
            else
                device.Location = Location.Trim();
            dbContext.SaveChanges();
        }
        private void UpdateAssignedUserId(Disco.Models.Repository.Device device, string UserId)
        {
            var daus = dbContext.DeviceUserAssignments.Where(m => m.DeviceSerialNumber == device.SerialNumber && m.UnassignedDate == null);
            Disco.Models.Repository.User u = null;
            if (!string.IsNullOrEmpty(UserId))
            {
                // Changed 2012-12-13 G# - Stop error when assigning user - Force Refresh
                // http://www.discoict.com.au/forum/support/2012/11/error-when-assigning-multiple-devices-to-single-user.aspx
                //u = BI.UserBI.UserCache.GetUser(UserId, dbContext);
                u = BI.UserBI.UserCache.GetUser(UserId, dbContext, true);
                // End Changed 2012-12-13 G#
                if (u == null)
                {
                    throw new Exception("Invalid Username");
                }
            }

            device.AssignDevice(dbContext, u);
            dbContext.SaveChanges();
        }
        private void UpdateAllowUnauthenticatedEnrol(Disco.Models.Repository.Device device, string AllowUnauthenticatedEnrol)
        {
            bool bAllowUnauthenticatedEnrol;
            if (string.IsNullOrEmpty(AllowUnauthenticatedEnrol) || !bool.TryParse(AllowUnauthenticatedEnrol, out bAllowUnauthenticatedEnrol))
            {
                throw new Exception("Invalid AllowUnauthenticatedEnrol Value");
            }

            if (device.AllowUnauthenticatedEnrol != bAllowUnauthenticatedEnrol)
            {
                device.AllowUnauthenticatedEnrol = bAllowUnauthenticatedEnrol;
                dbContext.SaveChanges();
            }
        }
        #endregion

        #region Device Actions
        public virtual ActionResult Decommission(string id, bool redirect)
        {
            var d = dbContext.Devices.Find(id);
            dbContext.Configuration.LazyLoadingEnabled = true;
            if (d != null)
            {
                if (d.CanDecommission())
                {
                    d.OnDecommission();

                    dbContext.SaveChanges();
                    if (redirect)
                        return RedirectToAction(MVC.Device.Show(id));
                    else
                        return Json("OK", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Device's state doesn't allow this action", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Invalid Device Serial Number", JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult Recommission(string id, bool redirect)
        {
            var d = dbContext.Devices.Find(id);
            dbContext.Configuration.LazyLoadingEnabled = true;
            if (d != null)
            {
                if (d.CanRecommission())
                {
                    d.OnRecommission();

                    dbContext.SaveChanges();
                    if (redirect)
                        return RedirectToAction(MVC.Device.Show(id));
                    else
                        return Json("OK", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Device's state doesn't allow this action", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Invalid Device Serial Number", JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult Delete(string id, bool redirect)
        {
            var j = dbContext.Devices.Find(id);
            dbContext.Configuration.LazyLoadingEnabled = true;
            if (j != null)
            {
                if (j.CanDelete())
                {
                    j.OnDelete(dbContext);

                    dbContext.SaveChanges();
                    if (redirect)
                        return RedirectToAction(MVC.Device.Index());
                    else
                        return Json("OK", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Job's state doesn't allow this action", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Invalid Device Serial Number", JsonRequestBehavior.AllowGet);
        }
        #endregion

        public virtual ActionResult GeneratePdf(string id, string DocumentTemplateId)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");
            if (string.IsNullOrEmpty(DocumentTemplateId))
                throw new ArgumentNullException("AttachmentTypeId");
            var device = dbContext.Devices.Find(id);
            if (device != null)
            {
                var documentTemplate = dbContext.DocumentTemplates.Find(DocumentTemplateId);
                if (documentTemplate != null)
                {
                    var timeStamp = DateTime.Now;
                    Stream pdf;
                    using (var generationState = Disco.Models.BI.DocumentTemplates.DocumentState.DefaultState()){
                        pdf = documentTemplate.GeneratePdf(dbContext, device, DiscoApplication.CurrentUser, timeStamp, generationState);
                    }
                    dbContext.SaveChanges();
                    return File(pdf, "application/pdf", string.Format("{0}_{1}_{2:yyyyMMdd-HHmmss}.pdf", documentTemplate.Id, device.SerialNumber, timeStamp));
                }
                else
                {
                    throw new ArgumentException("Invalid Document Template Id", "id");
                }
            }
            else
            {
                throw new ArgumentException("Invalid Serial Number", "id");
            }
        }

        public virtual ActionResult LastNetworkLogonDate(string id)
        {
            var device = dbContext.Devices.Find(id);
            if (device == null)
            {
                return HttpNotFound("Invalid Device Serial Number");
            }

            if (device.UpdateLastNetworkLogonDate())
                dbContext.SaveChanges();

            var result = new
            {
                Timestamp = device.LastNetworkLogonDate,
                Friendly = device.LastNetworkLogonDate.ToFuzzy("Unknown"),
                Formatted = device.LastNetworkLogonDate.ToFullDateTime("Unknown")
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region Device Attachements
        [OutputCache(Location = System.Web.UI.OutputCacheLocation.Client, Duration = 172800)]
        public virtual ActionResult AttachmentDownload(int id)
        {
            var da = dbContext.DeviceAttachments.Find(id);
            if (da != null)
            {
                var filePath = da.RepositoryFilename(dbContext);
                if (System.IO.File.Exists(filePath))
                {
                    return File(filePath, da.MimeType, da.Filename);
                }
                else
                {
                    return HttpNotFound("Attachment reference exists, but file not found");
                }
            }
            return HttpNotFound("Invalid Attachment Number");
        }
        [OutputCache(Location = System.Web.UI.OutputCacheLocation.Client, Duration = 172800)]
        public virtual ActionResult AttachmentThumbnail(int id)
        {
            var da = dbContext.DeviceAttachments.Find(id);
            if (da != null)
            {
                var thumbPath = da.RepositoryThumbnailFilename(dbContext);
                if (System.IO.File.Exists(thumbPath))
                {
                    if (thumbPath.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                        return File(thumbPath, "image/png");
                    else
                        return File(thumbPath, "image/jpg");
                }
                else
                    return File(ClientSource.Style.Images.AttachmentTypes.MimeTypeIcons.Icon(da.MimeType), "image/png");
            }
            return HttpNotFound("Invalid Attachment Number");
        }
        public virtual ActionResult AttachmentUpload(string id, string Comments)
        {
            var d = dbContext.Devices.Find(id);
            if (d != null)
            {
                if (Request.Files.Count > 0)
                {
                    var file = Request.Files.Get(0);
                    if (file.ContentLength > 0)
                    {
                        var contentType = file.ContentType;
                        if (string.IsNullOrEmpty(contentType) || contentType.Equals("unknown/unknown", StringComparison.InvariantCultureIgnoreCase))
                            contentType = BI.Interop.MimeTypes.ResolveMimeType(file.FileName);

                        var da = new Disco.Models.Repository.DeviceAttachment()
                        {
                            DeviceSerialNumber = d.SerialNumber,
                            TechUserId = DiscoApplication.CurrentUser.Id,
                            Filename = file.FileName,
                            MimeType = contentType,
                            Timestamp = DateTime.Now,
                            Comments = Comments
                        };
                        dbContext.DeviceAttachments.Add(da);
                        dbContext.SaveChanges();

                        da.SaveAttachment(dbContext, file.InputStream);

                        da.GenerateThumbnail(dbContext);

                        return Json(da.Id, JsonRequestBehavior.AllowGet);
                    }
                }
                throw new Exception("No Attachment Uploaded");
            }
            throw new Exception("Invalid Device Serial Number");
        }
        public virtual ActionResult Attachment(int id)
        {
            var da = dbContext.DeviceAttachments.Include("TechUser").Where(m => m.Id == id).FirstOrDefault();
            if (da != null)
            {

                var m = new Models.Attachment.AttachmentModel()
                {
                    Attachment = Models.Attachment._AttachmentModel.FromAttachment(da),
                    Result = "OK"
                };

                return Json(m, JsonRequestBehavior.AllowGet);
            }
            return Json(new Models.Attachment.AttachmentModel() { Result = "Invalid Attachment Number" }, JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult Attachments(string id)
        {
            var d = dbContext.Devices.Include("DeviceAttachments.TechUser").Where(m => m.SerialNumber == id).FirstOrDefault();
            if (d != null)
            {
                var m = new Models.Attachment.AttachmentsModel()
                {
                    Attachments = d.DeviceAttachments.Select(ua => Models.Attachment._AttachmentModel.FromAttachment(ua)).ToList(),
                    Result = "OK"
                };

                return Json(m, JsonRequestBehavior.AllowGet);
            }
            return Json(new Models.Attachment.AttachmentsModel() { Result = "Invalid Device Serial Number" }, JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult AttachmentRemove(int id)
        {
            var da = dbContext.DeviceAttachments.Include("TechUser").Where(m => m.Id == id).FirstOrDefault();
            if (da != null)
            {
                // 2012-02-17 G# Remove - 'Delete Own Comments' policy
                //if (da.TechUserId == DiscoApplication.CurrentUser.Id)
                //{
                da.OnDelete(dbContext);
                dbContext.SaveChanges();
                return Json("OK", JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    return Json("You can only delete your own attachments.", JsonRequestBehavior.AllowGet);
                //}
            }
            return Json("Invalid Attachment Number", JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Importing / Exporting
        public virtual ActionResult ImportParse(HttpPostedFileBase ImportFile)
        {
            if (ImportFile == null || ImportFile.ContentLength == 0)
                throw new ArgumentNullException("ImportFile");

            var fileName = ImportFile.FileName;
            if (fileName.Contains(@"\"))
                fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);

            var status = Disco.BI.DeviceBI.Importing.ImportParseTask.Run(ImportFile.InputStream, fileName);

            status.SetFinishedUrl(Url.Action(MVC.Device.ImportReview(status.SessionId)));

            return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
        }

        public virtual ActionResult ImportProcess(string ParseTaskSessionKey)
        {
            if (string.IsNullOrWhiteSpace(ParseTaskSessionKey))
                throw new ArgumentNullException("ParseTaskSessionKey");

            var status = Disco.BI.DeviceBI.Importing.ImportProcessTask.Run(ParseTaskSessionKey);

            status.SetFinishedUrl(Url.Action(MVC.Device.Index()));

            return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
        }

        public virtual ActionResult ExportAllDevices()
        {
            // Non-Decommissioned Devices
            var devices = dbContext.Devices.Where(d => !d.DecommissionedDate.HasValue);

            var export = BI.DeviceBI.Importing.Export.GenerateExport(devices);

            var filename = string.Format("DiscoDeviceExport-AllDevices-{0:yyyyMMdd-HHmmss}.csv", DateTime.Now);

            return File(export, "text/csv", filename);
        } 
        #endregion

    }
}
