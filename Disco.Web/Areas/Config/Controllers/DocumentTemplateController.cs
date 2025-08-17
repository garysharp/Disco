using Disco.BI.Extensions;
using Disco.Data.Repository;
using Disco.Models.Areas.Config.UI.UserFlag;
using Disco.Models.Repository;
using Disco.Models.Services.Documents;
using Disco.Models.UI.Config.DocumentTemplate;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Documents;
using Disco.Services.Documents.ManagedGroups;
using Disco.Services.Exporting;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Web;
using Disco.Web.Areas.Config.Models.DocumentTemplate;
using Disco.Web.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class DocumentTemplateController : AuthorizedDatabaseController
    {
        [DiscoAuthorize(Claims.Config.DocumentTemplate.Show)]
        public virtual ActionResult Index(string id, Guid? bulkGenerateId = null, string bulkGenerateFilename = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                var m = new IndexModel()
                {
                    DocumentTemplates = Database.DocumentTemplates
                        .Select(dt => new
                        {
                            documentTemplate = dt,
                            storedInstances =
                                Database.DeviceAttachments.Count(a => a.DocumentTemplateId == dt.Id) +
                                Database.JobAttachments.Count(a => a.DocumentTemplateId == dt.Id) +
                                Database.UserAttachments.Count(a => a.DocumentTemplateId == dt.Id)
                        })
                        .ToDictionary(i => i.documentTemplate, i => i.storedInstances),
                    Packages = DocumentTemplatePackages.GetPackages()
                };

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigDocumentTemplateIndexModel>(ControllerContext, m);

                return View(m);
            }
            else
            {
                // Normal Document Template
                var m = new ShowModel()
                {
                    DocumentTemplate = Database.DocumentTemplates.Include("JobSubTypes").FirstOrDefault(at => at.Id == id)
                };
                if (m.DocumentTemplate == null)
                    throw new ArgumentException("Invalid Document Template Id", nameof(id));

                m.TemplatePagesHaveAttachmentId = m.DocumentTemplate.PdfPageHasAttachmentId(Database);
                m.TemplateExpressions = m.DocumentTemplate.ExtractPdfExpressions(Database);
                m.UpdateModel(Database);

                if (DocumentTemplateDevicesManagedGroup.TryGetManagedGroup(m.DocumentTemplate, out var devicesManagedGroup))
                    m.DevicesLinkedGroup = devicesManagedGroup;
                if (DocumentTemplateUsersManagedGroup.TryGetManagedGroup(m.DocumentTemplate, out var usersManagedGroup))
                    m.UsersLinkedGroup = usersManagedGroup;

                m.BulkGenerateDownloadId = bulkGenerateId;
                m.BulkGenerateDownloadFilename = bulkGenerateFilename;

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigDocumentTemplateShowModel>(ControllerContext, m);

                return View(MVC.Config.DocumentTemplate.Views.Show, m);
            }
        }

        [DiscoAuthorize(Claims.Config.DocumentTemplate.Show)]
        public virtual ActionResult ShowPackage(string id)
        {
            // Document Template Package
            var m = new ShowPackageModel()
            {
                Package = DocumentTemplatePackages.GetPackage(id)
            };
            if (m.Package == null)
                throw new ArgumentException("Invalid Document Template Package Id", nameof(id));

            if (m.Package.Scope == AttachmentTypes.Job)
            {
                m.JobTypes = Database.JobTypes.Include("JobSubTypes").ToList();
                m.JobSubTypesSelected = m.Package.GetJobSubTypes(m.JobTypes.SelectMany(jt => jt.JobSubTypes));
            }
            var packageScopeString = m.Package.Scope.ToString();

            m.DocumentTemplates = Database.DocumentTemplates.Where(dt => dt.Scope == packageScopeString).ToList();
            m.DocumentTemplatesSelected = m.Package.GetDocumentTemplates(m.DocumentTemplates);

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDocumentTemplateShowPackageModel>(ControllerContext, m);

            return View(MVC.Config.DocumentTemplate.Views.ShowPackage, m);
        }

        [DiscoAuthorize(Claims.Config.DocumentTemplate.ShowStatus)]
        public virtual ActionResult ImportStatus()
        {
            var m = new ImportStatusModel();

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDocumentTemplateImportStatusModel>(ControllerContext, m);

            return View();
        }

        [DiscoAuthorize(Claims.Config.DocumentTemplate.UndetectedPages)]
        public virtual ActionResult UndetectedPages()
        {
            var m = new UndetectedPagesModel()
            {
                DocumentTemplates = Database.DocumentTemplates.ToList()
            };

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDocumentTemplateUndetectedPagesModel>(ControllerContext, m);

            return View(m);
        }

        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.Create, Claims.Config.DocumentTemplate.Configure)]
        public virtual ActionResult Create()
        {
            var m = new CreateModel();
            m.UpdateModel(Database);

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDocumentTemplateCreateModel>(ControllerContext, m);

            return View(m);
        }

        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.Create, Claims.Config.DocumentTemplate.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Create(CreateModel model)
        {
            model.UpdateModel(Database);

            if (ModelState.IsValid)
            {
                // Check for Existing
                var existing = Database.DocumentTemplates.Where(m => m.Id == model.Id).FirstOrDefault();
                if (existing == null)
                {
                    var template = new DocumentTemplate()
                    {
                        Id = model.Id,
                        Description = model.Description,
                        Scope = model.Scope,
                    };

                    if (model.Scope == DocumentTemplate.DocumentTemplateScopes.Job)
                        template.JobSubTypes = model.GetJobSubTypes();

                    Database.DocumentTemplates.Add(template);
                    Database.SaveChanges();

                    // Save Template
                    template.SavePdfTemplate(Database, model.Template.InputStream);

                    return RedirectToAction(MVC.Config.DocumentTemplate.Index(template.Id));
                }
                else
                {
                    ModelState.AddModelError(nameof(DocumentTemplate.Id), "A Document Template with this Id already exists.");
                }
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDocumentTemplateCreateModel>(ControllerContext, model);

            return View(model);
        }

        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.Create, Claims.Config.DocumentTemplate.Configure)]
        public virtual ActionResult CreatePackage()
        {
            var m = new CreatePackageModel();

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDocumentTemplateCreatePackageModel>(ControllerContext, m);

            return View(m);
        }

        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.Create, Claims.Config.DocumentTemplate.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult CreatePackage(CreatePackageModel model)
        {
            if (ModelState.IsValid)
            {
                // Check for Existing
                var existing = DocumentTemplatePackages.GetPackage(model.Id);
                if (existing == null)
                {
                    DocumentTemplatePackages.CreatePackage(model.Id, model.Description, model.Scope);

                    return RedirectToAction(MVC.Config.DocumentTemplate.ShowPackage(model.Id));
                }
                else
                {
                    ModelState.AddModelError(nameof(model.Id), "A Document Template Package with this Id already exists.");
                }
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDocumentTemplateCreatePackageModel>(ControllerContext, model);

            return View(model);
        }

        public static (string viewName, ConfigDocumentTemplateBulkGenerate model) BuildBulkGenerateModel(DocumentTemplate documentTemplate, DiscoDataContext database, AuthorizationToken authorization)
        {
            BulkGenerateModel model;
            string viewName;

            if (documentTemplate.Scope == DocumentTemplate.DocumentTemplateScopes.User)
            {
                authorization.Require(Claims.User.Actions.GenerateDocuments);
                model = new BulkGenerateUserModel()
                {
                    UserFlags = database.UserFlags.Select(f => new ItemWithCount<UserFlag>()
                    {
                        Item = f,
                        Count = f.UserFlagAssignments.Where(a => a.RemovedDate == null).Count(),
                    }).ToList(),
                };
                model.DeviceProfiles = database.DeviceProfiles.Select(p => new ItemWithCount<DeviceProfile>()
                {
                    Item = p,
                    Count = p.Devices.Where(d => d.AssignedUserId != null).Count(),
                }).ToList();
                model.DeviceBatches = database.DeviceBatches.Select(p => new ItemWithCount<DeviceBatch>()
                {
                    Item = p,
                    Count = p.Devices.Where(d => d.AssignedUserId != null).Count(),
                }).ToList();
                model.DocumentTemplates = database.DocumentTemplates.Select(dt => new ItemWithCount<DocumentTemplate>()
                {
                    Item = dt,
                }).ToList();
                foreach (var record in model.DocumentTemplates)
                {
                    switch (record.Item.AttachmentType)
                    {
                        case AttachmentTypes.Device:
                            record.Count = database.DeviceAttachments.Where(a => a.DocumentTemplateId == record.Item.Id).Select(a => a.Device.AssignedUser).Distinct().Count();
                            break;
                        case AttachmentTypes.Job:
                            record.Count = database.JobAttachments.Where(a => a.DocumentTemplateId == record.Item.Id).Select(a => a.Job.User).Distinct().Count();
                            break;
                        case AttachmentTypes.User:
                            record.Count = database.UserAttachments.Where(a => a.DocumentTemplateId == record.Item.Id).Select(a => a.User).Distinct().Count();
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }
                if (authorization.Has(Claims.User.ShowDetails))
                {
                    model.UserDetails = database.UserDetails.Where(d => d.Scope == "Details").GroupBy(d => d.Key).Select(g => new ItemWithCount<string>()
                    {
                        Item = g.Key,
                        Count = g.Count(),
                    }).ToList();
                }
                else
                {
                    model.UserDetails = new List<ItemWithCount<string>>();
                }
                viewName = MVC.Config.DocumentTemplate.Views.BulkGenerateUser;
            }
            else if (documentTemplate.Scope == DocumentTemplate.DocumentTemplateScopes.Device)
            {
                authorization.Require(Claims.Device.Actions.GenerateDocuments);
                model = new BulkGenerateDeviceModel()
                {
                    DeviceFlags = database.DeviceFlags.Select(f => new ItemWithCount<DeviceFlag>()
                    {
                        Item = f,
                        Count = f.DeviceFlagAssignments.Where(a => a.RemovedDate == null).Count(),
                    }).ToList(),
                };
                model.DeviceProfiles = database.DeviceProfiles.Select(p => new ItemWithCount<DeviceProfile>()
                {
                    Item = p,
                    Count = p.Devices.Count(),
                }).ToList();
                model.DeviceBatches = database.DeviceBatches.Select(p => new ItemWithCount<DeviceBatch>()
                {
                    Item = p,
                    Count = p.Devices.Count(),
                }).ToList();
                model.DocumentTemplates = database.DocumentTemplates.Select(dt => new ItemWithCount<DocumentTemplate>()
                {
                    Item = dt,
                }).ToList();
                foreach (var record in model.DocumentTemplates)
                {
                    switch (record.Item.AttachmentType)
                    {
                        case AttachmentTypes.Device:
                            record.Count = database.DeviceAttachments.Where(a => a.DocumentTemplateId == record.Item.Id).Select(a => a.Device).Distinct().Count();
                            break;
                        case AttachmentTypes.Job:
                            record.Count = database.JobAttachments.Where(a => a.DocumentTemplateId == record.Item.Id).Select(a => a.Job.Device).Distinct().Count();
                            break;
                        case AttachmentTypes.User:
                            record.Count = database.UserAttachments.Where(a => a.DocumentTemplateId == record.Item.Id).SelectMany(a => a.User.DeviceUserAssignments).Where(a => a.UnassignedDate == null).Select(a => a.DeviceSerialNumber).Distinct().Count();
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }
                if (authorization.Has(Claims.User.ShowDetails))
                {
                    model.UserDetails = database.UserDetails.Where(d => d.Scope == "Details").GroupBy(d => d.Key).Select(g => new ItemWithCount<string>()
                    {
                        Item = g.Key,
                        Count = g.Select(i => i.User).SelectMany(u => u.DeviceUserAssignments).Where(a => a.UnassignedDate == null).Count(),
                    }).ToList();
                }
                else
                {
                    model.UserDetails = new List<ItemWithCount<string>>();
                }
                viewName = MVC.Config.DocumentTemplate.Views.BulkGenerateDevice;
            }
            else
                throw new NotSupportedException("Only user and device scoped document templates can be bulk generated using this method");

            model.DocumentTemplate = documentTemplate;

            model.TemplatePageCount = model.DocumentTemplate.PdfPageHasAttachmentId(database).Count;

            return (viewName, model);
        }

        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.BulkGenerate)]
        public virtual ActionResult BulkGenerate(string id)
        {
            var documentTemplate = Database.DocumentTemplates.FirstOrDefault(at => at.Id == id);

            if (documentTemplate == null)
                throw new ArgumentException("Invalid Document Template Id", nameof(id));

            if (documentTemplate.Scope != DocumentTemplate.DocumentTemplateScopes.User && documentTemplate.Scope != DocumentTemplate.DocumentTemplateScopes.Device)
                throw new NotSupportedException("Only user-scoped document templates can be bulk generated using this method");

            var (viewName, model) = BuildBulkGenerateModel(documentTemplate, Database, Authorization);

            // UI Extensions
            UIExtensions.ExecuteExtensions(ControllerContext, model);

            return View(viewName, model);
        }

        [HttpGet]
        [DiscoAuthorize(Claims.Config.DocumentTemplate.Export)]
        public virtual ActionResult Export(string id, Guid? exportId)
        {
            var m = new ExportModel()
            {
                Options = Database.DiscoConfiguration.Documents.LastExportOptions,
                DocumentTemplates = Database.DocumentTemplates.OrderBy(d => d.Id).ToList(),
            };

            m.Fields = ExportFieldsModel.Create(m.Options, DocumentExportOptions.DefaultOptions(), nameof(DocumentExportOptions.LatestOnly));
            m.Fields.AddCustomUserDetails(o => o.UserDetailCustom);

            if (ExportTask.TryFromCache(exportId, out var context))
            {
                m.ExportId = exportId;
                m.ExportResult = context.Result;
            }

            if (!string.IsNullOrWhiteSpace(id))
            {
                var template = m.DocumentTemplates.FirstOrDefault(d => string.Equals(d.Id, id, StringComparison.OrdinalIgnoreCase));

                if (template != null)
                {
                    m.Options.DocumentTemplateIds.Clear();
                    m.Options.DocumentTemplateIds.Add(template.Id);
                }
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDocumentTemplateExportModel>(ControllerContext, m);

            return View(m);
        }

        public virtual ActionResult ExpressionBrowser()
        {
            // for backwards compatibility
            return RedirectToAction(MVC.Config.Expressions.Browser());
        }

    }
}
