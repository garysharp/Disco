using Disco.BI.Extensions;
using Disco.Models.Repository;
using Disco.Models.Services.Documents;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Documents;
using Disco.Services.Users;
using Disco.Services.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class DocumentTemplatePackageController : AuthorizedDatabaseController
    {
        const string pDescription = "description";
        const string pScope = "scope";
        const string pFilterExpression = "filterexpression";
        const string pOnGenerateExpression = "ongenerateexpression";
        const string pIsHidden = "ishidden";
        const string pInsertBlankPages = "insertblankpages";

        [DiscoAuthorize(Claims.Config.DocumentTemplate.Configure)]
        public virtual ActionResult Update(string id, string key, string value = null, bool redirect = false)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new ArgumentNullException("id");
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");

                var package = DocumentTemplatePackages.GetPackage(id);

                if (package != null)
                {
                    switch (key.ToLower())
                    {
                        case pDescription:
                            UpdateDescription(package, value);
                            break;
                        case pScope:
                            UpdateScope(package, value);
                            break;
                        case pFilterExpression:
                            Authorization.Require(Claims.Config.DocumentTemplate.ConfigureFilterExpression);
                            UpdateFilterExpression(package, value);
                            break;
                        case pOnGenerateExpression:
                            UpdateOnGenerateExpression(package, value);
                            break;
                        case pIsHidden:
                            UpdateIsHidden(package, value);
                            break;
                        case pInsertBlankPages:
                            UpdateInsertBlankPages(package, value);
                            break;
                        default:
                            throw new Exception("Invalid Update Key");
                    }
                }
                else
                {
                    throw new Exception("Invalid Document Template Package Id");
                }
                if (redirect)
                    return RedirectToAction(MVC.Config.DocumentTemplate.ShowPackage(package.Id));
                else
                    return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (redirect)
                    throw;
                else
                    return Json($"Error: {ex.Message}", JsonRequestBehavior.AllowGet);
            }
        }

        #region Update Shortcut Methods
        [DiscoAuthorize(Claims.Config.DocumentTemplate.Configure)]
        public virtual ActionResult UpdateDescription(string id, string Description = null, bool redirect = false)
        {
            return Update(id, pDescription, Description, redirect);
        }
        [DiscoAuthorize(Claims.Config.DocumentTemplate.Configure)]
        public virtual ActionResult UpdateDocumentTemplates(string id, List<string> DocumentTemplates = null, bool redirect = false)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new ArgumentNullException("id");

                var package = DocumentTemplatePackages.GetPackage(id);

                if (package == null)
                    throw new ArgumentException("Invalid Document Template Package Id", nameof(id));

                UpdateDocumentTemplates(package, DocumentTemplates);

                if (redirect)
                    return RedirectToAction(MVC.Config.DocumentTemplate.ShowPackage(package.Id));
                else
                    return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (redirect)
                    throw;
                else
                    return Json($"Error: {ex.Message}", JsonRequestBehavior.AllowGet);
            }

        }
        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.Configure, Claims.Config.DocumentTemplate.ConfigureFilterExpression)]
        public virtual ActionResult UpdateFilterExpression(string id, string FilterExpression = null, bool redirect = false)
        {
            return Update(id, pFilterExpression, FilterExpression, redirect);
        }
        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.Configure, Claims.Config.DocumentTemplate.ConfigureFilterExpression)]
        public virtual ActionResult UpdateOnGenerateExpression(string id, string OnGenerateExpression = null, bool redirect = false)
        {
            return Update(id, pOnGenerateExpression, OnGenerateExpression, redirect);
        }
        [DiscoAuthorize(Claims.Config.DocumentTemplate.Configure)]
        public virtual ActionResult UpdateIsHidden(string id, string IsHidden = null, bool redirect = false)
        {
            return Update(id, pIsHidden, IsHidden, redirect);
        }
        [DiscoAuthorize(Claims.Config.DocumentTemplate.Configure)]
        public virtual ActionResult UpdateInsertBlankPages(string id, string InsertBlankPages = null, bool redirect = false)
        {
            return Update(id, pInsertBlankPages, InsertBlankPages, redirect);
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

                var package = DocumentTemplatePackages.GetPackage(id);

                if (package == null)
                    throw new ArgumentException("Invalid Document Template Package Id", nameof(id));

                UpdateJobSubTypes(package, JobSubTypes);

                if (redirect)
                    return RedirectToAction(MVC.Config.DocumentTemplate.ShowPackage(package.Id));
                else
                    return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (redirect)
                    throw;
                else
                    return Json($"Error: {ex.Message}", JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region Update Properties
        private void UpdateDescription(DocumentTemplatePackage Package, string Description)
        {
            if (!string.IsNullOrWhiteSpace(Description))
            {
                var description = Description.Trim();
                if (Package.Description != description)
                {
                    Package.Description = description;
                    DocumentTemplatePackages.UpdatePackage(Package);
                }
            }
            throw new Exception("Invalid Description");
        }
        private void UpdateDocumentTemplates(DocumentTemplatePackage Package, List<string> DocumentTemplates)
        {
            List<string> documentTemplateIds = null;

            if (DocumentTemplates != null && DocumentTemplates.Count > 0)
            {
                var packageScope = Package.Scope.ToString();

                // Collect Valid from Database (maintain order)
                documentTemplateIds = new List<string>(DocumentTemplates.Count);
                foreach (var templateId in DocumentTemplates)
                {
                    var dbId = Database.DocumentTemplates
                        .Where(dt => dt.Scope == packageScope && dt.Id == templateId)
                        .Select(dt => dt.Id).FirstOrDefault();
                    if (dbId != null)
                    {
                        documentTemplateIds.Add(dbId);
                    }
                }
            }

            if (documentTemplateIds == null)
            {
                if (Package.DocumentTemplateIds != null)
                {
                    Package.DocumentTemplateIds = null;
                    DocumentTemplatePackages.UpdatePackage(Package);
                }
            }
            else
            {
                if (Package.DocumentTemplateIds == null || Package.DocumentTemplateIds.Count != documentTemplateIds.Count)
                {
                    Package.DocumentTemplateIds = documentTemplateIds;
                    DocumentTemplatePackages.UpdatePackage(Package);
                }
                else
                {
                    if (Package.DocumentTemplateIds.Zip(documentTemplateIds, (a, b) => a != b).Any(r => r))
                    {
                        Package.DocumentTemplateIds = documentTemplateIds;
                        DocumentTemplatePackages.UpdatePackage(Package);
                    }
                }
            }

        }
        private void UpdateScope(DocumentTemplatePackage Package, string Scope)
        {
            if (!Enum.TryParse<AttachmentTypes>(Scope, true, out var scope))
                throw new ArgumentException("Invalid Scope", nameof(Scope));

            if (Package.Scope != scope)
            {
                Package.Scope = scope;

                // Remove all Templates (none can be of the same scope)
                Package.DocumentTemplateIds = null;

                DocumentTemplatePackages.UpdatePackage(Package);
            }
        }
        private void UpdateFilterExpression(DocumentTemplatePackage Package, string FilterExpression)
        {
            string expression;
            if (string.IsNullOrWhiteSpace(FilterExpression))
                expression = null;
            else
                expression = FilterExpression.Trim();

            if (Package.FilterExpression != expression)
            {
                Package.FilterExpression = expression;
                DocumentTemplatePackages.UpdatePackage(Package);
                Package.FilterExpressionInvalidateCache();
            }
        }
        private void UpdateOnGenerateExpression(DocumentTemplatePackage Package, string OnGenerateExpression)
        {
            string expression;
            if (string.IsNullOrWhiteSpace(OnGenerateExpression))
                expression = null;
            else
                expression = OnGenerateExpression.Trim();

            if (Package.OnGenerateExpression != expression)
            {
                Package.OnGenerateExpression = expression;
                DocumentTemplatePackages.UpdatePackage(Package);
                Package.OnGenerateExpressionInvalidateCache();
            }
        }
        private void UpdateIsHidden(DocumentTemplatePackage Package, string IsHidden)
        {
            var isHidden = false;

            if (!string.IsNullOrWhiteSpace(IsHidden) && !bool.TryParse(IsHidden, out isHidden))
                throw new ArgumentOutOfRangeException(nameof(IsHidden));

            if (Package.IsHidden != isHidden)
            {
                Package.IsHidden = isHidden;
                DocumentTemplatePackages.UpdatePackage(Package);
            }
        }
        private void UpdateInsertBlankPages(DocumentTemplatePackage Package, string InsertBlankPages)
        {
            var insertBlankPages = false;

            if (!string.IsNullOrWhiteSpace(InsertBlankPages) && !bool.TryParse(InsertBlankPages, out insertBlankPages))
                throw new ArgumentOutOfRangeException(nameof(InsertBlankPages));

            if (Package.InsertBlankPages != insertBlankPages)
            {
                Package.InsertBlankPages = insertBlankPages;
                DocumentTemplatePackages.UpdatePackage(Package);
            }
        }
        private void UpdateJobSubTypes(DocumentTemplatePackage Package, List<string> JobSubTypes)
        {
            List<string> jobSubTypes = null;

            if (JobSubTypes != null && JobSubTypes.Count > 0)
            {
                var subTypeIds = Database.JobSubTypes.Select(jst => jst.JobTypeId + "_" + jst.Id).ToList();

                jobSubTypes = subTypeIds
                    .Where(id => JobSubTypes.Contains(id, StringComparer.OrdinalIgnoreCase))
                    .OrderBy(id => id)
                    .ToList();
            }

            if (jobSubTypes == null)
            {
                if (Package.JobSubTypes != null)
                {
                    Package.JobSubTypes = null;
                    DocumentTemplatePackages.UpdatePackage(Package);
                }
            }
            else
            {
                if (Package.JobSubTypes == null || Package.JobSubTypes.Count != jobSubTypes.Count)
                {
                    Package.JobSubTypes = jobSubTypes;
                    DocumentTemplatePackages.UpdatePackage(Package);
                }
                else
                {
                    if (Package.JobSubTypes.Zip(jobSubTypes, (a, b) => a != b).Any(r => r))
                    {
                        Package.JobSubTypes = jobSubTypes;
                        DocumentTemplatePackages.UpdatePackage(Package);
                    }
                }
            }

        }
        #endregion

        #region Actions

        [DiscoAuthorize(Claims.Config.DocumentTemplate.BulkGenerate)]
        public virtual ActionResult BulkGenerate(string id, string DataIds = null, bool InsertBlankPage = false)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrEmpty(DataIds))
                throw new ArgumentNullException(nameof(DataIds));

            var package = DocumentTemplatePackages.GetPackage(id);

            if (package == null)
                throw new ArgumentException("Invalid Document Template Package Id", "id");

            switch (package.Scope)
            {
                case AttachmentTypes.Device:
                    Authorization.Require(Claims.Device.Actions.GenerateDocuments);
                    break;
                case AttachmentTypes.Job:
                    Authorization.Require(Claims.Job.Actions.GenerateDocuments);
                    break;
                case AttachmentTypes.User:
                    Authorization.Require(Claims.User.Actions.GenerateDocuments);
                    break;
                default:
                    throw new InvalidOperationException("Unknown DocumentType Scope");
            }

            var dataIds = DataIds.Split(new string[] { Environment.NewLine, ",", ";" }, StringSplitOptions.RemoveEmptyEntries).Select(d => d.Trim()).Where(d => !string.IsNullOrEmpty(d)).ToList();
            var timeStamp = DateTime.Now;
            var pdf = package.GeneratePdfPackageBulk(Database, UserService.CurrentUser, timeStamp, InsertBlankPage, dataIds);

            return File(pdf, "application/pdf", $"{package.Id}_Bulk_{timeStamp:yyyyMMdd-HHmmss}.pdf");
        }

        public virtual ActionResult Generate(string id, string TargetId)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrWhiteSpace(TargetId))
                throw new ArgumentNullException(nameof(TargetId));

            var package = DocumentTemplatePackages.GetPackage(id);
            if (package == null)
                throw new ArgumentException("Invalid document template package id", nameof(id));

            switch (package.Scope)
            {
                case AttachmentTypes.Device:
                    Authorization.Require(Claims.Device.Actions.GenerateDocuments);
                    break;
                case AttachmentTypes.Job:
                    Authorization.Require(Claims.Job.Actions.GenerateDocuments);
                    break;
                case AttachmentTypes.User:
                    Authorization.Require(Claims.User.Actions.GenerateDocuments);
                    break;
                default:
                    throw new InvalidOperationException("Unknown document type scope");
            }

            // resolve target
            var target = package.ResolveScopeTarget(Database, TargetId);
            if (target == null)
                throw new ArgumentException("Target not found", nameof(TargetId));

            var timestamp = DateTime.Now;
            var document = default(Stream);
            using (var state = DocumentState.DefaultState())
            {
                document = package.GeneratePdfPackage(Database, target, UserService.CurrentUser, timestamp, state);
            }
            Database.SaveChanges();

            return File(document, "application/pdf", $"{package.Id}_{target.AttachmentReferenceId.Replace('\\', '_')}_{timestamp:yyyyMMdd-HHmmss}.pdf");
        }

        [DiscoAuthorize(Claims.Config.DocumentTemplate.Delete)]
        public virtual ActionResult Delete(string id, bool? redirect = false)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new ArgumentNullException("id");

                var package = DocumentTemplatePackages.GetPackage(id);

                if (package == null)
                    throw new ArgumentException("Invalid Document Template Package Id", nameof(id));

                if (package != null)
                {
                    DocumentTemplatePackages.RemovePackage(package.Id);

                    if (redirect.HasValue && redirect.Value)
                        return RedirectToAction(MVC.Config.DocumentTemplate.Index(null));
                    else
                        return Json("OK", JsonRequestBehavior.AllowGet);
                }
                throw new Exception("Invalid Document Template Package Id");
            }
            catch (Exception ex)
            {
                if (redirect.HasValue && redirect.Value)
                    throw;
                else
                    return Json($"Error: {ex.Message}", JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}