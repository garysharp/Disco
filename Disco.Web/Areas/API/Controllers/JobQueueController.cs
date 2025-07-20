using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Jobs.JobQueues;
using Disco.Services.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class JobQueueController : AuthorizedDatabaseController
    {
        const string pName = "name";
        const string pDescription = "description";
        const string pIcon = "icon";
        const string pIconColour = "iconcolour";
        const string pPriority = "priority";
        const string pDefaultSLAExpiry = "defaultslaexpiry";

        [DiscoAuthorize(Claims.Config.JobQueue.Configure)]
        public virtual ActionResult Update(int id, string key, string value = null, bool? redirect = null)
        {
            Authorization.Require(Claims.Config.JobQueue.Configure);

            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");
                var jobQueue = Database.JobQueues.Find(id);
                if (jobQueue != null)
                {
                    switch (key.ToLower())
                    {
                        case pName:
                            UpdateName(jobQueue, value);
                            break;
                        case pDescription:
                            UpdateDescription(jobQueue, value);
                            break;
                        case pPriority:
                            UpdatePriority(jobQueue, value);
                            break;
                        case pIcon:
                            UpdateIcon(jobQueue, value);
                            break;
                        case pIconColour:
                            UpdateIconColour(jobQueue, value);
                            break;
                        case pDefaultSLAExpiry:
                            UpdateDefaultSLAExpiry(jobQueue, value);
                            break;
                        default:
                            throw new Exception("Invalid Update Key");
                    }
                }
                else
                {
                    throw new Exception("Invalid Job Queue Id");
                }
                if (redirect.HasValue && redirect.Value)
                    return RedirectToAction(MVC.Config.JobQueue.Index(jobQueue.Id));
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
        [DiscoAuthorize(Claims.Config.JobQueue.Configure)]
        public virtual ActionResult UpdateName(int id, string QueueName = null, bool? redirect = null)
        {
            return Update(id, pName, QueueName, redirect);
        }

        [DiscoAuthorize(Claims.Config.JobQueue.Configure)]
        public virtual ActionResult UpdateDescription(int id, string Description = null, bool? redirect = null)
        {
            return Update(id, pDescription, Description, redirect);
        }

        [DiscoAuthorize(Claims.Config.JobQueue.Configure)]
        public virtual ActionResult UpdatePriority(int id, string Priority = null, bool? redirect = null)
        {
            return Update(id, pPriority, Priority, redirect);
        }

        [DiscoAuthorize(Claims.Config.JobQueue.Configure)]
        public virtual ActionResult UpdateDefaultSLAExpiry(int id, string DefaultSLAExpiry = null, bool? redirect = null)
        {
            return Update(id, pDefaultSLAExpiry, DefaultSLAExpiry, redirect);
        }

        [DiscoAuthorize(Claims.Config.JobQueue.Configure)]
        public virtual ActionResult UpdateIcon(int id, string Icon = null, bool? redirect = null)
        {
            return Update(id, pIcon, Icon, redirect);
        }

        [DiscoAuthorize(Claims.Config.JobQueue.Configure)]
        public virtual ActionResult UpdateIconColour(int id, string IconColour = null, bool? redirect = null)
        {
            return Update(id, pIconColour, IconColour, redirect);
        }

        [DiscoAuthorize(Claims.Config.JobQueue.Configure)]
        public virtual ActionResult UpdateIconAndColour(int id, string Icon = null, string IconColour = null, bool redirect = false)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");

                var jobQueue = Database.JobQueues.Find(id);
                if (jobQueue != null)
                {
                    UpdateIconAndColour(jobQueue, Icon, IconColour);
                }
                else
                {
                    return Json("Invalid Job Queue Id", JsonRequestBehavior.AllowGet);
                }
                if (redirect)
                    return RedirectToAction(MVC.Config.JobQueue.Index(jobQueue.Id));
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

        [DiscoAuthorize(Claims.Config.JobQueue.Configure)]
        public virtual ActionResult UpdateSubjects(int id, string[] Subjects = null, bool redirect = false)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");

                var jobQueue = Database.JobQueues.Find(id);
                if (jobQueue != null)
                {
                    UpdateSubjects(jobQueue, Subjects);
                }
                else
                {
                    return Json("Invalid Job Queue Id", JsonRequestBehavior.AllowGet);
                }
                if (redirect)
                    return RedirectToAction(MVC.Config.JobQueue.Index(jobQueue.Id));
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

        [DiscoAuthorize(Claims.Config.JobQueue.Configure)]
        public virtual ActionResult UpdateJobSubTypes(int id, List<string> JobSubTypes = null, bool redirect = false)
        {
            try
            {
                var jobQueue = Database.JobQueues.Find(id);
                if (jobQueue != null)
                {
                    UpdateJobSubTypes(jobQueue, JobSubTypes);
                }
                else
                {
                    return Json("Invalid Job Queue Id", JsonRequestBehavior.AllowGet);
                }

                if (redirect)
                    return RedirectToAction(MVC.Config.JobQueue.Index(jobQueue.Id));
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
        private void UpdateIconAndColour(JobQueue jobQueue, string Icon, string IconColour)
        {
            if (string.IsNullOrWhiteSpace(Icon))
                throw new ArgumentNullException("Icon");
            if (string.IsNullOrWhiteSpace(IconColour))
                throw new ArgumentNullException("IconColour");

            jobQueue.Icon = Icon;
            jobQueue.IconColour = IconColour;
            JobQueueService.UpdateJobQueue(Database, jobQueue);
        }
        private void UpdateIcon(JobQueue jobQueue, string Icon)
        {
            if (string.IsNullOrWhiteSpace(Icon))
                throw new ArgumentNullException("Icon");

            jobQueue.Icon = Icon;
            JobQueueService.UpdateJobQueue(Database, jobQueue);
        }
        private void UpdateIconColour(JobQueue jobQueue, string IconColour)
        {
            if (string.IsNullOrWhiteSpace(IconColour))
                throw new ArgumentNullException("IconColour");

            jobQueue.IconColour = IconColour;
            JobQueueService.UpdateJobQueue(Database, jobQueue);
        }

        private void UpdateName(JobQueue jobQueue, string Name)
        {
            jobQueue.Name = Name;
            JobQueueService.UpdateJobQueue(Database, jobQueue);
        }

        private void UpdateDescription(JobQueue jobQueue, string Description)
        {
            jobQueue.Description = Description;
            JobQueueService.UpdateJobQueue(Database, jobQueue);
        }

        private void UpdatePriority(JobQueue jobQueue, string Priority)
        {

            if (!Enum.TryParse<JobQueuePriority>(Priority, out var priority))
                throw new ArgumentException("Invalid Priority Value", "Priority");

            jobQueue.Priority = priority;
            JobQueueService.UpdateJobQueue(Database, jobQueue);
        }

        private void UpdateDefaultSLAExpiry(JobQueue jobQueue, string DefaultSLAExpiry)
        {
            int? defaultSLAExpiry = null;

            if (!string.IsNullOrEmpty(DefaultSLAExpiry))
            {

                if (!int.TryParse(DefaultSLAExpiry, out var intValue))
                    throw new ArgumentException("Invalid Default SLA Expiry Value", "DefaultSLAPriority");

                if (intValue < 0)
                    throw new ArgumentException("Default SLA Expiry Value must be greater than zero", "DefaultSLAPriority");

                // if intValue == 0, then no SLA.
                if (intValue > 0)
                    defaultSLAExpiry = intValue;
            }

            jobQueue.DefaultSLAExpiry = defaultSLAExpiry;
            JobQueueService.UpdateJobQueue(Database, jobQueue);
        }

        private void UpdateSubjects(JobQueue jobQueue, string[] Subjects)
        {
            string subjectIds = null;

            // Validate Subjects
            if (Subjects != null && Subjects.Length > 0)
            {
                var subjects = Subjects
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .Select(s => Tuple.Create(s, ActiveDirectory.RetrieveADObject(s, Quick: true)))
                    .Where(s => s.Item2 is ADUserAccount || s.Item2 is ADGroup)
                    .ToList();
                var invalidSubjects = subjects.Where(s => s.Item2 == null).ToList();

                if (invalidSubjects.Count > 0)
                    throw new ArgumentException($"Subjects not found: {string.Join(", ", invalidSubjects)}", "Subjects");

                var proposedSubjects = subjects.Select(s => s.Item2.Id).OrderBy(s => s).ToArray();

                subjectIds = string.Join(",", proposedSubjects);

                if (string.IsNullOrEmpty(subjectIds))
                    subjectIds = null;
            }

            if (jobQueue.SubjectIds != subjectIds)
            {
                jobQueue.SubjectIds = subjectIds;
                JobQueueService.UpdateJobQueue(Database, jobQueue);
            }
        }

        private void UpdateJobSubTypes(JobQueue jobQueue, List<string> JobSubTypes)
        {
            Database.Configuration.LazyLoadingEnabled = true;

            // Remove All Existing
            if (jobQueue.JobSubTypes != null)
            {
                foreach (var st in jobQueue.JobSubTypes.ToArray())
                    jobQueue.JobSubTypes.Remove(st);
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
                jobQueue.JobSubTypes = subTypes;
            }
            Database.SaveChanges();
        }
        #endregion

        #region Actions
        [DiscoAuthorize(Claims.Config.JobQueue.Delete)]
        public virtual ActionResult Delete(int id, bool? redirect = false)
        {
            try
            {
                var jq = Database.JobQueues.Find(id);
                if (jq != null)
                {
                    var status = JobQueueDeleteTask.ScheduleNow(jq.Id);
                    status.SetFinishedUrl(Url.Action(MVC.Config.JobQueue.Index(null)));

                    if (redirect.HasValue && redirect.Value)
                        return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
                    else
                        return Json("OK", JsonRequestBehavior.AllowGet);
                }
                throw new Exception("Invalid Job Queue Id");
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
