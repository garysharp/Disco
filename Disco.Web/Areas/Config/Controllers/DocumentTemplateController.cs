using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.BI;
using Disco.BI.Extensions;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class DocumentTemplateController : dbAdminController
    {

        public virtual ActionResult Index(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                var m = new Models.DocumentTemplate.IndexModel() { DocumentTemplates = dbContext.DocumentTemplates.ToList() };
                return View(m);
            }
            else
            {
                var m = new Models.DocumentTemplate.ShowModel()
                {
                    DocumentTemplate = dbContext.DocumentTemplates.Include("JobSubTypes").Where(at => at.Id == id).FirstOrDefault()
                };
                m.TemplateExpressions = m.DocumentTemplate.ExtractPdfExpressions(dbContext);
                m.UpdateModel(dbContext);
                return View(MVC.Config.DocumentTemplate.Views.Show, m);
            }
        }

        public virtual ActionResult ImportStatus()
        {
            return View();
        }
        public virtual ActionResult UndetectedPages()
        {
            var m = new Models.DocumentTemplate.UndetectedPagesModel()
            {
                DocumentTemplates = dbContext.DocumentTemplates.ToList()
            };

            return View(m);
        }

        public virtual ActionResult Create()
        {
            var m = new Models.DocumentTemplate.CreateModel();
            m.UpdateModel(dbContext);

            return View(m);
        }

        [HttpPost]
        public virtual ActionResult Create(Models.DocumentTemplate.CreateModel model)
        {
            model.UpdateModel(dbContext);

            if (ModelState.IsValid)
            {
                // Check for Existing
                var existing = dbContext.DocumentTemplates.Where(m => m.Id == model.DocumentTemplate.Id).FirstOrDefault();
                if (existing == null)
                {

                    dbContext.DocumentTemplates.Add(model.DocumentTemplate);

                    if (model.DocumentTemplate.Scope == Disco.Models.Repository.DocumentTemplate.DocumentTemplateScopes.Job)
                    {
                        var jobSubTypes = new List<Disco.Models.Repository.JobSubType>();
                        jobSubTypes.AddRange(model.GetJobSubTypes);
                        model.DocumentTemplate.JobSubTypes = jobSubTypes;
                        //foreach (var jobSubType in model.GetJobSubTypes)
                        //    model.AttachmentType.JobSubTypes.Add(jobSubType);
                    }

                    dbContext.SaveChanges();

                    // Save Template
                    model.DocumentTemplate.SavePdfTemplate(dbContext, model.Template.InputStream);

                    return RedirectToAction(MVC.Config.DocumentTemplate.Index(model.DocumentTemplate.Id));
                }
                else
                {
                    ModelState.AddModelError("Name", "A Document Template with this Name already exists.");
                }
            }
            return View(model);
        }

        public virtual ActionResult ExpressionBrowser(string type, bool StaticDeclaredMembersOnly = false)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                var m = new Models.DocumentTemplate.ExpressionBrowserModel()
                {
                    DeviceType = typeof(Disco.Models.Repository.Device).AssemblyQualifiedName,
                    JobType = typeof(Disco.Models.Repository.Job).AssemblyQualifiedName,
                    UserType = typeof(Disco.Models.Repository.User).AssemblyQualifiedName,
                    Variables = BI.Expressions.Expression.StandardVariableTypes(),
                    ExtensionLibraries = BI.Expressions.Expression.ExtensionLibraryTypes()
                };

                return View(m);
            }
            else
            {
                var t = Type.GetType(type);
                if (t != null)
                {
                    return Json(BI.Expressions.ExpressionTypeDescriptor.Build(t, StaticDeclaredMembersOnly), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Invalid Type Specified", JsonRequestBehavior.AllowGet);
                }
            }
        }

    }
}
