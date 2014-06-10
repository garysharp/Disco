using Disco.Models.Repository;
using Disco.Models.Services.Jobs.JobQueues;
using Disco.Models.UI.Config.JobQueue;
using Disco.Services.Authorization;
using Disco.Services.Extensions;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Jobs.JobQueues;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Web;
using Disco.Web.Areas.API.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class JobQueueController : AuthorizedDatabaseController
    {
        [DiscoAuthorize(Claims.Config.JobQueue.Show)]
        public virtual ActionResult Index(int? id)
        {
            if (id.HasValue)
            {
                // Show
                var jq = Database.JobQueues.Include("JobSubTypes").FirstOrDefault(q => q.Id == id.Value);

                if (jq == null)
                    throw new ArgumentException("Invalid Job Queue Id");

                var token = JobQueueToken.FromJobQueue(jq);
                var subjects = token.SubjectIds == null ? new List<SubjectDescriptorModel>() :
                    token.SubjectIds.Select(subjectId => ActiveDirectory.RetrieveADObject(subjectId, Quick: true))
                    .Where(item => item != null)
                    .Select(item => SubjectDescriptorModel.FromActiveDirectoryObject(item))
                    .OrderBy(item => item.Name).ToList();

                var totalJobCount = Database.JobQueueJobs.Where(jqj => jqj.JobQueueId == id.Value).Select(jqj => jqj.Job).Distinct().Count();
                var openJobCount = Database.JobQueueJobs.Count(jqj => jqj.JobQueueId == id.Value && !jqj.RemovedDate.HasValue);

                var m = new Models.JobQueue.ShowModel()
                {
                    Token = token,
                    Subjects = subjects,
                    TotalJobCount = totalJobCount,
                    OpenJobCount = openJobCount,
                    CanDelete = openJobCount == 0
                };

                if (Authorization.Has(Claims.Config.JobQueue.Configure))
                {
                    m.JobTypes = Database.JobTypes.Include("JobSubTypes").ToList();
                    m.Icons = UIHelpers.Icons;
                    m.ThemeColours = UIHelpers.ThemeColours;
                }

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigJobQueueShowModel>(this.ControllerContext, m);

                return View(MVC.Config.JobQueue.Views.Show, m);
            }
            else
            {
                // List Index
                var jqs = Database.JobQueues.OrderBy(jq => jq.Name).ToList()
                    .Select(jq => JobQueueToken.FromJobQueue(jq)).Cast<IJobQueueToken>().ToList();

                var m = new Models.JobQueue.IndexModel()
                {
                    Tokens = jqs
                };

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigJobQueueIndexModel>(this.ControllerContext, m);

                return View(m);
            }
        }

        [DiscoAuthorizeAll(Claims.Config.JobQueue.Create, Claims.Config.JobQueue.Configure)]
        public virtual ActionResult Create()
        {
            // Default Queue
            var m = new Models.JobQueue.CreateModel()
            {
                JobQueue = new Disco.Models.Repository.JobQueue()
                {
                    Icon = JobQueueService.RandomUnusedIcon(),
                    IconColour = JobQueueService.RandomUnusedThemeColour(),
                    Priority = JobQueuePriority.Normal
                }
            };

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigJobQueueCreateModel>(this.ControllerContext, m);

            return View(m);
        }

        [DiscoAuthorizeAll(Claims.Config.JobQueue.Create, Claims.Config.JobQueue.Configure), HttpPost]
        public virtual ActionResult Create(Models.JobQueue.CreateModel model)
        {
            if (ModelState.IsValid)
            {
                // Check for Existing
                var existing = Database.JobQueues.Where(m => m.Name == model.JobQueue.Name).FirstOrDefault();
                if (existing == null)
                {
                    var token = JobQueueService.CreateJobQueue(Database, model.JobQueue);

                    return RedirectToAction(MVC.Config.JobQueue.Index(token.JobQueue.Id));
                }
                else
                {
                    ModelState.AddModelError("Name", "A Job Queue with this name already exists.");
                }
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigJobQueueCreateModel>(this.ControllerContext, model);

            return View(model);
        }
    }
}
