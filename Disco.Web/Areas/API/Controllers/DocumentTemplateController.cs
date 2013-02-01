using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.BI;
using Disco.BI.Extensions;
using Disco.Models.Repository;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class DocumentTemplateController : dbAdminController
    {

        const string pDescription = "description";
        const string pScope = "scope";
        const string pFilterExpression = "filterexpression";
        const string pFlattenForm = "flattenform";

        public virtual ActionResult Update(string id, string key, string value = null, bool redirect = false)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new ArgumentNullException("id");
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");
                var documentTemplate = dbContext.DocumentTemplates.Find(id);
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

        [HttpGet]
        public virtual ActionResult Template(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");
            var documentTemplate = dbContext.DocumentTemplates.Find(id);
            if (documentTemplate == null)
                throw new ArgumentException("Invalid Document Template Id", "id");

            var filename = documentTemplate.RepositoryFilename(dbContext);
            if (System.IO.File.Exists(filename))
            {
                return File(filename, DocumentTemplate.PdfMimeType, string.Format("{0}.pdf", documentTemplate.Id));
            }
            else
            {
                throw new InvalidOperationException("Template not found");
            }
        }
        [HttpPost]
        public virtual ActionResult Template(string id, bool redirect, HttpPostedFileBase Template)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new ArgumentNullException("id");
                var documentTemplate = dbContext.DocumentTemplates.Find(id);
                if (documentTemplate == null)
                    throw new ArgumentException("Invalid Document Template Id", "id");

                documentTemplate.SavePdfTemplate(dbContext, Template.InputStream);

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
        public virtual ActionResult UpdateDescription(string id, string Description = null, bool redirect = false)
        {
            return Update(id, pDescription, Description, redirect);
        }
        public virtual ActionResult UpdateFilterExpression(string id, string FilterExpression = null, bool redirect = false)
        {
            return Update(id, pFilterExpression, FilterExpression, redirect);
        }
        public virtual ActionResult UpdateFlattenForm(string id, string FlattenForm = null, bool redirect = false)
        {
            return Update(id, pFlattenForm, FlattenForm, redirect);
        }
        public virtual ActionResult UpdateScope(string id, string Scope = null, bool redirect = false)
        {
            return Update(id, pScope, Scope, redirect);
        }
        public virtual ActionResult UpdateSubTypes(string id, List<string> SubTypes = null)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new ArgumentNullException("id");
                var documentTemplate = dbContext.DocumentTemplates.Find(id);

                UpdateSubTypes(documentTemplate, SubTypes);

                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
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
                dbContext.SaveChanges();
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
                    dbContext.Configuration.LazyLoadingEnabled = true;

                    documentTemplate.Scope = Scope;

                    if (documentTemplate.Scope != Disco.Models.Repository.DocumentTemplate.DocumentTemplateScopes.Job &&
                        documentTemplate.JobSubTypes != null)
                    {
                        foreach (var st in documentTemplate.JobSubTypes.ToArray())
                            documentTemplate.JobSubTypes.Remove(st);
                    }

                    dbContext.SaveChanges();
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

            dbContext.SaveChanges();
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

            dbContext.SaveChanges();
        }
        private void UpdateSubTypes(Disco.Models.Repository.DocumentTemplate documentTemplate, List<string> SubTypes)
        {
            dbContext.Configuration.LazyLoadingEnabled = true;

            // Remove All Existing
            if (documentTemplate.JobSubTypes != null)
            {
                foreach (var st in documentTemplate.JobSubTypes.ToArray())
                    documentTemplate.JobSubTypes.Remove(st);
            }

            // Add New
            if (SubTypes != null && SubTypes.Count > 0)
            {
                var subTypes = new List<Disco.Models.Repository.JobSubType>();
                foreach (var stId in SubTypes)
                {
                    var typeId = stId.Substring(0, stId.IndexOf("_"));
                    var subTypeId = stId.Substring(stId.IndexOf("_") + 1);
                    var subType = dbContext.JobSubTypes.FirstOrDefault(jst => jst.JobTypeId == typeId && jst.Id == subTypeId);
                    subTypes.Add(subType);
                }
                documentTemplate.JobSubTypes = subTypes;
            }
            dbContext.SaveChanges();
        }
        #endregion



        #region Actions

        [OutputCache(NoStore = true, Duration = 0)]
        public virtual ActionResult ImporterThumbnail(string SessionId, int PageNumber)
        {
            // Load from Cache
            //var cacheKey = string.Format("Disco.BI.DocumentImporter-{0}-{1}", SessionId, PageNumber);
            //var cacheValue = HttpContext.Cache.Get(cacheKey);
            //if (cacheValue != null)
            //{
            //    var cacheFile = cacheValue as byte[];
            //    if (cacheFile != null)
            //    {
            //        return File(cacheFile, "image/png");
            //    }
            //}

            var dataStoreSessionPagesCacheLocation = DataStore.CreateLocation(dbContext, "Cache\\DocumentDropBox_SessionPages");
            var filename = System.IO.Path.Combine(dataStoreSessionPagesCacheLocation, string.Format("{0}-{1}", SessionId, PageNumber));
            if (System.IO.File.Exists(filename))
                return File(filename, "image/png");
            else
                return File("~/Content/Images/Status/fileBroken256.png", "image/png");
        }

        public virtual ActionResult ImporterUndetectedFiles()
        {
            var undetectedLocation = DataStore.CreateLocation(dbContext, "DocumentDropBox_Unassigned");
            var undetectedDirectory = new System.IO.DirectoryInfo(undetectedLocation);
            var m = undetectedDirectory.GetFiles("*.pdf").Select(f => new Models.DocumentTemplate.ImporterUndetectedFilesModel()
            {
                Id = System.IO.Path.GetFileNameWithoutExtension(f.Name),
                Timestamp = f.CreationTime.ToString(),
                TimestampFuzzy = f.CreationTime.ToFuzzy()
            }).ToArray();

            return Json(m);
        }
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
                    var documentTemplate = dbContext.DocumentTemplates.Find(id);
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
                            results = BI.DeviceBI.Searching.Search(dbContext, term, limitCount).Select(sr => Models.DocumentTemplate.ImporterUndetectedDataIdLookupModel.FromSearchResultItem(sr)).ToArray();
                            break;
                        case DocumentTemplate.DocumentTemplateScopes.Job:
                            results = BI.JobBI.Searching.Search(dbContext, term, limitCount, false).Items.Select(sr => Models.DocumentTemplate.ImporterUndetectedDataIdLookupModel.FromSearchResultItem(sr)).ToArray();
                            break;
                        case DocumentTemplate.DocumentTemplateScopes.User:
                            results = BI.UserBI.Searching.Search(dbContext, term, limitCount).Select(sr => Models.DocumentTemplate.ImporterUndetectedDataIdLookupModel.FromSearchResultItem(sr)).ToArray();
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
        public virtual ActionResult ImporterUndetectedFile(string id, Nullable<bool> Source, Nullable<bool> Thumbnail)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var undetectedLocation = DataStore.CreateLocation(dbContext, "DocumentDropBox_Unassigned");
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
        public virtual ActionResult ImporterUndetectedAssign(string id, string DocumentTemplateId, string DataId)
        {
            var undetectedLocation = DataStore.CreateLocation(dbContext, "DocumentDropBox_Unassigned");
            var filename = System.IO.Path.Combine(undetectedLocation, string.Concat(id, ".pdf"));
            if (BI.Interop.Pdf.PdfImporter.ProcessPdfAttachment(filename, dbContext, DocumentTemplateId, DataId, DiscoApplication.CurrentUser.Id, DateTime.Now))
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
        public virtual ActionResult ImporterUndetectedDelete(string id)
        {
            var undetectedLocation = DataStore.CreateLocation(dbContext, "DocumentDropBox_Unassigned");
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

        public virtual ActionResult BulkGenerate(string id, string DataIds = null)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");
            if (string.IsNullOrEmpty(DataIds))
                throw new ArgumentNullException("DataIds");
            var documentTemplate = dbContext.DocumentTemplates.Find(id);
            if (documentTemplate == null)
                throw new ArgumentException("Invalid Document Template Id", "id");

            var dataIds = DataIds.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var timeStamp = DateTime.Now;
            var pdf = documentTemplate.GeneratePdfBulk(dbContext, DiscoApplication.CurrentUser, timeStamp, dataIds);

            return File(pdf, "application/pdf", string.Format("{0}_Bulk_{1:yyyyMMdd-HHmmss}.pdf", documentTemplate.Id, timeStamp));
        }

        public virtual ActionResult Delete(string id, Nullable<bool> redirect = false)
        {
            try
            {
                var at = dbContext.DocumentTemplates.Include("JobSubTypes").FirstOrDefault(a => a.Id == id);
                if (at != null)
                {
                    at.Delete(dbContext);
                    dbContext.SaveChanges();
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
