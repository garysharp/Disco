using Disco.BI.DocumentTemplateBI.ManagedGroups;
using Disco.BI.Extensions;
using Disco.Models.UI.Config.DocumentTemplate;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Expressions;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class DocumentTemplateController : AuthorizedDatabaseController
    {
        [DiscoAuthorize(Claims.Config.DocumentTemplate.Show)]
        public virtual ActionResult Index(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                var m = new Models.DocumentTemplate.IndexModel() { DocumentTemplates = Database.DocumentTemplates.ToList() };

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigDocumentTemplateIndexModel>(this.ControllerContext, m);

                return View(m);
            }
            else
            {
                var m = new Models.DocumentTemplate.ShowModel()
                {
                    DocumentTemplate = Database.DocumentTemplates.Include("JobSubTypes").FirstOrDefault(at => at.Id == id)
                };
                m.TemplateExpressions = m.DocumentTemplate.ExtractPdfExpressions(Database);
                m.UpdateModel(Database);

                DocumentTemplateDevicesManagedGroup devicesManagedGroup;
                if (DocumentTemplateDevicesManagedGroup.TryGetManagedGroup(m.DocumentTemplate, out devicesManagedGroup))
                    m.DevicesLinkedGroup = devicesManagedGroup;
                DocumentTemplateUsersManagedGroup usersManagedGroup;
                if (DocumentTemplateUsersManagedGroup.TryGetManagedGroup(m.DocumentTemplate, out usersManagedGroup))
                    m.UsersLinkedGroup = usersManagedGroup;

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigDocumentTemplateShowModel>(this.ControllerContext, m);

                return View(MVC.Config.DocumentTemplate.Views.Show, m);
            }
        }

        [DiscoAuthorize(Claims.Config.DocumentTemplate.ShowStatus)]
        public virtual ActionResult ImportStatus()
        {
            var m = new Models.DocumentTemplate.ImportStatusModel();

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDocumentTemplateImportStatusModel>(this.ControllerContext, m);

            return View();
        }

        [DiscoAuthorize(Claims.Config.DocumentTemplate.UndetectedPages)]
        public virtual ActionResult UndetectedPages()
        {
            var m = new Models.DocumentTemplate.UndetectedPagesModel()
            {
                DocumentTemplates = Database.DocumentTemplates.ToList()
            };

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDocumentTemplateUndetectedPagesModel>(this.ControllerContext, m);

            return View(m);
        }

        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.Create, Claims.Config.DocumentTemplate.Configure)]
        public virtual ActionResult Create()
        {
            var m = new Models.DocumentTemplate.CreateModel();
            m.UpdateModel(Database);

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDocumentTemplateCreateModel>(this.ControllerContext, m);

            return View(m);
        }

        [DiscoAuthorizeAll(Claims.Config.DocumentTemplate.Create, Claims.Config.DocumentTemplate.Configure), HttpPost]
        public virtual ActionResult Create(Models.DocumentTemplate.CreateModel model)
        {
            model.UpdateModel(Database);

            if (ModelState.IsValid)
            {
                // Check for Existing
                var existing = Database.DocumentTemplates.Where(m => m.Id == model.DocumentTemplate.Id).FirstOrDefault();
                if (existing == null)
                {

                    Database.DocumentTemplates.Add(model.DocumentTemplate);

                    if (model.DocumentTemplate.Scope == Disco.Models.Repository.DocumentTemplate.DocumentTemplateScopes.Job)
                    {
                        var jobSubTypes = new List<Disco.Models.Repository.JobSubType>();
                        jobSubTypes.AddRange(model.GetJobSubTypes);
                        model.DocumentTemplate.JobSubTypes = jobSubTypes;
                        //foreach (var jobSubType in model.GetJobSubTypes)
                        //    model.AttachmentType.JobSubTypes.Add(jobSubType);
                    }

                    Database.SaveChanges();

                    // Save Template
                    model.DocumentTemplate.SavePdfTemplate(Database, model.Template.InputStream);

                    return RedirectToAction(MVC.Config.DocumentTemplate.Index(model.DocumentTemplate.Id));
                }
                else
                {
                    ModelState.AddModelError("Name", "A Document Template with this Name already exists.");
                }
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDocumentTemplateCreateModel>(this.ControllerContext, model);

            return View(model);
        }

        [DiscoAuthorize(Claims.Config.Show)]
        public virtual ActionResult ExpressionBrowser(string type, bool StaticDeclaredMembersOnly = false)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                var m = new Models.DocumentTemplate.ExpressionBrowserModel()
                {
                    DeviceType = typeof(Disco.Models.Repository.Device).AssemblyQualifiedName,
                    JobType = typeof(Disco.Models.Repository.Job).AssemblyQualifiedName,
                    UserType = typeof(Disco.Models.Repository.User).AssemblyQualifiedName,
                    Variables = Expression.StandardVariableTypes(),
                    ExtensionLibraries = Expression.ExtensionLibraryTypes()
                };

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigDocumentTemplateExpressionBrowserModel>(this.ControllerContext, m);

                return View(m);
            }
            else
            {
                var t = Type.GetType(type);
                if (t != null)
                {
                    return Json(ExpressionTypeDescriptor.Build(t, StaticDeclaredMembersOnly), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Invalid Type Specified", JsonRequestBehavior.AllowGet);
                }
            }
        }

    }
}
