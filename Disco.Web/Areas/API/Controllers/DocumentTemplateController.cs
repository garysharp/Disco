using Disco.BI.Extensions;
using Disco.Models.Repository;
using Disco.Models.Services.Documents;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Documents;
using Disco.Services.Documents.ManagedGroups;
using Disco.Services.Exporting;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.DocumentHandlerProvider;
using Disco.Services.Tasks;
using Disco.Services.Users;
using Disco.Services.Web;
using Disco.Web.Areas.API.Models.DocumentTemplate;
using Disco.Web.Areas.Config.Models.DocumentTemplate;
using Disco.Web.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class DocumentTemplateController : AuthorizedDatabaseController
    {

        const string pDescription = "description";
        const string pScope = "scope";
        const string pFilterExpression = "filterexpression";
        const string pOnGenerateExpression = "ongenerateexpression";
        const string pOnImportAttachmentExpression = "onimportattachmentexpression";
        const string pFlattenForm = "flattenform";
        const string pIsHidden = "ishidden";

        [DiscoAuthorize(Claims.Config.DocumentTemplate.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Update(string id, string key, string value = null, bool redirect = false)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new ArgumentNullException("id");
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");

                ScheduledTaskStatus resultTask = null;
                var documentTemplate = Database.DocumentTemplates.Find(id);

                if (documentTemplate != null)
                {
                    switch (key.ToLower())
                    {
                        case pDescription:
                            UpdateDescription(documentTemplate, value);
                            break;
                        case pScope:
                            resultTask = UpdateScope(documentTemplate, value);
                            break;
                        case pFilterExpression:
                            Authorization.Require(Claims.Config.DocumentTemplate.ConfigureFilterExpression);
                            UpdateFilterExpression(documentTemplate, value);
                            break;
                        case pOnGenerateExpression:
                            UpdateOnGenerateExpression(documentTemplate, value);
                            break;
                        case pOnImportAttachmentExpression:
                            UpdateOnImportAttachmentExpression(documentTemplate, value);
                            break;
                        case pFlattenForm:
                            UpdateFlattenForm(documentTemplate, value);
                            break;
                        case pIsHidden:
                            UpdateIsHidden(documentTemplate, value);
                            break;
                        default:
                            throw new Exception("Invalid Update Key");
                    }
                }
                else
                {
                    throw new Exception("Invalid Document Template Id");
                }
                if (redirect)
                    if (resultTask == null)
                    {
                        return RedirectToAction(MVC.Config.DocumentTemplate.Index(documentTemplate.Id));
                    }
                    else
                    {
                        resultTask.SetFinishedUrl(Url.Action(MVC.Config.DocumentTemplate.Index(documentTemplate.Id)));
                        return RedirectToAction(MVC.Config.Logging.TaskStatus(resultTask.SessionId));
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

        [DiscoAuthorize(Claims.Config.DocumentTemplate.Upload)]
        [HttpGet]
        public virtual ActionResult Template(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");
            var documentTemplate = Database.DocumentTemplates.Find(id);
            if (documentTemplate == null)
                throw new ArgumentException("Invalid Document Template Id", "id");

            var filename = documentTemplate.RepositoryFilename(Database);
            if (System.IO.File.Exists(filename))
            {
                return File(filename, DocumentTemplate.PdfMimeType, $"{documentTemplate.Id}.pdf");
            }
            else
            {
                throw new InvalidOperationException("Template not found");
            }
        }

        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.Upload, Claims.Config.DocumentTemplate.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Template(string id, bool redirect, HttpPostedFileBase Template)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new ArgumentNullException("id");
                var documentTemplate = Database.DocumentTemplates.Find(id);
                if (documentTemplate == null)
                    throw new ArgumentException("Invalid Document Template Id", "id");

                documentTemplate.SavePdfTemplate(Database, Template.InputStream);

                if (redirect)
                    return RedirectToAction(MVC.Config.DocumentTemplate.Index(documentTemplate.Id));
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

        [DiscoAuthorize(Claims.Config.DocumentTemplate.Show)]
        [HttpGet]
        public virtual ActionResult TemplatePreview(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");
            var documentTemplate = Database.DocumentTemplates.Find(id);
            if (documentTemplate == null)
                throw new ArgumentException("Invalid Document Template Id", "id");

            var imageStream = new MemoryStream();
            using (var previewImage = documentTemplate.GenerateTemplatePreview(Database, 450, 8, true))
            {
                if (previewImage == null)
                {
                    throw new InvalidOperationException("Template not found");
                }
                previewImage.SavePng(imageStream);
            }
            imageStream.Position = 0;

            return File(imageStream, "image/png");
        }

        #region Update Shortcut Methods
        [DiscoAuthorize(Claims.Config.DocumentTemplate.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateDescription(string id, string Description = null, bool redirect = false)
        {
            return Update(id, pDescription, Description, redirect);
        }
        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.Configure, Claims.Config.DocumentTemplate.ConfigureFilterExpression)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateFilterExpression(string id, string FilterExpression = null, bool redirect = false)
        {
            return Update(id, pFilterExpression, FilterExpression, redirect);
        }
        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.Configure, Claims.Config.DocumentTemplate.ConfigureFilterExpression)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateOnGenerateExpression(string id, string OnGenerateExpression = null, bool redirect = false)
        {
            return Update(id, pOnGenerateExpression, OnGenerateExpression, redirect);
        }
        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.Configure, Claims.Config.DocumentTemplate.ConfigureFilterExpression)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateOnImportAttachmentExpression(string id, string OnImportAttachmentExpression = null, bool redirect = false)
        {
            return Update(id, pOnImportAttachmentExpression, OnImportAttachmentExpression, redirect);
        }
        [DiscoAuthorize(Claims.Config.DocumentTemplate.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateFlattenForm(string id, string FlattenForm = null, bool redirect = false)
        {
            return Update(id, pFlattenForm, FlattenForm, redirect);
        }
        [DiscoAuthorize(Claims.Config.DocumentTemplate.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateIsHidden(string id, string IsHidden = null, bool redirect = false)
        {
            return Update(id, pIsHidden, IsHidden, redirect);
        }
        [DiscoAuthorize(Claims.Config.DocumentTemplate.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateScope(string id, string Scope = null, bool redirect = false)
        {
            return Update(id, pScope, Scope, redirect);
        }
        [DiscoAuthorize(Claims.Config.DocumentTemplate.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateJobSubTypes(string id, List<string> JobSubTypes = null, bool redirect = false)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new ArgumentNullException("id");
                var documentTemplate = Database.DocumentTemplates.Find(id);

                UpdateJobSubTypes(documentTemplate, JobSubTypes);

                if (redirect)
                    return RedirectToAction(MVC.Config.DocumentTemplate.Index(documentTemplate.Id));
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

        [DiscoAuthorize(Claims.Config.DocumentTemplate.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateDevicesLinkedGroup(string id, string GroupId = null, DateTime? FilterBeginDate = null, bool redirect = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentNullException("id");

                var documentTemplate = Database.DocumentTemplates.Find(id);
                if (documentTemplate == null)
                    throw new ArgumentException("Invalid Document Template Id", "id");

                var syncTaskStatus = UpdateDevicesLinkedGroup(documentTemplate, GroupId, FilterBeginDate);
                if (redirect)
                    if (syncTaskStatus == null)
                        return RedirectToAction(MVC.Config.DocumentTemplate.Index(documentTemplate.Id));
                    else
                    {
                        syncTaskStatus.SetFinishedUrl(Url.Action(MVC.Config.DocumentTemplate.Index(documentTemplate.Id)));
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

        [DiscoAuthorize(Claims.Config.DocumentTemplate.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateUsersLinkedGroup(string id, string GroupId = null, DateTime? FilterBeginDate = null, bool redirect = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentNullException("id");

                var documentTemplate = Database.DocumentTemplates.Find(id);
                if (documentTemplate == null)
                    throw new ArgumentException("Invalid Document Template Id", "id");

                var syncTaskStatus = UpdateUsersLinkedGroup(documentTemplate, GroupId, FilterBeginDate);
                if (redirect)
                    if (syncTaskStatus == null)
                        return RedirectToAction(MVC.Config.DocumentTemplate.Index(documentTemplate.Id));
                    else
                    {
                        syncTaskStatus.SetFinishedUrl(Url.Action(MVC.Config.DocumentTemplate.Index(documentTemplate.Id)));
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
        private void UpdateDescription(DocumentTemplate documentTemplate, string Description)
        {
            if (!string.IsNullOrWhiteSpace(Description))
            {
                documentTemplate.Description = Description.Trim();
                Database.SaveChanges();
                return;
            }
            throw new Exception("Invalid Description");
        }
        private ScheduledTaskStatus UpdateScope(DocumentTemplate documentTemplate, string Scope)
        {
            if (string.IsNullOrWhiteSpace(Scope) || !DocumentTemplate.DocumentTemplateScopes.ToList().Contains(Scope))
                throw new ArgumentException("Invalid Scope", "Scope");

            Database.Configuration.LazyLoadingEnabled = true;

            if (documentTemplate.Scope != Scope)
            {

                documentTemplate.Scope = Scope;

                if (documentTemplate.Scope != DocumentTemplate.DocumentTemplateScopes.Job &&
                    documentTemplate.JobSubTypes != null)
                {
                    foreach (var st in documentTemplate.JobSubTypes.ToArray())
                        documentTemplate.JobSubTypes.Remove(st);
                }

                Database.SaveChanges();

                // Trigger Managed Group Sync
                var managedGroups = new ADManagedGroup[] {
                    DocumentTemplateDevicesManagedGroup.Initialize(documentTemplate),
                    DocumentTemplateUsersManagedGroup.Initialize(documentTemplate)
                };

                if (managedGroups.Any(mg => mg != null)) // Sync Group
                    return ADManagedGroupsSyncTask.ScheduleSync(managedGroups.Where(mg => mg != null));
            }

            return null;
        }
        private void UpdateFilterExpression(DocumentTemplate documentTemplate, string FilterExpression)
        {
            if (string.IsNullOrWhiteSpace(FilterExpression))
            {
                documentTemplate.FilterExpression = null;
            }
            else
            {
                documentTemplate.FilterExpression = FilterExpression.Trim();
            }
            // Invalidate Cache
            documentTemplate.FilterExpressionInvalidateCache();

            Database.SaveChanges();
        }
        private void UpdateOnGenerateExpression(DocumentTemplate documentTemplate, string OnGenerateExpression)
        {
            if (string.IsNullOrWhiteSpace(OnGenerateExpression))
            {
                documentTemplate.OnGenerateExpression = null;
            }
            else
            {
                documentTemplate.OnGenerateExpression = OnGenerateExpression.Trim();
            }
            // Invalidate Cache
            documentTemplate.OnGenerateExpressionInvalidateCache();

            Database.SaveChanges();
        }
        private void UpdateOnImportAttachmentExpression(DocumentTemplate documentTemplate, string OnImportAttachmentExpression)
        {
            if (string.IsNullOrWhiteSpace(OnImportAttachmentExpression))
            {
                documentTemplate.OnImportAttachmentExpression = null;
            }
            else
            {
                documentTemplate.OnImportAttachmentExpression = OnImportAttachmentExpression.Trim();
            }
            // Invalidate Cache
            documentTemplate.OnImportAttachmentExpressionInvalidateCache();

            Database.SaveChanges();
        }
        private void UpdateFlattenForm(DocumentTemplate documentTemplate, string FlattenForm)
        {
            if (string.IsNullOrWhiteSpace(FlattenForm))
            {
                documentTemplate.FlattenForm = false;
            }
            else
            {
                if (bool.TryParse(FlattenForm, out var ff))
                    documentTemplate.FlattenForm = ff;
                else
                    throw new Exception("Invalid Boolean Format");
            }

            Database.SaveChanges();
        }
        private void UpdateIsHidden(DocumentTemplate documentTemplate, string IsHidden)
        {
            if (string.IsNullOrWhiteSpace(IsHidden))
            {
                documentTemplate.IsHidden = false;
            }
            else
            {
                if (bool.TryParse(IsHidden, out var value))
                    documentTemplate.IsHidden = value;
                else
                    throw new Exception("Invalid Boolean Format");
            }

            Database.SaveChanges();
        }
        private void UpdateJobSubTypes(DocumentTemplate documentTemplate, List<string> JobSubTypes)
        {
            Database.Configuration.LazyLoadingEnabled = true;

            // Remove All Existing
            if (documentTemplate.JobSubTypes != null)
            {
                foreach (var st in documentTemplate.JobSubTypes.ToArray())
                    documentTemplate.JobSubTypes.Remove(st);
            }

            // Add New
            if (JobSubTypes != null && JobSubTypes.Count > 0)
            {
                var subTypes = new List<JobSubType>();
                foreach (var stId in JobSubTypes)
                {
                    var typeId = stId.Substring(0, stId.IndexOf("_"));
                    var subTypeId = stId.Substring(stId.IndexOf("_") + 1);
                    var subType = Database.JobSubTypes.FirstOrDefault(jst => jst.JobTypeId == typeId && jst.Id == subTypeId);
                    subTypes.Add(subType);
                }
                documentTemplate.JobSubTypes = subTypes;
            }
            Database.SaveChanges();
        }

        private ScheduledTaskStatus UpdateDevicesLinkedGroup(DocumentTemplate DocumentTemplate, string DevicesLinkedGroup, DateTime? FilterBeginDate)
        {
            var configJson = ADManagedGroup.ValidConfigurationToJson(DocumentTemplateDevicesManagedGroup.GetKey(DocumentTemplate), DevicesLinkedGroup, FilterBeginDate);

            if (DocumentTemplate.DevicesLinkedGroup != configJson)
            {
                DocumentTemplate.DevicesLinkedGroup = configJson;
                Database.SaveChanges();

                var managedGroup = DocumentTemplateDevicesManagedGroup.Initialize(DocumentTemplate);
                if (managedGroup != null) // Sync Group
                    return ADManagedGroupsSyncTask.ScheduleSync(managedGroup);
            }

            return null;
        }

        private ScheduledTaskStatus UpdateUsersLinkedGroup(DocumentTemplate DocumentTemplate, string UsersLinkedGroup, DateTime? FilterBeginDate)
        {
            var configJson = ADManagedGroup.ValidConfigurationToJson(DocumentTemplateUsersManagedGroup.GetKey(DocumentTemplate), UsersLinkedGroup, FilterBeginDate);

            if (DocumentTemplate.UsersLinkedGroup != configJson)
            {
                DocumentTemplate.UsersLinkedGroup = configJson;
                Database.SaveChanges();

                var managedGroup = DocumentTemplateUsersManagedGroup.Initialize(DocumentTemplate);
                if (managedGroup != null) // Sync Group
                    return ADManagedGroupsSyncTask.ScheduleSync(managedGroup);
            }

            return null;
        }
        #endregion

        #region Actions

        [DiscoAuthorize(Claims.Config.DocumentTemplate.UndetectedPages), OutputCache(NoStore = true, Duration = 0)]
        public virtual ActionResult ImporterThumbnail(Guid SessionId, int PageNumber)
        {
            var dataStoreSessionPagesCacheLocation = DataStore.CreateLocation(Database, "Cache\\DocumentDropBox_SessionPages");
            var filename = Path.Combine(dataStoreSessionPagesCacheLocation, $"{SessionId}-{PageNumber}");
            if (System.IO.File.Exists(filename))
                return File(filename, "image/png");
            else
                return File("~/ClientSource/Style/Images/Status/fileBroken256.png", "image/png");
        }

        [DiscoAuthorize(Claims.Config.DocumentTemplate.UndetectedPages)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult ImporterUndetectedFiles()
        {
            var undetectedLocation = DataStore.CreateLocation(Database, "DocumentDropBox_Unassigned");
            var undetectedDirectory = new DirectoryInfo(undetectedLocation);
            var m = undetectedDirectory.GetFiles("*.pdf").Select(f => new ImporterUndetectedFilesModel()
            {
                Id = Path.GetFileNameWithoutExtension(f.Name),
                Timestamp = f.CreationTime.ToFullDateTime(),
                TimestampUnixEpoc = f.CreationTime.ToUnixEpoc()
            }).ToArray();

            return Json(m);
        }

        [DiscoAuthorize(Claims.Config.DocumentTemplate.UndetectedPages)]
        public virtual ActionResult ImporterUndetectedDataIdLookup(string id, string term, int limitCount = ActiveDirectory.DefaultSearchResultLimit)
        {
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrWhiteSpace(term))
            {
                string searchScope;
                if (id.StartsWith("--"))
                {
                    switch (id.ToUpper())
                    {
                        case "--DEVICE":
                            searchScope = DocumentTemplate.DocumentTemplateScopes.Device;
                            break;
                        case "--JOB":
                            searchScope = DocumentTemplate.DocumentTemplateScopes.Job;
                            break;
                        case "--USER":
                            searchScope = DocumentTemplate.DocumentTemplateScopes.User;
                            break;
                        default:
                            searchScope = null;
                            break;
                    }
                }
                else
                {
                    var documentTemplate = Database.DocumentTemplates.Find(id);
                    if (documentTemplate != null)
                        searchScope = documentTemplate.Scope;
                    else
                        searchScope = null;
                }
                if (searchScope != null)
                {
                    ImporterUndetectedDataIdLookupModel[] results;
                    switch (searchScope)
                    {
                        case DocumentTemplate.DocumentTemplateScopes.Device:
                            results = Disco.Services.Searching.Search.SearchDevices(Database, term, limitCount).Select(sr => ImporterUndetectedDataIdLookupModel.FromSearchResultItem(sr)).ToArray();
                            break;
                        case DocumentTemplate.DocumentTemplateScopes.Job:
                            results = Disco.Services.Searching.Search.SearchJobsTable(Database, term, limitCount, false).Items.Select(sr => ImporterUndetectedDataIdLookupModel.FromSearchResultItem(sr)).ToArray();
                            break;
                        case DocumentTemplate.DocumentTemplateScopes.User:
                            results = Disco.Services.Searching.Search.SearchUsers(Database, term, false, limitCount).Select(sr => ImporterUndetectedDataIdLookupModel.FromSearchResultItem(sr)).ToArray();
                            break;
                        default:
                            results = null;
                            break;
                    }
                    if (results != null)
                        return Json(results, JsonRequestBehavior.AllowGet);
                }

            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        [DiscoAuthorize(Claims.Config.DocumentTemplate.UndetectedPages)]
        [HttpGet]
        public virtual ActionResult ImporterUndetectedFile(string id, bool? Source, bool? Thumbnail)
        {
            if (!Regex.IsMatch(id, @"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}_\d+$"))
                return BadRequest("Invalid page identifier");

            var undetectedLocation = DataStore.CreateLocation(Database, "DocumentDropBox_Unassigned");
            if (Source.HasValue && Source.Value)
            {
                var filename = Path.Combine(undetectedLocation, $"{id}.pdf");
                if (System.IO.File.Exists(filename))
                    return File(filename, DocumentTemplate.PdfMimeType);
                else
                    return HttpNotFound();
            }
            else
            {
                if (Thumbnail.HasValue && Thumbnail.Value)
                {
                    var filename = Path.Combine(undetectedLocation, $"{id}_thumbnail.png");
                    if (System.IO.File.Exists(filename))
                        return File(filename, "image/png");
                    else
                        return File(Links.ClientSource.Style.Images.Status.fileBroken256_png, "image/png");
                }
                else
                {
                    var filename = Path.Combine(undetectedLocation, $"{id}.jpg");
                    if (System.IO.File.Exists(filename))
                        return File(filename, "image/jpeg");
                    else
                        return File(Links.ClientSource.Style.Images.Status.fileBroken256_png, "image/png");
                }
            }
        }

        [DiscoAuthorize(Claims.Config.DocumentTemplate.UndetectedPages)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult ImporterUndetectedAssign(string id, string DocumentTemplateId, string DataId)
        {
            if (!Regex.IsMatch(id, @"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}_\d+$"))
                return BadRequest("Invalid page identifier");

            var undetectedLocation = DataStore.CreateLocation(Database, "DocumentDropBox_Unassigned");
            var filename = Path.Combine(undetectedLocation, $"{id}.pdf");
            var identifier = DocumentUniqueIdentifier.Create(Database, DocumentTemplateId, DataId, UserService.CurrentUser.UserId, DateTime.Now, 0);

            if (Disco.Services.Documents.AttachmentImport.Importer.ImportPdfAttachment(identifier, Database, filename) != null)
            {
                // Delete File
                System.IO.File.Delete(filename);

                // Delete Thumbnail/Preview
                var thumbnailFilename = Path.Combine(undetectedLocation, $"{id}_thumbnail.png");
                if (System.IO.File.Exists(thumbnailFilename))
                    System.IO.File.Delete(thumbnailFilename);
                var previewFilename = Path.Combine(undetectedLocation, $"{id}.jpg");
                if (System.IO.File.Exists(previewFilename))
                    System.IO.File.Delete(previewFilename);

                return Ok();
            }
            else
            {
                return BadRequest("Unable to Import File with the supplied parameters");
            }
        }

        [DiscoAuthorize(Claims.Config.DocumentTemplate.UndetectedPages)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult ImporterUndetectedDelete(string id)
        {
            if (!Regex.IsMatch(id, @"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}_\d+$"))
                return BadRequest("Invalid page identifier");

            var undetectedLocation = DataStore.CreateLocation(Database, "DocumentDropBox_Unassigned");
            var filename = Path.Combine(undetectedLocation, $"{id}.pdf");
            if (System.IO.File.Exists(filename))
            {
                // Delete File
                System.IO.File.Delete(filename);

                // Delete Thumbnail/Preview
                var thumbnailFilename = Path.Combine(undetectedLocation, $"{id}_thumbnail.png");
                if (System.IO.File.Exists(thumbnailFilename))
                    System.IO.File.Delete(thumbnailFilename);
                var previewFilename = Path.Combine(undetectedLocation, $"{id}.jpg");
                if (System.IO.File.Exists(previewFilename))
                    System.IO.File.Delete(previewFilename);

                return Ok();
            }
            else
            {
                return BadRequest("File Not Found");
            }
        }

        [DiscoAuthorizeAll(Claims.Config.DeviceModel.Show, Claims.Config.DocumentTemplate.BulkGenerate)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult BulkGenerateDeviceModel(string id, int deviceGroupId)
        {
            var template = Database.DocumentTemplates.FirstOrDefault(t => t.Id == id);
            if (template == null)
                return HttpNotFound("Document Template not found");

            var deviceModel = Database.DeviceModels.FirstOrDefault(m => m.Id == deviceGroupId);
            if (deviceModel is null)
                return HttpNotFound("Device Model not found");

            List<string> dataIds;
            switch (template.AttachmentType)
            {
                case AttachmentTypes.Device:
                    dataIds = Database.Devices.Where(d => d.DeviceModelId == deviceModel.Id && d.DecommissionedDate == null).Select(d => d.SerialNumber).ToList();
                    break;
                case AttachmentTypes.Job:
                    dataIds = Database.Jobs.Where(j => j.ClosedDate == null && j.Device.DeviceModelId == deviceModel.Id).Select(j => j.Id).AsEnumerable().Select(j => j.ToString()).ToList();
                    break;
                case AttachmentTypes.User:
                    dataIds = Database.Users.Where(u => u.DeviceUserAssignments.Any(a => a.UnassignedDate == null && a.Device.DeviceModelId == deviceModel.Id)).Select(u => u.UserId).ToList();
                    break;
                default:
                    throw new NotSupportedException("The template type is not supported");
            }

            if (!dataIds.Any())
                return HttpNotFound($"No {template.AttachmentType} targets in scope");

            return BulkGenerate(template.Id, string.Join(Environment.NewLine, dataIds));
        }

        [DiscoAuthorizeAll(Claims.Config.DeviceProfile.Show, Claims.Config.DocumentTemplate.BulkGenerate)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult BulkGenerateDeviceProfile(string id, int deviceGroupId)
        {
            var template = Database.DocumentTemplates.FirstOrDefault(t => t.Id == id);
            if (template == null)
                return HttpNotFound("Document Template not found");

            var deviceProfile = Database.DeviceProfiles.FirstOrDefault(m => m.Id == deviceGroupId);
            if (deviceProfile is null)
                return HttpNotFound("Device Profile not found");

            List<string> dataIds;
            switch (template.AttachmentType)
            {
                case AttachmentTypes.Device:
                    dataIds = Database.Devices.Where(d => d.DeviceProfileId == deviceProfile.Id && d.DecommissionedDate == null).Select(d => d.SerialNumber).ToList();
                    break;
                case AttachmentTypes.Job:
                    dataIds = Database.Jobs.Where(j => j.ClosedDate == null && j.Device.DeviceProfileId == deviceProfile.Id).Select(j => j.Id).AsEnumerable().Select(j => j.ToString()).ToList();
                    break;
                case AttachmentTypes.User:
                    dataIds = Database.Users.Where(u => u.DeviceUserAssignments.Any(a => a.UnassignedDate == null && a.Device.DeviceProfileId == deviceProfile.Id)).Select(u => u.UserId).ToList();
                    break;
                default:
                    throw new NotSupportedException("The template type is not supported");
            }

            if (!dataIds.Any())
                return HttpNotFound($"No {template.AttachmentType} targets in scope");

            return BulkGenerate(template.Id, string.Join(Environment.NewLine, dataIds));
        }

        [DiscoAuthorizeAll(Claims.Config.DeviceBatch.Show, Claims.Config.DocumentTemplate.BulkGenerate)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult BulkGenerateDeviceBatch(string id, int deviceGroupId)
        {
            var template = Database.DocumentTemplates.FirstOrDefault(t => t.Id == id);
            if (template == null)
                return HttpNotFound("Document Template not found");

            var deviceBatch = Database.DeviceBatches.FirstOrDefault(m => m.Id == deviceGroupId);
            if (deviceBatch is null)
                return HttpNotFound("Device Batch not found");

            List<string> dataIds;
            switch (template.AttachmentType)
            {
                case AttachmentTypes.Device:
                    dataIds = Database.Devices.Where(d => d.DeviceBatchId == deviceBatch.Id && d.DecommissionedDate == null).Select(d => d.SerialNumber).ToList();
                    break;
                case AttachmentTypes.Job:
                    dataIds = Database.Jobs.Where(j => j.ClosedDate == null && j.Device.DeviceBatchId == deviceBatch.Id).Select(j => j.Id).AsEnumerable().Select(j => j.ToString()).ToList();
                    break;
                case AttachmentTypes.User:
                    dataIds = Database.Users.Where(u => u.DeviceUserAssignments.Any(a => a.UnassignedDate == null && a.Device.DeviceBatchId == deviceBatch.Id)).Select(u => u.UserId).ToList();
                    break;
                default:
                    throw new NotSupportedException("The template type is not supported");
            }

            if (!dataIds.Any())
                return HttpNotFound($"No {template.AttachmentType} targets in scope");

            return BulkGenerate(template.Id, string.Join(Environment.NewLine, dataIds));
        }

        [DiscoAuthorize(Claims.Config.DocumentTemplate.BulkGenerate)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult BulkGenerate(string id, string dataIds = null, bool insertBlankPage = false)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");
            if (string.IsNullOrEmpty(dataIds))
                throw new ArgumentNullException("DataIds");
            var documentTemplate = Database.DocumentTemplates.Find(id);
            if (documentTemplate == null)
                throw new ArgumentException("Invalid Document Template Id", "id");

            switch (documentTemplate.Scope)
            {
                case DocumentTemplate.DocumentTemplateScopes.Device:
                    Authorization.Require(Claims.Device.Actions.GenerateDocuments);
                    break;
                case DocumentTemplate.DocumentTemplateScopes.Job:
                    Authorization.Require(Claims.Job.Actions.GenerateDocuments);
                    break;
                case DocumentTemplate.DocumentTemplateScopes.User:
                    Authorization.Require(Claims.User.Actions.GenerateDocuments);
                    break;
                default:
                    throw new InvalidOperationException("Unknown DocumentType Scope");
            }

            var ids = dataIds.Split(new string[] { Environment.NewLine, ",", ";" }, StringSplitOptions.RemoveEmptyEntries).Select(d => d.Trim()).Where(d => !string.IsNullOrEmpty(d)).ToList();
            var timeStamp = DateTime.Now;

            var taskStatus = DocumentBulkGenerateTask.ScheduleNow(BI.Interop.Pdf.PdfGenerator.GenerateBulkFromTemplate, documentTemplate, UserService.CurrentUser, timeStamp, insertBlankPage, ids);

            var fileName = $"{documentTemplate.Id}_Bulk_{timeStamp:yyyyMMdd-HHmmss}.pdf";
            taskStatus.SetFinishedUrl(Url.Action(MVC.Config.DocumentTemplate.Index(documentTemplate.Id, Guid.Parse(taskStatus.SessionId), fileName)));

            if (!taskStatus.WaitUntilFinished(TimeSpan.FromSeconds(1)))
                return RedirectToAction(MVC.Config.Logging.TaskStatus(taskStatus.SessionId));

            var stream = DocumentBulkGenerateTask.GetCached(Database, Guid.Parse(taskStatus.SessionId));
            return File(stream, "application/pdf", fileName);
        }

        [DiscoAuthorize(Claims.Config.DocumentTemplate.BulkGenerate)]
        public virtual ActionResult BulkGenerateDownload(Guid id, string fileName)
        {
            var stream = DocumentBulkGenerateTask.GetCached(Database, id);
            return File(stream, "application/pdf", fileName);
        }

        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.BulkGenerate, Claims.User.Actions.GenerateDocuments)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult BulkGenerateAddUsers(string userIds)
        {
            if (string.IsNullOrWhiteSpace(userIds))
                return BadRequest();

            var dataIds = userIds.Split(new string[] { Environment.NewLine, ",", ";" }, StringSplitOptions.RemoveEmptyEntries).Select(d => d.Trim()).Where(d => !string.IsNullOrEmpty(d)).ToList();
            var results = new List<BulkGenerateUserModel>(dataIds.Count);
            foreach (var dataId in dataIds)
            {
                var accountId = ActiveDirectory.ParseDomainAccountId(dataId);

                if (UserService.TryGetUser(accountId, Database, true, out var user))
                {
                    results.Add(new BulkGenerateUserModel()
                    {
                        Id = user.UserId,
                        UserEmailAddress = user.EmailAddress,
                        DisplayName = user.DisplayName,
                        Scope = $"Matched '{dataId}'",
                        IsError = false,
                    });
                    continue;
                }
                else
                {
                    var adObject = ActiveDirectory.RetrieveADObject(accountId, true);

                    if (adObject == null)
                    {
                        results.Add(new BulkGenerateUserModel()
                        {
                            Id = dataId,
                            DisplayName = dataId,
                            Scope = $"Unknown User or Security Group '{dataId}'",
                            IsError = true,
                        });
                        continue;
                    }

                    if (adObject is ADGroup group)
                    {
                        foreach (var adUser in group.GetUserMembersRecursive())
                        {
                            results.Add(new BulkGenerateUserModel()
                            {
                                Id = adUser.Id,
                                DisplayName = adUser.DisplayName,
                                UserEmailAddress = adUser.Email,
                                Scope = $"Group Member '{group.Name}'",
                                IsError = false,
                            });
                        }
                        continue;
                    }
                    else
                    {
                        results.Add(new BulkGenerateUserModel()
                        {
                            Id = dataId,
                            DisplayName = dataId,
                            Scope = $"Unexpected AD Object found at '{adObject.DistinguishedName}'",
                            IsError = true,
                        });
                        continue;
                    }
                }
            }

            return Json(results);
        }

        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.BulkGenerate, Claims.User.Actions.GenerateDocuments)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult BulkGenerateAddGroupMembers(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                return BadRequest();

            var results = new List<BulkGenerateUserModel>();
            var accountId = ActiveDirectory.ParseDomainAccountId(groupId);

            var adObject = ActiveDirectory.RetrieveADObject(accountId, true);

            if (adObject == null)
            {
                results.Add(new BulkGenerateUserModel()
                {
                    Id = groupId,
                    DisplayName = groupId,
                    Scope = $"Unknown Security Group '{groupId}'",
                    IsError = true,
                });
            }
            else if (adObject is ADGroup group)
            {
                foreach (var adUser in group.GetUserMembersRecursive())
                {
                    results.Add(new BulkGenerateUserModel()
                    {
                        Id = adUser.Id,
                        DisplayName = adUser.DisplayName,
                        UserEmailAddress = adUser.Email,
                        Scope = $"Group Member '{group.Name}'",
                        IsError = false,
                    });
                }
            }
            else if (adObject is ADUserAccount user)
            {
                results.Add(new BulkGenerateUserModel()
                {
                    Id = user.Id,
                    DisplayName = user.DisplayName,
                    UserEmailAddress = user.Email,
                    Scope = $"Matched '{groupId}'",
                    IsError = false,
                });
            }
            else
            {
                results.Add(new BulkGenerateUserModel()
                {
                    Id = groupId,
                    DisplayName = groupId,
                    Scope = $"Unexpected AD Object found at '{adObject.DistinguishedName}'",
                    IsError = true,
                });
            }

            return Json(results);
        }

        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.BulkGenerate, Claims.User.Actions.GenerateDocuments)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult BulkGenerateAddUserFlag(int flagId)
        {
            if (flagId <= 0)
                return BadRequest();

            var results = new List<BulkGenerateUserModel>();

            var flag = Database.UserFlags.Include(f => f.UserFlagAssignments.Select(a => a.User)).FirstOrDefault(f => f.Id == flagId);

            if (flag == null)
            {
                results.Add(new BulkGenerateUserModel()
                {
                    Id = flagId.ToString(),
                    DisplayName = flagId.ToString(),
                    Scope = $"Unknown User Flag '{flagId}'",
                    IsError = true,
                });
            }
            else
            {
                var assignments = flag.UserFlagAssignments.Where(a => a.RemovedDate == null).ToList();

                if (assignments.Count == 0)
                {
                    results.Add(new BulkGenerateUserModel()
                    {
                        Id = flag.Name,
                        DisplayName = flag.Name,
                        Scope = $"User Flag has no active assignments",
                        IsError = true,
                    });
                }
                else
                {
                    foreach (var assignment in assignments)
                    {
                        results.Add(new BulkGenerateUserModel()
                        {
                            Id = assignment.UserId,
                            UserEmailAddress = assignment.User.EmailAddress,
                            DisplayName = assignment.User.DisplayName,
                            Scope = $"Assigned User Flag '{flag.Name}'",
                            IsError = false,
                        });
                    }
                }
            }

            return Json(results);
        }

        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.BulkGenerate, Claims.User.Actions.GenerateDocuments)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult BulkGenerateAddDeviceProfile(int deviceProfileId)
        {
            if (deviceProfileId <= 0)
                return BadRequest();

            var results = new List<BulkGenerateUserModel>();

            var profile = Database.DeviceProfiles.Include(p => p.Devices.Select(a => a.AssignedUser)).FirstOrDefault(f => f.Id == deviceProfileId);

            if (profile == null)
            {
                results.Add(new BulkGenerateUserModel()
                {
                    Id = deviceProfileId.ToString(),
                    DisplayName = deviceProfileId.ToString(),
                    Scope = $"Unknown Device Profile '{deviceProfileId}'",
                    IsError = true,
                });
            }
            else
            {
                var assignments = profile.Devices.Where(d => d.AssignedUser != null).ToList();

                if (assignments.Count == 0)
                {
                    results.Add(new BulkGenerateUserModel()
                    {
                        Id = profile.Name,
                        DisplayName = profile.Name,
                        Scope = $"Device Profile has no devices with active assignments",
                        IsError = true,
                    });
                }
                else
                {
                    foreach (var assignment in assignments)
                    {
                        results.Add(new BulkGenerateUserModel()
                        {
                            Id = assignment.AssignedUserId,
                            DisplayName = assignment.AssignedUser.DisplayName,
                            UserEmailAddress = assignment.AssignedUser.EmailAddress,
                            Scope = $"Device Profile '{profile.Name}' Matches Assigned Device '{assignment.SerialNumber}'",
                            IsError = false,
                        });
                    }
                }
            }

            return Json(results);
        }

        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.BulkGenerate, Claims.User.Actions.GenerateDocuments)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult BulkGenerateAddDeviceBatch(int deviceBatchId)
        {
            if (deviceBatchId <= 0)
                return BadRequest();

            var results = new List<BulkGenerateUserModel>();

            var batch = Database.DeviceBatches.Include(p => p.Devices.Select(a => a.AssignedUser)).FirstOrDefault(f => f.Id == deviceBatchId);

            if (batch == null)
            {
                results.Add(new BulkGenerateUserModel()
                {
                    Id = deviceBatchId.ToString(),
                    DisplayName = deviceBatchId.ToString(),
                    Scope = $"Unknown Device Batch '{deviceBatchId}'",
                    IsError = true,
                });
            }
            else
            {
                var assignments = batch.Devices.Where(d => d.AssignedUser != null).ToList();

                if (assignments.Count == 0)
                {
                    results.Add(new BulkGenerateUserModel()
                    {
                        Id = batch.Name,
                        DisplayName = batch.Name,
                        Scope = $"Device Batch has no devices with active assignments",
                        IsError = true,
                    });
                }
                else
                {
                    foreach (var assignment in assignments)
                    {
                        results.Add(new BulkGenerateUserModel()
                        {
                            Id = assignment.AssignedUserId,
                            DisplayName = assignment.AssignedUser.DisplayName,
                            UserEmailAddress = assignment.AssignedUser.EmailAddress,
                            Scope = $"Device Batch '{batch.Name}' Matches Assigned Device '{assignment.SerialNumber}'",
                            IsError = false,
                        });
                    }
                }
            }

            return Json(results);
        }

        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.BulkGenerate, Claims.User.Actions.GenerateDocuments)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult BulkGenerateAddDocumentAttachment(string documentTemplateId, DateTime? threshold)
        {
            if (string.IsNullOrWhiteSpace(documentTemplateId))
                return BadRequest();

            var results = new List<BulkGenerateUserModel>();

            var template = Database.DocumentTemplates.FirstOrDefault(f => f.Id == documentTemplateId);

            if (template == null)
            {
                results.Add(new BulkGenerateUserModel()
                {
                    Id = documentTemplateId,
                    DisplayName = documentTemplateId,
                    Scope = $"Unknown Document Template '{documentTemplateId}'",
                    IsError = true,
                });
            }
            else
            {
                switch (template.AttachmentType)
                {
                    case AttachmentTypes.Device:
                        var deviceAssignments = Database.DeviceAttachments
                            .Include(a => a.Device.AssignedUser)
                            .Where(a => a.DocumentTemplateId == template.Id && (threshold == null || a.Timestamp > threshold) && a.Device.AssignedUserId != null)
                            .ToList();
                        foreach (var assignment in deviceAssignments)
                        {
                            results.Add(new BulkGenerateUserModel()
                            {
                                Id = assignment.Device.AssignedUserId,
                                DisplayName = assignment.Device.AssignedUser.DisplayName,
                                UserEmailAddress = assignment.Device.AssignedUser.EmailAddress,
                                Scope = $"Document Template '{template.Id}' Attachment Matches Assigned Device '{assignment.Device.SerialNumber}'",
                                IsError = false,
                            });
                        }
                        break;
                    case AttachmentTypes.Job:
                        var jobAssignments = Database.JobAttachments
                            .Include(a => a.Job.User)
                            .Where(a => a.DocumentTemplateId == template.Id && (threshold == null || a.Timestamp > threshold) && a.Job.UserId != null)
                            .ToList();
                        foreach (var assignment in jobAssignments)
                        {
                            results.Add(new BulkGenerateUserModel()
                            {
                                Id = assignment.Job.UserId,
                                DisplayName = assignment.Job.User.DisplayName,
                                UserEmailAddress = assignment.Job.User.EmailAddress,
                                Scope = $"Document Template '{template.Id}' Attachment Matches Job '{assignment.Job.Id}'",
                                IsError = false,
                            });
                        }
                        break;
                    case AttachmentTypes.User:
                        var userAssignments = Database.UserAttachments
                            .Include(a => a.User)
                            .Where(a => a.DocumentTemplateId == template.Id && (threshold == null || a.Timestamp > threshold) && a.UserId != null)
                            .ToList();
                        foreach (var assignment in userAssignments)
                        {
                            results.Add(new BulkGenerateUserModel()
                            {
                                Id = assignment.UserId,
                                DisplayName = assignment.User.DisplayName,
                                UserEmailAddress = assignment.User.EmailAddress,
                                Scope = $"Document Template '{template.Id}' Attachment Matches User",
                                IsError = false,
                            });
                        }
                        break;
                    default:
                        throw new NotSupportedException();
                }

                if (results.Count == 0)
                {
                    if (threshold.HasValue)
                    {
                        results.Add(new BulkGenerateUserModel()
                        {
                            Id = template.Id,
                            DisplayName = template.Id,
                            Scope = $"Document Template has no attachments with associated users after {threshold:yyyy-MM-dd}",
                            IsError = true,
                        });
                    }
                    else
                    {
                        results.Add(new BulkGenerateUserModel()
                        {
                            Id = template.Id,
                            DisplayName = template.Id,
                            Scope = $"Document Template has no attachments with associated users",
                            IsError = true,
                        });
                    }
                }
                else
                {
                    var distinctSet = new HashSet<string>(StringComparer.Ordinal);
                    results = results.Where(r => distinctSet.Add(r.Id)).ToList();
                }
            }

            return Json(results);
        }

        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.BulkGenerate, Claims.User.Actions.GenerateDocuments, Claims.User.ShowDetails)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult BulkGenerateGetUserDetailValues(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return BadRequest();

            var results = Database.UserDetails.Where(d => d.Scope == "Details" && d.Key == key).Select(d => d.Value).Distinct().ToList();

            return Json(results);
        }

        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.BulkGenerate, Claims.User.Actions.GenerateDocuments, Claims.User.ShowDetails)]
        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public virtual ActionResult BulkGenerateAddUserDetail(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                return BadRequest();

            var results = new List<BulkGenerateUserModel>();

            var query = Database.UserDetails.Include(d => d.User).Where(d => d.Scope == "Details" && d.Key == key);
            if (!string.IsNullOrWhiteSpace(value))
            {
                query = query.Where(d => d.Value == value);
            }
            var details = query.ToList();

            if (details.Count == 0)
            {
                results.Add(new BulkGenerateUserModel()
                {
                    Id = key,
                    DisplayName = $"{key}{(string.IsNullOrWhiteSpace(value) ? null : $":{value}")}",
                    Scope = $"User Detail '{key}' didn't match any users{(string.IsNullOrWhiteSpace(value) ? null : $" with the value '{value}'")}",
                    IsError = true,
                });
            }
            else
            {
                foreach (var user in details.Select(d => d.User).Distinct())
                {
                    results.Add(new BulkGenerateUserModel()
                    {
                        Id = user.UserId,
                        DisplayName = user.DisplayName,
                        Scope = $"User Detail '{key}'{(string.IsNullOrWhiteSpace(value) ? null : $" with the value '{value}'")} Matches User",
                        IsError = false,
                    });
                }
            }

            return Json(results);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Generate(string id, string targetId)
        {
            Disco.Services.DocumentTemplateExtensions.GetTemplateAndTarget(Database, Authorization, id, targetId, out var template, out var target, out _);

            // generate document
            var timestamp = DateTime.Now;
            var document = default(Stream);
            using (var state = DocumentState.DefaultState())
            {
                document = template.GeneratePdf(Database, target, UserService.CurrentUser, timestamp, state);
            }
            Database.SaveChanges();

            return File(document, "application/pdf", $"{template.Id}_{target.AttachmentReferenceId.Replace('\\', '_')}_{timestamp:yyyyMMdd-HHmmss}.pdf");
        }

        [DiscoAuthorize(Claims.Config.DocumentTemplate.Delete)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Delete(string id, bool? redirect = false)
        {
            try
            {
                var at = Database.DocumentTemplates.Include("JobSubTypes").FirstOrDefault(a => a.Id == id);
                if (at != null)
                {
                    at.Delete(Database);
                    Database.SaveChanges();
                    if (redirect.HasValue && redirect.Value)
                        return RedirectToAction(MVC.Config.DocumentTemplate.Index(null));
                    else
                        return Ok();
                }
                throw new Exception("Invalid Document Template Id");
            }
            catch (Exception ex)
            {
                if (redirect.HasValue && redirect.Value)
                    throw;
                else
                    return BadRequest(ex.Message);
            }
        }

        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.Configure, Claims.Config.UserFlag.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult RemoveOnImportUserFlagRule([Required] string id, Guid? ruleId = null)
        {
            try
            {
                var template = Database.DocumentTemplates.FirstOrDefault(t => t.Id == id);

                if (template == null)
                    throw new ArgumentException("Unknown document template", nameof(id));

                template.RemoveOnImportUserFlagRule(Database, ruleId.Value);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.Configure, Claims.Config.UserFlag.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult AddOnImportUserFlagRule([Required] string id, bool? addFlag = null, int? userFlagId = null, string comments = null)
        {
            try
            {
                var template = Database.DocumentTemplates.FirstOrDefault(t => t.Id == id);

                if (template == null)
                    throw new ArgumentException("Unknown document template", nameof(id));

                var rule = new OnImportUserFlagRule()
                {
                    AddFlag = addFlag.Value,
                    FlagId = userFlagId.Value,
                    UserId = Authorization.User.UserId,
                    Comments = comments,
                };

                rule = template.AddOnImportUserFlagRule(Database, rule);

                var model = new AddOnImportUserFlagRuleModel()
                {
                    Id = rule.Id,
                    FlagId = rule.FlagId,
                    UserId = rule.UserId,
                    AddFlag = rule.AddFlag,
                    Comments = rule.Comments,
                    UserFlagName = rule.UserFlag.Name,
                    UserFlagIcon = rule.UserFlag.Icon,
                    UserFlagColour = rule.UserFlag.IconColour,
                };

                return Json(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region Handlers
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult GenerateDocumentHandlerUi(string templateId, string targetId, string handlerId)
        {
            Disco.Services.DocumentTemplateExtensions.GetTemplateAndTarget(Database, Authorization, templateId, targetId, out var template, out var target, out var targetUser);

            var handlerManifest = Plugins.GetPluginFeature(handlerId, typeof(DocumentHandlerProviderFeature));

            using (var handler = handlerManifest.CreateInstance<DocumentHandlerProviderFeature>())
            {
                if (!handler.CanHandle(template, target))
                    throw new NotSupportedException("Handler does not support this Document Template and Target");

                var handlerPartialView = handler.GenerationOptionsUi;

                if (handlerPartialView == null)
                    throw new NotSupportedException("Handler does not have a Generation Options UI");

                var model = handler.GetGenerationOptionsUiModel(template, target, targetUser, CurrentUser);

                return this.PrecompiledPartialView(handlerPartialView, model);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult DocumentHandlers(string templateId, string targetId)
        {
            Disco.Services.DocumentTemplateExtensions.GetTemplateAndTarget(Database, Authorization, templateId, targetId, out var template, out var target, out _);

            var handlers = Plugins.GetPluginFeatures(typeof(DocumentHandlerProviderFeature))
                .SelectMany(f =>
                {
                    using (var handler = f.CreateInstance<DocumentHandlerProviderFeature>())
                    {
                        if (handler.CanHandle(template, target))
                            return OneOf.Create(new DocumentHandlerModel()
                            {
                                Id = f.Id,
                                Title = handler.HandlerTitle,
                                Description = handler.HandlerDescription,
                                UiUrl = handler.GenerationOptionsUi == null ? null : Url.Action(MVC.API.DocumentTemplate.GenerateDocumentHandlerUi(template.Id, target.AttachmentReferenceId, f.Id)),
                                Icon = handler.GenerationOptionsIcon,
                            });
                    }
                    return Enumerable.Empty<DocumentHandlerModel>();
                }).ToList();

            var model = new DocumentHandlersModel()
            {
                TemplateId = template.Id,
                TemplateName = template.Description,
                TargetId = target.AttachmentReferenceId,
                TargetName = target.ToString(),
                Handlers = handlers,
            };

            return Json(model);
        }
        #endregion

        #region Exporting

        [DiscoAuthorize(Claims.Config.DocumentTemplate.Export)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Export(ExportModel model)
        {
            if (model == null || model.Options == null)
                throw new ArgumentNullException(nameof(model));

            var templateId = default(string);
            if (model.Options.DocumentTemplateIds.Count == 1)
                templateId = model.Options.DocumentTemplateIds.First();

            Database.DiscoConfiguration.Documents.LastExportOptions = model.Options;
            Database.SaveChanges();

            // Start Export
            var exportContext = new DocumentExport(model.Options);
            var taskContext = ExportTask.ScheduleNowCacheResult(exportContext, id => Url.Action(MVC.Config.DocumentTemplate.Export(null, id)));

            // Try waiting for completion
            if (taskContext.TaskStatus.WaitUntilFinished(TimeSpan.FromSeconds(2)))
                return RedirectToAction(MVC.Config.DocumentTemplate.Export(null, taskContext.Id));
            else
                return RedirectToAction(MVC.Config.Logging.TaskStatus(taskContext.TaskStatus.SessionId));
        }

        [DiscoAuthorize(Claims.Config.DocumentTemplate.Export)]
        public virtual ActionResult ExportRetrieve(Guid id)
        {
            if (!ExportTask.TryFromCache(id, out var context))
                throw new ArgumentException("The export id specified is invalid, or the export data expired (60 minutes)", nameof(id));

            if (context.Result == null || context.Result.Result == null)
                throw new ArgumentException("The export session is still running, or failed to complete successfully", nameof(id));

            if (context.Result.RecordCount == 0)
                throw new ArgumentException("No records were found to export", nameof(id));

            var fileStream = context.Result.Result;

            return this.File(fileStream.GetBuffer(), 0, (int)fileStream.Length, context.Result.MimeType, context.Result.Filename);
        }

        [DiscoAuthorizeAll(Claims.Config.ManageSavedExports, Claims.Config.DocumentTemplate.Export)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult SaveExport(ExportModel model)
        {
            Database.DiscoConfiguration.Documents.LastExportOptions = model.Options;

            var export = new DocumentExport(model.Options);
            var savedExport = SavedExports.SaveExport(export, Database, CurrentUser);

            return RedirectToAction(MVC.Config.Export.Create(savedExport.Id));
        }

        #endregion
    }
}
