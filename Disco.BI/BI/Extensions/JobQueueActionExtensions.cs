using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Jobs.JobQueues;
using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.BI.Extensions
{
    public static class JobQueueActionExtensions
    {

        #region Edit Sla
        public static bool CanEditSla(this JobQueueJob jqj)
        {
            if (jqj.RemovedDate.HasValue)
                return false;

            if (UserService.CurrentAuthorization.Has(Claims.Job.Properties.JobQueueProperties.EditAnySLA))
            {
                // Can edit ANY queue
                return true;
            }
            else if (UserService.CurrentAuthorization.Has(Claims.Job.Properties.JobQueueProperties.EditOwnSLA))
            {
                // Can edit from OWN queue
                return JobQueueService.UsersQueues(UserService.CurrentUser).Any(q => q.JobQueue.Id == jqj.JobQueueId);
            }
            else
            {
                return false;
            }
        }
        public static void OnEditSla(this JobQueueJob jqj, DateTime? SlaExpiresDate)
        {
            if (!jqj.CanEditSla())
                throw new InvalidOperationException("Editing job SLA for this queue is denied");

            if (SlaExpiresDate.HasValue && jqj.AddedDate > SlaExpiresDate.Value)
                throw new ArgumentException("The SLA Expires Date must be greater than the Added Date", "SLAExpiresDate");

            jqj.SLAExpiresDate = SlaExpiresDate;
        } 
        #endregion

        #region Edit Priority
        public static bool CanEditPriority(this JobQueueJob jqj)
        {
            if (jqj.RemovedDate.HasValue)
                return false;

            if (UserService.CurrentAuthorization.Has(Claims.Job.Properties.JobQueueProperties.EditAnyPriority))
            {
                // Can edit ANY queue
                return true;
            }
            else if (UserService.CurrentAuthorization.Has(Claims.Job.Properties.JobQueueProperties.EditOwnPriority))
            {
                // Can edit from OWN queue
                return JobQueueService.UsersQueues(UserService.CurrentUser).Any(q => q.JobQueue.Id == jqj.JobQueueId);
            }
            else
            {
                return false;
            }
        }
        public static void OnEditPriority(this JobQueueJob jqj, JobQueuePriority Priority)
        {
            if (!jqj.CanEditPriority())
                throw new InvalidOperationException("Editing job priority for this queue is denied");

            jqj.Priority = Priority;
        }
        #endregion

        #region Edit Comments
        private static bool CanEditComments(this JobQueueJob jqj)
        {
            if (UserService.CurrentAuthorization.Has(Claims.Job.Properties.JobQueueProperties.EditAnyComments))
            {
                // Can edit ANY queue
                return true;
            }
            else if (UserService.CurrentAuthorization.Has(Claims.Job.Properties.JobQueueProperties.EditOwnComments))
            {
                // Can edit from OWN queue
                return JobQueueService.UsersQueues(UserService.CurrentUser).Any(q => q.JobQueue.Id == jqj.JobQueueId);
            }
            else
            {
                return false;
            }
        }
        public static bool CanEditAddedComment(this JobQueueJob jqj)
        {
            return jqj.CanEditComments();
        }
        public static bool CanEditRemovedComment(this JobQueueJob jqj)
        {
            if (!jqj.RemovedDate.HasValue)
                return false;

            return jqj.CanEditComments();
        }
        public static void OnEditAddedComment(this JobQueueJob jqj, string AddedComment)
        {
            if (!jqj.CanEditAddedComment())
                throw new InvalidOperationException("Editing job added comments for this queue is denied");

            jqj.AddedComment = string.IsNullOrWhiteSpace(AddedComment) ? null : AddedComment.Trim();
        }
        public static void OnEditRemovedComment(this JobQueueJob jqj, string RemovedComment)
        {
            if (!jqj.CanEditRemovedComment())
                throw new InvalidOperationException("Editing job removed comments for this queue is denied");

            jqj.RemovedComment = string.IsNullOrWhiteSpace(RemovedComment) ? null : RemovedComment.Trim();
        }
        #endregion

        #region Remove
        public static bool CanRemove(this JobQueueJob jqj)
        {
            if (jqj.RemovedDate.HasValue)
                return false;

            if (UserService.CurrentAuthorization.Has(Claims.Job.Actions.RemoveAnyQueues))
            {
                // Can remove from ANY queue
                return true;
            }
            else if (UserService.CurrentAuthorization.Has(Claims.Job.Actions.RemoveOwnQueues))
            {
                // Can remove from OWN queue
                return JobQueueService.UsersQueues(UserService.CurrentUser).Any(q => q.JobQueue.Id == jqj.JobQueueId);
            }
            else
            {
                return false;
            }
        }
        public static void OnRemove(this JobQueueJob jqj, User Technician, string Comment)
        {
            if (!jqj.CanRemove())
                throw new InvalidOperationException("Removing job from queue is Denied");

            jqj.RemovedDate = DateTime.Now;
            jqj.RemovedUserId = Technician.UserId;
            jqj.RemovedComment = string.IsNullOrWhiteSpace(Comment) ? null : Comment.Trim();
        }
        #endregion

        #region Add
        public static bool CanAddQueues(this Job j)
        {
            // Job Closed?
            if (j.ClosedDate.HasValue)
                return false;

            if (UserService.CurrentAuthorization.HasAny(Claims.Job.Actions.AddAnyQueues, Claims.Job.Actions.AddOwnQueues))
                return true;

            return false;
        }
        public static bool CanAddQueue(this Job j, JobQueue jq)
        {
            // Shortcut
            if (!j.CanAddQueues())
                return false;

            // Already in Queue?
            if (j.JobQueues.Count(jjq => !jjq.RemovedDate.HasValue && jjq.JobQueueId == jq.Id) > 0)
                return false;

            // Can add ANY queue
            if (UserService.CurrentAuthorization.Has(Claims.Job.Actions.AddAnyQueues))
                return true;

            // Can add OWN queue
            if (UserService.CurrentAuthorization.Has(Claims.Job.Actions.AddOwnQueues))
            {
                return JobQueueService.UsersQueues(UserService.CurrentUser).Any(q => q.JobQueue.Id == jq.Id);
            }

            return false;
        }
        public static JobQueueJob OnAddQueue(this Job j, DiscoDataContext Database, JobQueue jq, User Technician, string Comment, DateTime? SLAExpires, JobQueuePriority Priority)
        {
            if (!j.CanAddQueue(jq))
                throw new InvalidOperationException("Adding job to queue is Denied");

            if (SLAExpires.HasValue && SLAExpires.Value < DateTime.Now)
                throw new ArgumentException("The SLA Date must be greater than the current time", "SLAExpires");

            var jqj = new JobQueueJob()
            {
                JobQueueId = jq.Id,
                JobId = j.Id,
                AddedDate = DateTime.Now,
                AddedUserId = Technician.UserId,
                AddedComment = string.IsNullOrWhiteSpace(Comment) ? null : Comment.Trim(),
                SLAExpiresDate = SLAExpires,
                Priority = Priority
            };

            Database.JobQueueJobs.Add(jqj);
            return jqj;
        }
        #endregion

    }
}
