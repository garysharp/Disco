using Disco.BI;
using Disco.BI.Extensions;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Users;
using Disco.Services.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class DocumentTemplateController : AuthorizedDatabaseController
    {

        const string pDescription = "description";
        const string pScope = "scope";
        const string pFilterExpression = "filterexpression";
        const string pFlattenForm = "flattenform";

        [DiscoAuthorize(Claims.Config.DocumentTemplate.Configure)]
        public virtual ActionResult Update(string id, string key, string value = null, bool redirect = false)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new ArgumentNullException("id");
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");
                var documentTemplate = Database.DocumentTemplates.Find(id);
                if (documentTemplate != null)
                {
                    switch (key.ToLower())
                    {
                        case pDescription:
                            UpdateDescription(documentTemplate, value);
                            break;
                        case pScope:
                            UpdateScope(documentTemplate, value);
                            break;
                        case pFilterExpression:
                            Authorization.Require(Claims.Config.DocumentTemplate.ConfigureFilterExpression);
                            UpdateFilterExpression(documentTemplate, value);
                            break;
                        case pFlattenForm:
                            UpdateFlattenForm(documentTemplate, value);
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
                    return RedirectToAction(MVC.Config.DocumentTemplate.Index(documentTemplate.Id));
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

        [DiscoAuthorize(Claims.Config.DocumentTemplate.Upload), HttpGet]
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
                return File(filename, DocumentTemplate.PdfMimeType, string.Format("{0}.pdf", documentTemplate.Id));
            }
            else
            {
                throw new InvalidOperationException("Template not found");
            }
        }

        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.Upload, Claims.Config.DocumentTemplate.Configure), HttpPost]
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
        [DiscoAuthorize(Claims.Config.DocumentTemplate.Configure)]
        public virtual ActionResult UpdateDescription(string id, string Description = null, bool redirect = false)
        {
            return Update(id, pDescription, Description, redirect);
        }
        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.Configure, Claims.Config.DocumentTemplate.ConfigureFilterExpression)]
        public virtual ActionResult UpdateFilterExpression(string id, string FilterExpression = null, bool redirect = false)
        {
            return Update(id, pFilterExpression, FilterExpression, redirect);
        }
        [DiscoAuthorize(Claims.Config.DocumentTemplate.Configure)]
        public virtual ActionResult UpdateFlattenForm(string id, string FlattenForm = null, bool redirect = false)
        {
            return Update(id, pFlattenForm, FlattenForm, redirect);
        }
        [DiscoAuthorize(Claims.Config.DocumentTemplate.Configure)]
        public virtual ActionResult UpdateScope(string id, string Scope = null, bool redirect = false)
        {
            return Update(id, pScope, Scope, redirect);
        }
        [DiscoAuthorize(Claims.Config.DocumentTemplate.Configure)]
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
        #endregion

        #region Update Properties
        private void UpdateDescription(Disco.Models.Repository.DocumentTemplate documentTemplate, string Description)
        {
            if (!string.IsNullOrWhiteSpace(Description))
            {
                documentTemplate.Description = Description.Trim();
                Database.SaveChanges();
                return;
            }
            throw new Exception("Invalid Description");
        }
        private void UpdateScope(Disco.Models.Repository.DocumentTemplate documentTemplate, string Scope)
        {
            if (!string.IsNullOrWhiteSpace(Scope))
            {
                if (Disco.Models.Repository.DocumentTemplate.DocumentTemplateScopes.ToList().Contains(Scope))
                {
                    Database.Configuration.LazyLoadingEnabled = true;

                    documentTemplate.Scope = Scope;

                    if (documentTemplate.Scope != Disco.Models.Repository.DocumentTemplate.DocumentTemplateScopes.Job &&
                        documentTemplate.JobSubTypes != null)
                    {
                        foreach (var st in documentTemplate.JobSubTypes.ToArray())
                            documentTemplate.JobSubTypes.Remove(st);
                    }

                    Database.SaveChanges();
                    return;
                }
            }
            throw new Exception("Invalid Scope");
        }
        private void UpdateFilterExpression(Disco.Models.Repository.DocumentTemplate documentTemplate, string FilterExpression)
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
        private void UpdateFlattenForm(Disco.Models.Repository.DocumentTemplate documentTemplate, string FlattenForm)
        {
            if (string.IsNullOrWhiteSpace(FlattenForm))
            {
                documentTemplate.FlattenForm = false;
            }
            else
            {
                bool ff = default(bool);
                if (bool.TryParse(FlattenForm, out ff))
                    documentTemplate.FlattenForm = ff;
                else
                    throw new Exception("Invalid Boolean Format");
            }

            Database.SaveChanges();
        }
        private void UpdateJobSubTypes(Disco.Models.Repository.DocumentTemplate documentTemplate, List<string> JobSubTypes)
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
                var subTypes = new List<Disco.Models.Repository.JobSubType>();
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
        #endregion

        #region Actions

        [DiscoAuthorize(Claims.Config.DocumentTemplate.UndetectedPages), OutputCache(NoStore = true, Duration = 0)]
        public virtual ActionResult ImporterThumbnail(string SessionId, int PageNumber)
        {
            var dataStoreSessionPagesCacheLocation = DataStore.CreateLocation(Database, "Cache\\DocumentDropBox_SessionPages");
            var filename = System.IO.Path.Combine(dataStoreSessionPagesCacheLocation, string.Format("{0}-{1}", SessionId, PageNumber));
            if (System.IO.File.Exists(filename))
                return File(filename, "image/png");
            else
                return File("~/ClientSource/Style/Images/Status/fileBroken256.png", "image/png");
        }

        [DiscoAuthorize(Claims.Config.DocumentTemplate.UndetectedPages)]
        public virtual ActionResult ImporterUndetectedFiles()
        {
            var undetectedLocation = DataStore.CreateLocation(Database, "DocumentDropBox_Unassigned");
            var undetectedDirectory = new System.IO.DirectoryInfo(undetectedLocation);
            var m = undetectedDirectory.GetFiles("*.pdf").Select(f => new Models.DocumentTemplate.ImporterUndetectedFilesModel()
            {
                Id = System.IO.Path.GetFileNameWithoutExtension(f.Name),
                Timestamp = f.CreationTime.ToFullDateTime(),
                TimestampUnixEpoc = f.CreationTime.ToUnixEpoc()
            }).ToArray();

            return Json(m);
        }

        [DiscoAuthorize(Claims.Config.DocumentTemplate.UndetectedPages)]
        public virtual ActionResult ImporterUndetectedDataIdLookup(string id, string term, int limitCount = 20)
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
                    Models.DocumentTemplate.ImporterUndetectedDataIdLookupModel[] results;
                    switch (searchScope)
                    {
                        case DocumentTemplate.DocumentTemplateScopes.Device:
                            results = Disco.Services.Searching.Search.SearchDevices(Database, term, limitCount).Select(sr => Models.DocumentTemplate.ImporterUndetectedDataIdLookupModel.FromSearchResultItem(sr)).ToArray();
                            break;
                        case DocumentTemplate.DocumentTemplateScopes.Job:
                            results = Disco.Services.Searching.Search.SearchJobsTable(Database, term, limitCount, false).Items.Select(sr => Models.DocumentTemplate.ImporterUndetectedDataIdLookupModel.FromSearchResultItem(sr)).ToArray();
                            break;
                        case DocumentTemplate.DocumentTemplateScopes.User:
                            results = Disco.Services.Searching.Search.SearchUsers(Database, term, limitCount).Select(sr => Models.DocumentTemplate.ImporterUndetectedDataIdLookupModel.FromSearchResultItem(sr)).ToArray();
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
        public virtual ActionResult ImporterUndetectedFile(string id, Nullable<bool> Source, Nullable<bool> Thumbnail)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var undetectedLocation = DataStore.CreateLocation(Database, "DocumentDropBox_Unassigned");
                if (Source.HasValue && Source.Value)
                {
                    var filename = System.IO.Path.Combine(undetectedLocation, string.Concat(id, ".pdf"));
                    if (System.IO.File.Exists(filename))
                        return File(filename, DocumentTemplate.PdfMimeType);
                    else
                        return HttpNotFound();
                }
                else
                {
                    if (Thumbnail.HasValue && Thumbnail.Value)
                    {
                        var filename = System.IO.Path.Combine(undetectedLocation, string.Concat(id, "_thumbnail.png"));
                        if (System.IO.File.Exists(filename))
                            return File(filename, "image/png");
                        else
                            return File(Links.ClientSource.Style.Images.Status.fileBroken256_png, "image/png");
                    }
                    else
                    {
                        var filename = System.IO.Path.Combine(undetectedLocation, string.Concat(id, ".jpg"));
                        if (System.IO.File.Exists(filename))
                            return File(filename, "image/jpeg");
                        else
                            return File(Links.ClientSource.Style.Images.Status.fileBroken256_png, "image/png");
                    }
                }
            }
            return HttpNotFound();
        }

        [DiscoAuthorize(Claims.Config.DocumentTemplate.UndetectedPages)]
        public virtual ActionResult ImporterUndetectedAssign(string id, string DocumentTemplateId, string DataId)
        {
            var undetectedLocation = DataStore.CreateLocation(Database, "DocumentDropBox_Unassigned");
            var filename = System.IO.Path.Combine(undetectedLocation, string.Concat(id, ".pdf"));
            if (BI.Interop.Pdf.PdfImporter.ProcessPdfAttachment(filename, Database, DocumentTemplateId, DataId, UserService.CurrentUserId, DateTime.Now))
            {
                // Delete File
                System.IO.File.Delete(filename);

                // Delete Thumbnail/Preview
                var thumbnailFilename = System.IO.Path.Combine(undetectedLocation, string.Concat(id, "_thumbnail.png"));
                if (System.IO.File.Exists(thumbnailFilename))
                    System.IO.File.Delete(thumbnailFilename);
                var previewFilename = System.IO.Path.Combine(undetectedLocation, string.Concat(id, ".jpg"));
                if (System.IO.File.Exists(previewFilename))
                    System.IO.File.Delete(previewFilename);

                return Json("OK");
            }
            else
            {
                return Json("Unable to Import File with the supplied parameters");
            }
        }

        [DiscoAuthorize(Claims.Config.DocumentTemplate.UndetectedPages)]
        public virtual ActionResult ImporterUndetectedDelete(string id)
        {
            var undetectedLocation = DataStore.CreateLocation(Database, "DocumentDropBox_Unassigned");
            var filename = System.IO.Path.Combine(undetectedLocation, string.Concat(id, ".pdf"));
            if (System.IO.File.Exists(filename))
            {
                // Delete File
                System.IO.File.Delete(filename);

                // Delete Thumbnail/Preview
                var thumbnailFilename = System.IO.Path.Combine(undetectedLocation, string.Concat(id, "_thumbnail.png"));
                if (System.IO.File.Exists(thumbnailFilename))
                    System.IO.File.Delete(thumbnailFilename);
                var previewFilename = System.IO.Path.Combine(undetectedLocation, string.Concat(id, ".jpg"));
                if (System.IO.File.Exists(previewFilename))
                    System.IO.File.Delete(previewFilename);

                return Json("OK");
            }
            else
            {
                return Json("File Not Found");
            }
        }

        [DiscoAuthorize(Claims.Config.DocumentTemplate.BulkGenerate)]
        public virtual ActionResult BulkGenerate(string id, string DataIds = null)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");
            if (string.IsNullOrEmpty(DataIds))
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

            var dataIds = DataIds.Split(new string[] { Environment.NewLine, ",", ";" }, StringSplitOptions.RemoveEmptyEntries).Select(d => d.Trim()).Where(d => !string.IsNullOrEmpty(d)).ToArray();
            var timeStamp = DateTime.Now;
            var pdf = documentTemplate.GeneratePdfBulk(Database, UserService.CurrentUser, timeStamp, dataIds);

            return File(pdf, "application/pdf", string.Format("{0}_Bulk_{1:yyyyMMdd-HHmmss}.pdf", documentTemplate.Id, timeStamp));
        }

        [DiscoAuthorize(Claims.Config.DocumentTemplate.Delete)]
        public virtual ActionResult Delete(string id, Nullable<bool> redirect = false)
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
                        return Json("OK", JsonRequestBehavior.AllowGet);
                }
                throw new Exception("Invalid Document Template Id");
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

    }
}
