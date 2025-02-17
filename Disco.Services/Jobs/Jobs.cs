﻿using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.UI.Job;
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
            return ExpressionCache.GetOrCreateSingleExpressions("Job_OnCreateExpression", () => Expression.TokenizeSingleDynamic(null, Database.DiscoConfiguration.JobPreferences.OnCreateExpression, 0));
        }

        public static void OnCreateExpressionInvalidateCache()
        {
            ExpressionCache.InvalidateSingleCache("Job_OnCreateExpression");
        }

        public static Expression OnDeviceReadyForReturnExpressionFromCache(DiscoDataContext Database)
        {
            return ExpressionCache.GetOrCreateSingleExpressions("Job_OnDeviceReadyForReturnExpression", () => Expression.TokenizeSingleDynamic(null, Database.DiscoConfiguration.JobPreferences.OnDeviceReadyForReturnExpression, 0));
        }

        public static void OnDeviceReadyForReturnExpressionInvalidateCache()
        {
            ExpressionCache.InvalidateSingleCache("Job_OnDeviceReadyForReturnExpression");
        }

        public static Expression OnCloseExpressionFromCache(DiscoDataContext Database)
        {
            return ExpressionCache.GetOrCreateSingleExpressions("Job_OnCloseExpression", () => Expression.TokenizeSingleDynamic(null, Database.DiscoConfiguration.JobPreferences.OnCloseExpression, 0));
        }

        public static void OnCloseExpressionInvalidateCache()
        {
            ExpressionCache.InvalidateSingleCache("Job_OnCloseExpression");
        }

        public static Expression InitialCommentsTemplateFromCache(DiscoDataContext database)
        {
            return ExpressionCache.GetOrCreateSingleExpressions("Job_InitialCommentsTemplate", () => Expression.Tokenize(null, database.DiscoConfiguration.JobPreferences.InitialCommentsTemplate ?? string.Empty, 0, false, false));
        }

        public static void InitialCommentsTemplateInvalidateCache()
        {
            ExpressionCache.InvalidateSingleCache("Job_InitialCommentsTemplate");
        }

        public static string GenerateInitialComments(DiscoDataContext database, JobCreateModel createModel, User techUser, out bool isTypeDynamic)
        {
            var type = createModel.JobTypes.First(t => t.Id == createModel.Type);
            var subTypes = default(List<string>);
            if (createModel.SubTypes != null && createModel.SubTypes.Count > 0)
                subTypes = type.JobSubTypes.Where(s => createModel.SubTypes.Contains(s.Id)).Select(s => s.Description).ToList();
            else
                subTypes = new List<string>();

            return GenerateInitialComments(database, createModel.Device, createModel.User, type.Description, subTypes, techUser, out isTypeDynamic);
        }

        public static string GenerateInitialComments(DiscoDataContext database, Device device, User user, string jobTypeDescription, List<string> jobSubTypeDescriptions, User techUser, out bool isTypeDynamic)
        {
            var expression = InitialCommentsTemplateFromCache(database);

            IDictionary evaluatorVariables = Expression.StandardVariables(null, database, user, DateTime.Now, null, (IAttachmentTarget)user ?? device);

            evaluatorVariables["TechUser"] = techUser;
            // User is part of the StandardVariables
            evaluatorVariables["Device"] = device;
            evaluatorVariables["JobType"] = jobTypeDescription;
            evaluatorVariables["JobSubTypes"] = jobSubTypeDescriptions;

            var result = expression.Evaluate(techUser, evaluatorVariables);

            isTypeDynamic = expression.Source.IndexOf("#JobType", StringComparison.OrdinalIgnoreCase) >= 0 ||
                expression.Source.IndexOf("#JobSubTypes", StringComparison.OrdinalIgnoreCase) >= 0;

            return result.Item1;
        }

    }
}
