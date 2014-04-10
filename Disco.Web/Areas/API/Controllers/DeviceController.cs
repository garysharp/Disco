using Disco.BI.Extensions;
using Disco.Services.Authorization;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Users;
using Disco.Services.Web;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class DeviceController : AuthorizedDatabaseController
    {

        const string pDeviceProfileId = "deviceprofileid";
        const string pDeviceBatchId = "devicebatchid";
        const string pAssetNumber = "assetnumber";
        const string pAssignedUserId = "assigneduserid";
        const string pLocation = "location";
        const string pAllowUnauthenticatedEnrol = "allowunauthenticatedenrol";
        const string pDetailACAdapter = "detailacadapter";

        public virtual ActionResult Update(string id, string key, string value = null, bool redirect = false)
        {
            Database.Configuration.LazyLoadingEnabled = true;

            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new ArgumentNullException("id");
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");
                var device = Database.Devices.Find(id);
                if (device != null)
                {
                    switch (key.ToLower())
                    {
                        case pDeviceProfileId:
                            Authorization.Require(Claims.Device.Properties.DeviceProfile);
                            UpdateDeviceProfileId(device, value);
                            break;
                        case pDeviceBatchId:
                            Authorization.Require(Claims.Device.Properties.DeviceBatch);
                            UpdateDeviceBatchId(device, value);
                            break;
                        case pAssetNumber:
                            Authorization.Require(Claims.Device.Properties.AssetNumber);
                            UpdateAssetNumber(device, value);
                            break;
                        case pAssignedUserId:
                            Authorization.Require(Claims.Device.Actions.AssignUser);
                            UpdateAssignedUserId(device, value);
                            break;
                        case pLocation:
                            Authorization.Require(Claims.Device.Properties.Location);
                            UpdateLocation(device, value);
                            break;
                        case pAllowUnauthenticatedEnrol:
                            Authorization.Require(Claims.Device.Actions.AllowUnauthenticatedEnrol);
                            UpdateAllowUnauthenticatedEnrol(device, value);
                            break;
                        case pDetailACAdapter:
                            Authorization.Require(Claims.Device.Properties.Details);
                            UpdateDetailACAdapter(device, value);
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

        [DiscoAuthorize(Claims.Device.Properties.DeviceProfile)]
        public virtual ActionResult UpdateDeviceProfileId(string id, string DeviceProfileId = null, bool redirect = false)
        {
            return Update(id, pDeviceProfileId, DeviceProfileId, redirect);
        }

        [DiscoAuthorize(Claims.Device.Properties.DeviceBatch)]
        public virtual ActionResult UpdateDeviceBatchId(string id, string DeviceBatchId = null, bool redirect = false)
        {
            return Update(id, pDeviceBatchId, DeviceBatchId, redirect);
        }

        [DiscoAuthorize(Claims.Device.Properties.AssetNumber)]
        public virtual ActionResult UpdateAssetNumber(string id, string AssetNumber = null, bool redirect = false)
        {
            return Update(id, pAssetNumber, AssetNumber, redirect);
        }

        [DiscoAuthorize(Claims.Device.Properties.Location)]
        public virtual ActionResult UpdateLocation(string id, string Location = null, bool redirect = false)
        {
            return Update(id, pLocation, Location, redirect);
        }

        [DiscoAuthorize(Claims.Device.Actions.AssignUser)]
        public virtual ActionResult UpdateAssignedUserId(string id, string AssignedUserId = null, bool redirect = false)
        {
            return Update(id, pAssignedUserId, AssignedUserId, redirect);
        }

        [DiscoAuthorize(Claims.Device.Actions.AllowUnauthenticatedEnrol)]
        public virtual ActionResult UpdateAllowUnauthenticatedEnrol(string id, string AllowUnauthenticatedEnrol = null, bool redirect = false)
        {
            return Update(id, pAllowUnauthenticatedEnrol, AllowUnauthenticatedEnrol, redirect);
        }

        [DiscoAuthorize(Claims.Device.Properties.Details)]
        public virtual ActionResult UpdateDetailACAdapter(string id, string DetailACAdapter = null, bool redirect = false)
        {
            return Update(id, pDetailACAdapter, DetailACAdapter, redirect);
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
                    var p = Database.DeviceProfiles.Find(pId);
                    if (p != null)
                    {
                        device.DeviceProfileId = p.Id;
                        device.DeviceProfile = p;

                        // Update AD Account
                        if (!string.IsNullOrEmpty(device.DeviceDomainId) && device.DeviceDomainId.Length <= 24)
                        {
                            var adMachineAccount = ActiveDirectory.RetrieveMachineAccount(device.DeviceDomainId);
                            if (adMachineAccount != null)
                                adMachineAccount.SetDescription(device);
                        }

                        Database.SaveChanges();
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
                    var b = Database.DeviceBatches.Find(bId);
                    if (b != null)
                    {
                        device.DeviceBatchId = b.Id;
                        device.DeviceBatch = b;

                        Database.SaveChanges();
                        return;
                    }
                }
            }
            else
            {
                // Null Id - No Batch
                device.DeviceBatchId = null;
                device.DeviceBatch = null;

                Database.SaveChanges();
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
            Database.SaveChanges();
        }
        private void UpdateLocation(Disco.Models.Repository.Device device, string Location)
        {
            if (string.IsNullOrWhiteSpace(Location))
                device.Location = null;
            else
                device.Location = Location.Trim();
            Database.SaveChanges();
        }
        private void UpdateAssignedUserId(Disco.Models.Repository.Device device, string UserId)
        {
            var daus = Database.DeviceUserAssignments.Where(m => m.DeviceSerialNumber == device.SerialNumber && m.UnassignedDate == null);
            Disco.Models.Repository.User u = null;
            if (!string.IsNullOrEmpty(UserId))
            {
                u = UserService.GetUser(UserId, Database, true);

                if (u == null)
                    throw new Exception("Invalid Username");
            }

            device.AssignDevice(Database, u);
            Database.SaveChanges();
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
                Database.SaveChanges();
            }
        }
        private void UpdateDetailACAdapter(Disco.Models.Repository.Device device, string ACAdapter)
        {
            if (string.IsNullOrWhiteSpace(ACAdapter))
                device.DeviceDetails.ACAdapter(device, null);
            else
                device.DeviceDetails.ACAdapter(device, ACAdapter.Trim());
            Database.SaveChanges();
        }
        #endregion

        #region Device Actions

        [DiscoAuthorize(Claims.Device.Actions.Decommission)]
        public virtual ActionResult Decommission(string id, int Reason, bool redirect)
        {
            var d = Database.Devices.Find(id);
            Database.Configuration.LazyLoadingEnabled = true;
            if (d != null)
            {
                if (d.CanDecommission())
                {
                    d.OnDecommission((Disco.Models.Repository.Device.DecommissionReasons)Reason);

                    Database.SaveChanges();
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

        [DiscoAuthorize(Claims.Device.Actions.Recommission)]
        public virtual ActionResult Recommission(string id, bool redirect)
        {
            var d = Database.Devices.Find(id);
            Database.Configuration.LazyLoadingEnabled = true;
            if (d != null)
            {
                if (d.CanRecommission())
                {
                    d.OnRecommission();

                    Database.SaveChanges();
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

        [DiscoAuthorize(Claims.Device.Actions.Delete)]
        public virtual ActionResult Delete(string id, bool redirect)
        {
            var j = Database.Devices.Find(id);
            Database.Configuration.LazyLoadingEnabled = true;
            if (j != null)
            {
                if (j.CanDelete())
                {
                    j.OnDelete(Database);

                    Database.SaveChanges();
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

        [DiscoAuthorize(Claims.Device.Actions.GenerateDocuments)]
        public virtual ActionResult GeneratePdf(string id, string DocumentTemplateId)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");
            if (string.IsNullOrEmpty(DocumentTemplateId))
                throw new ArgumentNullException("AttachmentTypeId");
            var device = Database.Devices.Find(id);
            if (device != null)
            {
                var documentTemplate = Database.DocumentTemplates.Find(DocumentTemplateId);
                if (documentTemplate != null)
                {
                    var timeStamp = DateTime.Now;
                    Stream pdf;
                    using (var generationState = Disco.Models.BI.DocumentTemplates.DocumentState.DefaultState())
                    {
                        pdf = documentTemplate.GeneratePdf(Database, device, UserService.CurrentUser, timeStamp, generationState);
                    }
                    Database.SaveChanges();
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

        [DiscoAuthorize(Claims.Device.Show)]
        public virtual ActionResult LastNetworkLogonDate(string id)
        {
            var device = Database.Devices.Find(id);
            if (device == null)
            {
                return HttpNotFound("Invalid Device Serial Number");
            }

            if (device.UpdateLastNetworkLogonDate())
                Database.SaveChanges();

            var result = new
            {
                Timestamp = device.LastNetworkLogonDate,
                UnixEpoc = device.LastNetworkLogonDate.ToUnixEpoc(),
                Formatted = device.LastNetworkLogonDate.ToFullDateTime("Unknown")
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region Device Attachements

        [DiscoAuthorize(Claims.Device.ShowAttachments), OutputCache(Location = System.Web.UI.OutputCacheLocation.Client, Duration = 172800)]
        public virtual ActionResult AttachmentDownload(int id)
        {
            var da = Database.DeviceAttachments.Find(id);
            if (da != null)
            {
                var filePath = da.RepositoryFilename(Database);
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

        [DiscoAuthorize(Claims.Device.ShowAttachments), OutputCache(Location = System.Web.UI.OutputCacheLocation.Client, Duration = 172800)]
        public virtual ActionResult AttachmentThumbnail(int id)
        {
            var da = Database.DeviceAttachments.Find(id);
            if (da != null)
            {
                var thumbPath = da.RepositoryThumbnailFilename(Database);
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

        [DiscoAuthorize(Claims.Device.Actions.AddAttachments)]
        public virtual ActionResult AttachmentUpload(string id, string Comments)
        {
            var d = Database.Devices.Find(id);
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
                            TechUserId = UserService.CurrentUserId,
                            Filename = file.FileName,
                            MimeType = contentType,
                            Timestamp = DateTime.Now,
                            Comments = Comments
                        };
                        Database.DeviceAttachments.Add(da);
                        Database.SaveChanges();

                        da.SaveAttachment(Database, file.InputStream);

                        da.GenerateThumbnail(Database);

                        return Json(da.Id, JsonRequestBehavior.AllowGet);
                    }
                }
                throw new Exception("No Attachment Uploaded");
            }
            throw new Exception("Invalid Device Serial Number");
        }

        [DiscoAuthorize(Claims.Device.ShowAttachments)]
        public virtual ActionResult Attachment(int id)
        {
            var da = Database.DeviceAttachments.Include("TechUser").Where(m => m.Id == id).FirstOrDefault();
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

        [DiscoAuthorize(Claims.Device.ShowAttachments)]
        public virtual ActionResult Attachments(string id)
        {
            var d = Database.Devices.Include("DeviceAttachments.TechUser").Where(m => m.SerialNumber == id).FirstOrDefault();
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

        [DiscoAuthorizeAny(Claims.Job.Actions.RemoveAnyAttachments, Claims.Job.Actions.RemoveOwnAttachments)]
        public virtual ActionResult AttachmentRemove(int id)
        {
            var da = Database.DeviceAttachments.Include("TechUser").Where(m => m.Id == id).FirstOrDefault();
            if (da != null)
            {
                if (da.TechUserId.Equals(CurrentUser.UserId, StringComparison.InvariantCultureIgnoreCase))
                    Authorization.RequireAny(Claims.Device.Actions.RemoveAnyAttachments, Claims.Device.Actions.RemoveOwnAttachments);
                else
                    Authorization.Require(Claims.Device.Actions.RemoveAnyAttachments);

                da.OnDelete(Database);
                Database.SaveChanges();
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            return Json("Invalid Attachment Number", JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Importing / Exporting

        [DiscoAuthorize(Claims.Device.Actions.Import)]
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

        [DiscoAuthorize(Claims.Device.Actions.Import)]
        public virtual ActionResult ImportProcess(string ParseTaskSessionKey)
        {
            if (string.IsNullOrWhiteSpace(ParseTaskSessionKey))
                throw new ArgumentNullException("ParseTaskSessionKey");

            var status = Disco.BI.DeviceBI.Importing.ImportProcessTask.Run(ParseTaskSessionKey);

            status.SetFinishedUrl(Url.Action(MVC.Device.Index()));

            return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
        }

        [DiscoAuthorize(Claims.Device.Actions.Export)]
        public virtual ActionResult ExportAllDevices()
        {
            // Non-Decommissioned Devices
            var devices = Database.Devices.Where(d => !d.DecommissionedDate.HasValue);

            var export = BI.DeviceBI.Importing.Export.GenerateExport(devices);

            var filename = string.Format("DiscoDeviceExport-AllDevices-{0:yyyyMMdd-HHmmss}.csv", DateTime.Now);

            return File(export, "text/csv", filename);
        }

        #endregion

        [DiscoAuthorize(Claims.DiscoAdminAccount)]
        public virtual ActionResult MigrateDeviceMacAddressesFromLog()
        {
            var taskStatus = Disco.BI.DeviceBI.Migration.LogMacAddressImporting.ScheduleImmediately();
            return RedirectToAction(MVC.Config.Logging.TaskStatus(taskStatus.SessionId));
        }
    }
}
