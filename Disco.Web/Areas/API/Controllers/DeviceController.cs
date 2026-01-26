using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Devices;
using Disco.Services.Devices.Importing;
using Disco.Services.Exporting;
using Disco.Services.Interop;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Interop.DiscoServices.Upload;
using Disco.Services.Logging;
using Disco.Services.Users;
using Disco.Services.Web;
using Disco.Web.Extensions;
using Disco.Web.Models.Device;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class DeviceController : AuthorizedDatabaseController
    {
        private const string pDeviceProfileId = "deviceprofileid";
        private const string pDeviceBatchId = "devicebatchid";
        private const string pAssetNumber = "assetnumber";
        private const string pAssignedUserId = "assigneduserid";
        private const string pLocation = "location";
        private const string pAllowUnauthenticatedEnrol = "allowunauthenticatedenrol";
        private const string pDetailACAdapter = "detailacadapter";
        private const string pDetailBattery = "detailbattery";
        private const string pDetailKeyboard = "detailkeyboard";

        [HttpPost, ValidateAntiForgeryToken]
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
                        case pDetailBattery:
                            Authorization.Require(Claims.Device.Properties.Details);
                            UpdateDetailBattery(device, value);
                            break;
                        case pDetailKeyboard:
                            Authorization.Require(Claims.Device.Properties.Details);
                            UpdateDetailKeyboard(device, value);
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

        [DiscoAuthorize(Claims.Device.Properties.DeviceProfile)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateDeviceProfileId(string id, string DeviceProfileId = null, bool enforceOrganisationalUnit = false, bool redirect = false)
        {
            var updateResult = Update(id, pDeviceProfileId, DeviceProfileId, redirect);

            if (enforceOrganisationalUnit)
            {
                var device = Database.Devices
                    .Include(d => d.DeviceProfile)
                    .First(d => d.SerialNumber == id);

                if (ActiveDirectory.IsValidDomainAccountId(device.DeviceDomainId, out ADDomain deviceDomain))
                {
                    var ou = device.DeviceProfile.OrganisationalUnit;
                    if (string.IsNullOrWhiteSpace(ou))
                        ou = ActiveDirectory.Context.PrimaryDomain.DefaultComputerContainer;
                    var domain = ActiveDirectory.Context.GetDomainFromDistinguishedName(ou);

                    if (domain != deviceDomain)
                        SystemLog.LogWarning($"Device '{device.SerialNumber}' [{device.DeviceDomainId}] is not in the same domain as the Organisational Unit '{ou}' and cannot be moved");
                    else
                    {
                        var domainController = domain.GetAvailableDomainController(RequireWritable: true);
                        var deviceAccount = domainController.RetrieveADMachineAccount(device.DeviceDomainId);

                        if (deviceAccount == null)
                            SystemLog.LogWarning($"Device '{device.SerialNumber}' [{device.DeviceDomainId}] was not found on the domain controller");
                        else
                        {
                            if (!string.Equals(deviceAccount.ParentDistinguishedName, ou, StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    var existingOu = deviceAccount.ParentDistinguishedName;
                                    deviceAccount.MoveOrganisationalUnit(domainController, ou);
                                    SystemLog.LogInformation($"Device Profile Updated; Moved Device '{device.SerialNumber}' [{device.DeviceDomainId}] from '{existingOu}' to '{ou}'");
                                }
                                catch (Exception ex)
                                {
                                    SystemLog.LogException($"Failed to move Device '{device.SerialNumber}' [{device.DeviceDomainId}] from '{deviceAccount.ParentDistinguishedName}' to '{ou}'", ex);
                                }
                            }
                        }
                    }
                }
            }

            return updateResult;
        }

        [DiscoAuthorize(Claims.Device.Properties.DeviceBatch)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateDeviceBatchId(string id, string DeviceBatchId = null, bool redirect = false)
        {
            return Update(id, pDeviceBatchId, DeviceBatchId, redirect);
        }

        [DiscoAuthorize(Claims.Device.Properties.AssetNumber)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateAssetNumber(string id, string AssetNumber = null, bool redirect = false)
        {
            return Update(id, pAssetNumber, AssetNumber, redirect);
        }

        [DiscoAuthorize(Claims.Device.Properties.Location)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateLocation(string id, string Location = null, bool redirect = false)
        {
            return Update(id, pLocation, Location, redirect);
        }

        [DiscoAuthorize(Claims.Device.Actions.AssignUser)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateAssignedUserId(string id, string AssignedUserId = null, bool redirect = false)
        {
            if (!string.IsNullOrWhiteSpace(AssignedUserId))
                AssignedUserId = ActiveDirectory.ParseDomainAccountId(AssignedUserId);

            return Update(id, pAssignedUserId, AssignedUserId, redirect);
        }

        [DiscoAuthorize(Claims.Device.Actions.AllowUnauthenticatedEnrol)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateAllowUnauthenticatedEnrol(string id, string AllowUnauthenticatedEnrol = null, bool redirect = false)
        {
            return Update(id, pAllowUnauthenticatedEnrol, AllowUnauthenticatedEnrol, redirect);
        }

        [DiscoAuthorize(Claims.Device.Properties.Details)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateDetailACAdapter(string id, string DetailACAdapter = null, bool redirect = false)
        {
            return Update(id, pDetailACAdapter, DetailACAdapter, redirect);
        }

        [DiscoAuthorize(Claims.Device.Properties.Details)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateDetailBattery(string id, string DetailBattery = null, bool redirect = false)
        {
            return Update(id, pDetailBattery, DetailBattery, redirect);
        }

        [DiscoAuthorize(Claims.Device.Properties.Details)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateDetailKeyboard(string id, string DetailKeyboard = null, bool redirect = false)
        {
            return Update(id, pDetailKeyboard, DetailKeyboard, redirect);
        }

        #endregion

        #region Update Properties
        private void UpdateDeviceProfileId(Device device, string DeviceProfileId)
        {
            if (!string.IsNullOrEmpty(DeviceProfileId))
            {
                if (int.TryParse(DeviceProfileId, out var pId))
                {
                    var p = Database.DeviceProfiles.Find(pId);
                    if (p != null)
                    {
                        device.DeviceProfileId = p.Id;
                        device.DeviceProfile = p;

                        // Update AD Account
                        if (ActiveDirectory.IsValidDomainAccountId(device.DeviceDomainId))
                        {
                            var adMachineAccount = ActiveDirectory.RetrieveADMachineAccount(device.DeviceDomainId);
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
        private void UpdateDeviceBatchId(Device device, string DeviceBatchId)
        {
            if (!string.IsNullOrEmpty(DeviceBatchId))
            {
                if (int.TryParse(DeviceBatchId, out var bId))
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
        private void UpdateAssetNumber(Device device, string AssetNumber)
        {
            if (string.IsNullOrWhiteSpace(AssetNumber))
                device.AssetNumber = null;
            else
                device.AssetNumber = AssetNumber.Trim();
            Database.SaveChanges();
        }
        private void UpdateLocation(Device device, string Location)
        {
            if (string.IsNullOrWhiteSpace(Location))
                device.Location = null;
            else
                device.Location = Location.Trim();
            Database.SaveChanges();
        }
        private void UpdateAssignedUserId(Device device, string UserId)
        {
            var daus = Database.DeviceUserAssignments.Where(m => m.DeviceSerialNumber == device.SerialNumber && m.UnassignedDate == null);
            User u = null;
            if (!string.IsNullOrEmpty(UserId))
            {
                u = UserService.GetUser(UserId, Database, true);

                if (u == null)
                    throw new Exception("Invalid Username");
            }

            device.AssignDevice(Database, u);
            Database.SaveChanges();
        }
        private void UpdateAllowUnauthenticatedEnrol(Device device, string AllowUnauthenticatedEnrol)
        {
            if (string.IsNullOrEmpty(AllowUnauthenticatedEnrol) || !bool.TryParse(AllowUnauthenticatedEnrol, out var bAllowUnauthenticatedEnrol))
            {
                throw new Exception("Invalid AllowUnauthenticatedEnrol Value");
            }

            if (device.AllowUnauthenticatedEnrol != bAllowUnauthenticatedEnrol)
            {
                device.AllowUnauthenticatedEnrol = bAllowUnauthenticatedEnrol;
                Database.SaveChanges();
            }
        }
        private void UpdateDetailACAdapter(Device device, string ACAdapter)
        {
            if (string.IsNullOrWhiteSpace(ACAdapter))
                device.DeviceDetails.ACAdapter(device, null);
            else
                device.DeviceDetails.ACAdapter(device, ACAdapter.Trim());
            Database.SaveChanges();
        }
        private void UpdateDetailBattery(Device device, string Battery)
        {
            if (string.IsNullOrWhiteSpace(Battery))
                device.DeviceDetails.Battery(device, null);
            else
                device.DeviceDetails.Battery(device, Battery.Trim());
            Database.SaveChanges();
        }
        private void UpdateDetailKeyboard(Device device, string Keyboard)
        {
            if (string.IsNullOrWhiteSpace(Keyboard))
                device.DeviceDetails.Keyboard(device, null);
            else
                device.DeviceDetails.Keyboard(device, Keyboard.Trim());
            Database.SaveChanges();
        }
        #endregion

        #region Device Actions

        [DiscoAuthorize(Claims.Device.Actions.Decommission)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Decommission(string id, int? Reason = null, bool redirect = false)
        {
            if (Reason == null)
                throw new ArgumentNullException(nameof(Reason), "Decommission Reason is required");

            var d = Database.Devices.Find(id);
            Database.Configuration.LazyLoadingEnabled = true;
            if (d != null)
            {
                if (d.CanDecommission())
                {
                    d.OnDecommission((DecommissionReasons)Reason.Value, Database);

                    Database.SaveChanges();
                    if (redirect)
                        return RedirectToAction(MVC.Device.Show(id));
                    else
                        return Ok();
                }
                else
                {
                    return BadRequest("Device's state doesn't allow this action");
                }
            }
            return BadRequest("Invalid Device Serial Number");
        }

        [DiscoAuthorize(Claims.Device.Actions.Recommission)]
        [HttpPost, ValidateAntiForgeryToken]
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
                        return Ok();
                }
                else
                {
                    return BadRequest("Device's state doesn't allow this action");
                }
            }
            return BadRequest("Invalid Device Serial Number");
        }

        [DiscoAuthorize(Claims.Device.Actions.Delete)]
        [HttpPost, ValidateAntiForgeryToken]
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
                        return Ok();
                }
                else
                {
                    return BadRequest("Job's state doesn't allow this action");
                }
            }
            return BadRequest("Invalid Device Serial Number");
        }

        #endregion

        [DiscoAuthorize(Claims.Device.Show)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult LastNetworkLogonDate(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException("id", "The Device Serial Number is required");

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

        #region Device Comments

        [DiscoAuthorize(Claims.Device.ShowComments)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Comments(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            var device = Database.Devices
                .Include(d => d.DeviceComments.Select(l => l.TechUser))
                .Where(d => d.SerialNumber == id).FirstOrDefault();
            if (device == null)
                return BadRequest("Invalid Device Serial Number");

            var results = device.DeviceComments.OrderByDescending(c => c.Timestamp).Select(c => Models.Shared.CommentModel.FromEntity(c)).ToList();
            return Json(results);
        }

        [DiscoAuthorize(Claims.Device.ShowComments)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Comment(int id)
        {
            var entity = Database.DeviceComments
                .Include(c => c.TechUser)
                .FirstOrDefault(c => c.Id == id);

            if (entity == null)
                return BadRequest("Invalid Device Comment Id");

            var comment = Models.Shared.CommentModel.FromEntity(entity);
            return Json(comment);
        }

        [DiscoAuthorize(Claims.Device.Actions.AddComments)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult CommentAdd(string id, string comment = null)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            if (string.IsNullOrWhiteSpace(comment))
                return BadRequest("Comment is required");

            var device = Database.Devices.Find(id);
            if (device == null)
                return BadRequest("Invalid Device Serial Number");

            var entity = new DeviceComment()
            {
                DeviceSerialNumber = device.SerialNumber,
                TechUserId = CurrentUser.UserId,
                Timestamp = DateTime.Now,
                Comments = comment
            };
            Database.DeviceComments.Add(entity);
            Database.SaveChanges();

            return Json(entity.Id);
        }

        [DiscoAuthorizeAny(Claims.Device.Actions.RemoveAnyComments, Claims.Device.Actions.RemoveOwnComments)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult CommentRemove(int id)
        {
            var entity = Database.DeviceComments.Find(id);
            if (entity != null)
            {
                if (entity.TechUserId.Equals(CurrentUser.UserId, StringComparison.OrdinalIgnoreCase))
                    Authorization.RequireAny(Claims.Device.Actions.RemoveAnyComments, Claims.Device.Actions.RemoveOwnComments);
                else
                    Authorization.Require(Claims.Device.Actions.RemoveAnyComments);

                Database.DeviceComments.Remove(entity);
                Database.SaveChanges();
            }
            // Doesn't Exist/Already Deleted - OK
            return Ok();
        }
        #endregion

        #region Device Attachments

        [DiscoAuthorize(Claims.Device.ShowAttachments), OutputCache(Location = System.Web.UI.OutputCacheLocation.Client, Duration = 172800)]
        public virtual ActionResult AttachmentDownload(int id, bool? inline = null)
        {
            var da = Database.DeviceAttachments.Find(id);
            if (da != null)
            {
                var filePath = da.RepositoryFilename(Database);
                if (System.IO.File.Exists(filePath))
                {
                    return File(filePath, da.MimeType, (inline ?? false) ? null : da.Filename);
                }
                else
                {
                    return HttpNotFound("Attachment reference exists, but file not found");
                }
            }
            return HttpNotFound("Invalid Attachment Number");
        }

        [DiscoAuthorize(Claims.Device.ShowAttachments)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult AttachmentDownloadAll(string id)
        {
            var device = Database.Devices
                .Include(u => u.DeviceAttachments)
                .Where(u => u.SerialNumber == id)
                .FirstOrDefault();

            if (device == null || device.DeviceAttachments.Count == 0)
                return NotFound();

            var responseStream = new MemoryStream();
            using (var archive = new ZipArchive(responseStream, ZipArchiveMode.Create, true))
            {
                foreach (var attachment in device.DeviceAttachments)
                {
                    var repoFileName = attachment.RepositoryFilename(Database);
                    if (System.IO.File.Exists(repoFileName))
                    {
                        var fileName = $"{Path.GetFileNameWithoutExtension(attachment.Filename)}-{attachment.Timestamp:yyyyMMdd-HHmmss}{Path.GetExtension(attachment.Filename)}";

                        var entry = archive.CreateEntry(fileName, CompressionLevel.Fastest);
                        entry.LastWriteTime = attachment.Timestamp;
                        using (var entryStream = entry.Open())
                        {
                            using (var attachmentStream = System.IO.File.OpenRead(repoFileName))
                            {
                                attachmentStream.CopyTo(entryStream);
                            }
                        }
                    }
                }
            }
            responseStream.Position = 0;
            return File(responseStream, "application/zip", $"{device.SerialNumber}_DeviceAttachments_{DateTime.Now:yyyyMMdd-HHmmss}.zip");
        }

        [DiscoAuthorize(Claims.Device.ShowAttachments), OutputCache(Location = System.Web.UI.OutputCacheLocation.Client, Duration = 172800)]
        public virtual ActionResult AttachmentThumbnail(int id)
        {
            var da = Database.DeviceAttachments.Find(id);
            if (da != null)
            {
                if (da.WaitForThumbnailGeneration(Database, out var thumbPath, out var mimeType))
                    return File(thumbPath, mimeType);
                else
                    return File(ClientSource.Style.Images.AttachmentTypes.MimeTypeIcons.Icon(da.MimeType), "image/png");
            }
            return HttpNotFound("Invalid Attachment Number");
        }

        [DiscoAuthorize(Claims.Device.Actions.AddAttachments)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult AttachmentUpload(string id, string comments)
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
                        if (string.IsNullOrEmpty(contentType) || contentType.Equals("unknown/unknown", StringComparison.OrdinalIgnoreCase))
                            contentType = MimeTypes.ResolveMimeType(file.FileName);

                        if (string.IsNullOrWhiteSpace(comments))
                            comments = null;

                        var da = new DeviceAttachment()
                        {
                            DeviceSerialNumber = d.SerialNumber,
                            TechUserId = CurrentUser.UserId,
                            Filename = file.FileName,
                            MimeType = contentType,
                            Timestamp = DateTime.Now,
                            Comments = comments
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
            var da = Database.DeviceAttachments
                .Include(a => a.DocumentTemplate)
                .Include(a => a.TechUser)
                .Where(m => m.Id == id).FirstOrDefault();
            if (da != null)
            {
                var m = new Models.Attachment.AttachmentModel()
                {
                    Attachment = Models.Attachment._AttachmentModel.FromAttachment(da),
                    Result = "OK"
                };

                return Json(m, JsonRequestBehavior.AllowGet);
            }
            return BadRequest("Invalid Attachment Number");
        }

        [DiscoAuthorize(Claims.Device.ShowAttachments)]
        public virtual ActionResult Attachments(string id)
        {
            var d = Database.Devices.Include("DeviceAttachments.DocumentTemplate").Include("DeviceAttachments.TechUser").Where(m => m.SerialNumber == id).FirstOrDefault();
            if (d != null)
            {
                var m = new Models.Attachment.AttachmentsModel()
                {
                    Attachments = d.DeviceAttachments.Select(ua => Models.Attachment._AttachmentModel.FromAttachment(ua)).ToList(),
                    Result = "OK"
                };

                return Json(m, JsonRequestBehavior.AllowGet);
            }
            return BadRequest("Invalid Device Serial Number");
        }

        [DiscoAuthorizeAny(Claims.Job.Actions.RemoveAnyAttachments, Claims.Job.Actions.RemoveOwnAttachments)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult AttachmentRemove(int id)
        {
            var da = Database.DeviceAttachments.Include("TechUser").Where(m => m.Id == id).FirstOrDefault();
            if (da != null)
            {
                if (da.TechUserId.Equals(CurrentUser.UserId, StringComparison.OrdinalIgnoreCase))
                    Authorization.RequireAny(Claims.Device.Actions.RemoveAnyAttachments, Claims.Device.Actions.RemoveOwnAttachments);
                else
                    Authorization.Require(Claims.Device.Actions.RemoveAnyAttachments);

                da.OnDelete(Database);
                Database.SaveChanges();
                return Ok();
            }
            return BadRequest("Invalid Attachment Number");
        }

        [DiscoAuthorize(Claims.Device.Actions.AddAttachments)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> AttachmentOnlineUploadSession(string id)
        {
            var device = Database.Devices.Find(id) ?? throw new InvalidOperationException("Unknown Device Serial Number");

            try
            {
                if (!Database.DiscoConfiguration.IsActivated)
                    throw new InvalidOperationException("Activation is required to use this feature (See: Configuration > System)");

                var (uri, expiration) = await UploadOnlineService.CreateSession(CurrentUser, device);

                UploadOnlineSyncTask.ScheduleInOneHour();

                return Json(new
                {
                    Success = true,
                    Expiration = expiration.ToUnixEpoc(),
                    SessionUri = uri.ToString(),
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region Importing
        internal const string ImportSessionCacheKey = "DeviceImportContext_{0}";

        internal static void Import_StoreContext(IDeviceImportContext Context)
        {
            string key = string.Format(ImportSessionCacheKey, Context.SessionId);
            HttpRuntime.Cache.Insert(key, Context, null, DateTime.Now.AddMinutes(60), Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
        }
        internal static IDeviceImportContext Import_RetrieveContext(string SessionId, bool Remove = false)
        {
            string key = string.Format(ImportSessionCacheKey, SessionId);
            IDeviceImportContext context = HttpRuntime.Cache.Get(key) as IDeviceImportContext;

            if (Remove && context != null)
                HttpRuntime.Cache.Remove(key);

            return context;
        }

        [DiscoAuthorize(Claims.Device.Actions.Import)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult ImportBegin(HttpPostedFileBase ImportFile, bool HasHeader)
        {
            if (ImportFile == null || ImportFile.ContentLength == 0)
                throw new ArgumentNullException("ImportFile");

            var fileName = ImportFile.FileName;
            if (fileName.Contains(@"\"))
                fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);

            var context = DeviceImport.BeginImport(Database, fileName, HasHeader, ImportFile.InputStream);
            Import_StoreContext(context);

            return RedirectToAction(MVC.Device.ImportHeaders(context.SessionId));
        }

        [DiscoAuthorize(Claims.Device.Actions.Import)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult ImportParse(string Id, List<DeviceImportFieldTypes> Headers)
        {
            if (string.IsNullOrWhiteSpace(Id))
                throw new ArgumentNullException(nameof(Id));

            var context = Import_RetrieveContext(Id)
                ?? throw new ArgumentException("The Import Session Id is invalid or the session timed out (60 minutes), try importing again", nameof(Id));

            context.UpdateColumnTypes(Headers);

            var status = DeviceImportParseTask.ScheduleNow(context);

            var finishedUrl = MVC.Device.ImportReview(context.SessionId);

            status.SetFinishedUrl(Url.Action(finishedUrl));

            if (status.WaitUntilFinished(TimeSpan.FromSeconds(2)))
                return RedirectToAction(finishedUrl);
            else
                return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
        }

        [DiscoAuthorize(Claims.Device.Actions.Import)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult ImportApply(string Id)
        {
            if (string.IsNullOrWhiteSpace(Id))
                throw new ArgumentNullException(nameof(Id));

            var context = Import_RetrieveContext(Id)
                ?? throw new ArgumentException("The Import Session Id is invalid or the session timed out (60 minutes), try importing again", nameof(Id));

            var status = DeviceImportApplyTask.ScheduleNow(context);
            status.SetFinishedUrl(Url.Action(MVC.Device.Import(context.SessionId)));

            return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
        }

        [DiscoAuthorize(Claims.Device.Actions.Import)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult DeviceBatchDecommission(int id, DecommissionReasons? decommissionReason = null, bool? unassignUsers = null)
        {
            var deviceBatch = Database.DeviceBatches.Find(id)
                ?? throw new ArgumentException("Invalid Device Batch Id", nameof(id));
            if (decommissionReason == null)
                throw new ArgumentNullException(nameof(decommissionReason), "Decommission Reason is required");

            var context = DeviceImport.BeginDecommissionImport(Database, deviceBatch, decommissionReason.Value, unassignUsers.GetValueOrDefault());
            Import_StoreContext(context);

            return RedirectToAction(MVC.Device.ImportReview(context.SessionId));
        }

        [DiscoAuthorize(Claims.Device.Actions.Import)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult DeviceProfileDecommission(int id, DecommissionReasons? decommissionReason = null, bool? unassignUsers = null)
        {
            var deviceProfile = Database.DeviceProfiles.Find(id)
                ?? throw new ArgumentException("Invalid Device Profile Id", nameof(id));
            if (decommissionReason == null)
                throw new ArgumentNullException(nameof(decommissionReason), "Decommission Reason is required");
            var context = DeviceImport.BeginDecommissionImport(Database, deviceProfile, decommissionReason.Value, unassignUsers.GetValueOrDefault());
            Import_StoreContext(context);
            return RedirectToAction(MVC.Device.ImportReview(context.SessionId));
        }

        [DiscoAuthorize(Claims.Device.Actions.Import)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult DeviceModelDecommission(int id, DecommissionReasons? decommissionReason = null, bool? unassignUsers = null)
        {
            var deviceModel = Database.DeviceModels.Find(id)
                ?? throw new ArgumentException("Invalid Device Model Id", nameof(id));
            if (decommissionReason == null)
                throw new ArgumentNullException(nameof(decommissionReason), "Decommission Reason is required");
            var context = DeviceImport.BeginDecommissionImport(Database, deviceModel, decommissionReason.Value, unassignUsers.GetValueOrDefault());
            Import_StoreContext(context);
            return RedirectToAction(MVC.Device.ImportReview(context.SessionId));
        }

        #endregion

        #region Exporting

        [DiscoAuthorize(Claims.Device.Actions.Export)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Export(ExportModel model)
        {
            if (model == null || model.Options == null)
                throw new ArgumentNullException("Model");

            // Write Options to Configuration
            Database.DiscoConfiguration.Devices.LastExportOptions = model.Options;
            Database.SaveChanges();

            // Start Export
            var exportContext = new DeviceExport(model.Options);
            var taskContext = ExportTask.ScheduleNowCacheResult(exportContext, id => Url.Action(MVC.Device.Export(id, null, null)));

            // Try waiting for completion
            if (taskContext.TaskStatus.WaitUntilFinished(TimeSpan.FromSeconds(2)))
                return RedirectToAction(MVC.Device.Export(taskContext.Id, null, null));
            else
                return RedirectToAction(MVC.Config.Logging.TaskStatus(taskContext.TaskStatus.SessionId));
        }

        [DiscoAuthorize(Claims.Device.Actions.Export)]
        public virtual ActionResult ExportRetrieve(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentNullException(nameof(id));

            if (!ExportTask.TryFromCache(id, out var context))
                throw new ArgumentException("The export id specified is invalid, or the export data expired (60 minutes)", nameof(id));

            if (context.Result == null || context.Result.Result == null)
                throw new ArgumentException("The export session is still running, or failed to complete successfully", nameof(id));

            if (context.Result.RecordCount == 0)
                throw new ArgumentException("No records were found to export", nameof(id));

            var fileStream = context.Result.Result;

            return this.File(fileStream.GetBuffer(), 0, (int)fileStream.Length, context.Result.MimeType, context.Result.Filename);
        }

        [DiscoAuthorizeAll(Claims.Config.ManageSavedExports, Claims.Device.Actions.Export)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult SaveExport(ExportModel model)
        {
            Database.DiscoConfiguration.Devices.LastExportOptions = model.Options;

            var export = new DeviceExport(model.Options);
            var savedExport = SavedExports.SaveExport(export, Database, CurrentUser);

            return RedirectToAction(MVC.Config.Export.Create(savedExport.Id));
        }

        #endregion

        [DiscoAuthorize(Claims.DiscoAdminAccount)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult MigrateDeviceMacAddressesFromLog()
        {
            var taskStatus = Disco.Services.Devices.Enrolment.LogMacAddressImportingTask.ScheduleImmediately();
            return RedirectToAction(MVC.Config.Logging.TaskStatus(taskStatus.SessionId));
        }
    }
}
