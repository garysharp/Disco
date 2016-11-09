using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disco.Services.Jobs
{
    public static class Jobs
    {

        public static Job Create(DiscoDataContext Database, Device device, User user, JobType type, List<JobSubType> subTypes, User initialTech, bool addAutoQueues = true)
        {
            Job j = new Job()
            {
                JobType = type,
                OpenedTechUserId = initialTech.UserId,
                OpenedTechUser = initialTech,
                OpenedDate = DateTime.Now
            };

            // Device
            if (device != null)
            {
                j.Device = device;
                j.DeviceSerialNumber = device.SerialNumber;
            }

            // User
            if (user != null)
            {
                j.User = user;
                j.UserId = user.UserId;
            }

            // Sub Types
            List<JobSubType> jobSubTypes = subTypes.ToList();
            j.JobSubTypes = jobSubTypes;

            Database.Jobs.Add(j);

            // Job Queues
            if (addAutoQueues)
            {
                var queues = from st in subTypes
                             from jq in st.JobQueues
                             group st by jq into g
                             select new { queue = g.Key, subTypes = g };
                foreach (var queue in queues)
                {
                    var commentBuilder = new StringBuilder("Automatically added by:").AppendLine();
                    foreach (var subType in queue.subTypes)
                    {
                        commentBuilder.AppendLine().Append("* ").Append(subType.Description);
                    }

                    var jqj = new JobQueueJob()
                    {
                        JobQueueId = queue.queue.Id,
                        Job = j,
                        AddedDate = DateTime.Now,
                        AddedUserId = initialTech.UserId,
                        AddedComment = commentBuilder.ToString(),
                        SLAExpiresDate = queue.queue.DefaultSLAExpiry.HasValue ? (DateTime?)DateTime.Now.AddMinutes(queue.queue.DefaultSLAExpiry.Value) : null,
                        Priority = JobQueuePriority.Normal
                    };

                    Database.JobQueueJobs.Add(jqj);
                }
            }

            switch (type.Id)
            {
                case JobType.JobTypeIds.HWar:
                    Database.JobMetaWarranties.Add(new JobMetaWarranty() { Job = j });
                    break;
                case JobType.JobTypeIds.HNWar:
                    Database.JobMetaNonWarranties.Add(new JobMetaNonWarranty() { Job = j });
                    if (device != null)
                    {
                        // Add Job Components
                        var components = Database.DeviceComponents.Include("JobSubTypes").Where(c => !c.DeviceModelId.HasValue || c.DeviceModelId == j.Device.DeviceModelId);
                        var addedComponents = new List<DeviceComponent>();
                        foreach (var c in components)
                        {
                            if (c.JobSubTypes.Count == 0)
                            { // No Filter
                                addedComponents.Add(c);
                            }
                            else
                            {
                                foreach (var st in c.JobSubTypes)
                                {
                                    foreach (var jst in jobSubTypes)
                                    {
                                        if (st.JobTypeId == jst.JobTypeId && st.Id == jst.Id)
                                        {
                                            addedComponents.Add(c);
                                            break;
                                        }
                                    }
                                    if (addedComponents.Contains(c))
                                        break;
                                }
                            }
                        }
                        foreach (var c in addedComponents)
                            Database.JobComponents.Add(new JobComponent()
                            {
                                Job = j,
                                TechUserId = initialTech.UserId,
                                Cost = c.Cost,
                                Description = c.Description
                            });
                    }
                    break;
            }

            return j;
        }

        public static Expression OnCreateExpressionFromCache(DiscoDataContext Database)
        {
            return ExpressionCache.GetValue("Job_OnCreateExpression", string.Empty, () => { return Expression.TokenizeSingleDynamic(null, Database.DiscoConfiguration.JobPreferences.OnCreateExpression, 0); });
        }

        public static void OnCreateExpressionInvalidateCache()
        {
            ExpressionCache.InvalidateKey("Job_OnCreateExpression", string.Empty);
        }

        public static Expression OnCloseExpressionFromCache(DiscoDataContext Database)
        {
            return ExpressionCache.GetValue("Job_OnCloseExpression", string.Empty, () => { return Expression.TokenizeSingleDynamic(null, Database.DiscoConfiguration.JobPreferences.OnCloseExpression, 0); });
        }

        public static void OnCloseExpressionInvalidateCache()
        {
            ExpressionCache.InvalidateKey("Job_OnCloseExpression", string.Empty);
        }

    }
}
