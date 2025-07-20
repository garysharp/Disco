using Disco.Models.Repository;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Jobs.JobQueues;
using Disco.Services.Web;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class JobQueueJobController : AuthorizedDatabaseController
    {
        const string pAddedComment = "addedcomment";
        const string pRemovedComment = "removedcomment";
        const string pSla = "sla";
        const string pPriority = "priority";

        public virtual ActionResult Update(int id, string key, string value = null, bool? redirect = null)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");
                var jobQueueJob = Database.JobQueueJobs.Include("Job").FirstOrDefault(jqj => jqj.Id == id);
                if (jobQueueJob != null)
                {
                    switch (key.ToLower())
                    {
                        case pAddedComment:
                            UpdateAddedComment(jobQueueJob, value);
                            break;
                        case pRemovedComment:
                            UpdateRemovedComment(jobQueueJob, value);
                            break;
                        case pSla:
                            UpdateSla(jobQueueJob, value);
                            break;
                        case pPriority:
                            UpdatePriority(jobQueueJob, value);
                            break;
                        default:
                            throw new Exception("Invalid Update Key");
                    }
                }
                else
                {
                    throw new Exception("Invalid Job Queue Job Id");
                }
                if (redirect.HasValue && redirect.Value)
                    return Redirect($"{Url.Action(MVC.Job.Show(jobQueueJob.JobId))}#jobDetailTab-Queues");
                else
                    return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (redirect.HasValue && redirect.Value)
                    throw;
                else
                    return Json($"Error: {ex.Message}", JsonRequestBehavior.AllowGet);
            }
        }

        #region Update Shortcut Methods
        [DiscoAuthorizeAny(Claims.Job.Properties.JobQueueProperties.EditAnyComments, Claims.Job.Properties.JobQueueProperties.EditOwnComments)]
        public virtual ActionResult UpdateAddedComment(int id, string AddedComment = null, bool? redirect = null)
        {
            return Update(id, pAddedComment, AddedComment, redirect);
        }
        [DiscoAuthorizeAny(Claims.Job.Properties.JobQueueProperties.EditAnyComments, Claims.Job.Properties.JobQueueProperties.EditOwnComments)]
        public virtual ActionResult UpdateRemovedComment(int id, string RemovedComment = null, bool? redirect = null)
        {
            return Update(id, pRemovedComment, RemovedComment, redirect);
        }
        [DiscoAuthorizeAny(Claims.Job.Properties.JobQueueProperties.EditAnySLA, Claims.Job.Properties.JobQueueProperties.EditOwnSLA)]
        public virtual ActionResult UpdateSla(int id, string SLA = null, bool? redirect = null)
        {
            return Update(id, pSla, SLA, redirect);
        }
        [DiscoAuthorizeAny(Claims.Job.Properties.JobQueueProperties.EditAnyPriority, Claims.Job.Properties.JobQueueProperties.EditOwnPriority)]
        public virtual ActionResult UpdatePriority(int id, string Priority = null, bool? redirect = null)
        {
            return Update(id, pPriority, Priority, redirect);
        }
        [DiscoAuthorizeAny(Claims.Job.Properties.JobQueueProperties.EditAnySLA, Claims.Job.Properties.JobQueueProperties.EditOwnSLA,
            Claims.Job.Properties.JobQueueProperties.EditAnyPriority, Claims.Job.Properties.JobQueueProperties.EditOwnPriority)]
        public virtual ActionResult UpdateSlaAndPriority(int id, string Sla = null, string Priority = null, bool? redirect = null)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");

                var jobQueueJob = Database.JobQueueJobs.Include("Job").FirstOrDefault(jqj => jqj.Id == id);
                if (jobQueueJob != null)
                {
                    UpdateSla(jobQueueJob, Sla);
                    UpdatePriority(jobQueueJob, Priority);
                }
                else
                {
                    throw new Exception("Invalid Job Queue Job Id");
                }
                if (redirect.HasValue && redirect.Value)
                    return Redirect($"{Url.Action(MVC.Job.Show(jobQueueJob.JobId))}#jobDetailTab-Queues");
                else
                    return Json("OK", JsonRequestBehavior.AllowGet);
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

        #region Update Properties
        private void UpdateAddedComment(JobQueueJob jobQueueJob, string AddedComment)
        {
            if (!jobQueueJob.CanEditAddedComment())
                throw new InvalidOperationException("Editing added comment for job queue job is Denied");

            jobQueueJob.OnEditAddedComment(AddedComment);
            Database.SaveChanges();
        }
        private void UpdateRemovedComment(JobQueueJob jobQueueJob, string RemovedComment)
        {
            if (!jobQueueJob.CanEditRemovedComment())
                throw new InvalidOperationException("Editing removed comment for job queue job is Denied");

            jobQueueJob.OnEditRemovedComment(RemovedComment);
            Database.SaveChanges();
        }
        private void UpdateSla(JobQueueJob jobQueueJob, string Sla)
        {
            if (!jobQueueJob.CanEditSla())
                throw new InvalidOperationException("Editing SLA for job queue job is Denied");

            if (!string.IsNullOrEmpty(Sla))
            {
                if (DateTime.TryParse(Sla, out var SLADate))
                {
                    jobQueueJob.OnEditSla(SLADate);
                    Database.SaveChanges();
                }
                else
                {
                    throw new ArgumentException("Unable to Parse SLA Date", "SLA");
                }
            }
            else
            {
                jobQueueJob.OnEditSla(null);
                Database.SaveChanges();
            }
        }
        private void UpdatePriority(JobQueueJob jobQueueJob, string Priority)
        {
            if (!jobQueueJob.CanEditPriority())
                throw new InvalidOperationException("Editing Priority for job queue job is Denied");


            if (!Enum.TryParse<JobQueuePriority>(Priority, out var priority))
                throw new ArgumentException("Invalid Priority Value", "Priority");

            jobQueueJob.OnEditPriority(priority);
            Database.SaveChanges();
        }
        #endregion

        #region Actions

        [DiscoAuthorizeAny(Claims.Job.Actions.AddAnyQueues, Claims.Job.Actions.AddOwnQueues)]
        public virtual ActionResult AddJob(int id, int JobId, string Comment, int? SLAExpiresMinutes, JobQueuePriority Priority)
        {
            DateTime? SLAExpires = (SLAExpiresMinutes.HasValue && SLAExpiresMinutes.Value > 0) ? DateTime.Now.AddMinutes(SLAExpiresMinutes.Value) : (DateTime?)null;

            var jobQueueToken = JobQueueService.GetQueue(id);
            if (jobQueueToken == null)
                throw new ArgumentException("Invalid Job Queue Id", "id");

            var job = Database.Jobs.Include("JobQueues").FirstOrDefault(j => j.Id == JobId);
            if (job == null)
                throw new ArgumentException("Invalid Job Id", "JobId");

            if (!job.CanAddQueue(jobQueueToken.JobQueue))
                throw new InvalidOperationException("Adding job to queue is Denied");

            var jobQueueJob = job.OnAddQueue(Database, jobQueueToken.JobQueue, CurrentUser, Comment, SLAExpires, Priority);
            Database.SaveChanges();

            return Redirect($"{Url.Action(MVC.Job.Show(job.Id))}#jobDetailTab-Queues");
        }

        [DiscoAuthorizeAny(Claims.Job.Actions.RemoveAnyQueues, Claims.Job.Actions.RemoveOwnQueues)]
        public virtual ActionResult RemoveJob(int id, string Comment, bool? CloseJob = null)
        {
            Database.Configuration.LazyLoadingEnabled = true;

            var jobQueueJob = Database.JobQueueJobs.Find(id);
            if (jobQueueJob == null)
                throw new ArgumentException("Invalid Job Queue Job Id", "id");

            if (!jobQueueJob.CanRemove())
                throw new InvalidOperationException("Removing job from queue is Denied");

            var job = Database.Jobs.Find(jobQueueJob.JobId);
            if (job == null)
                throw new ArgumentException("Invalid Job Id", "JobId");

            jobQueueJob.OnRemove(CurrentUser, Comment);
            Database.SaveChanges();

            if (CloseJob.HasValue && CloseJob.Value && job.CanCloseNormally())
            {
                job.OnCloseNormally(Database, Database.Users.Find(CurrentUser.UserId));
                Database.SaveChanges();
            }

            return Redirect($"{Url.Action(MVC.Job.Show(job.Id))}#jobDetailTab-Queues");
        }

        #endregion

    }
}