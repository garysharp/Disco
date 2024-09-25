﻿using Disco.Data.Repository;
using Disco.Models.BI.Config;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Logging;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.InsuranceProvider;
using Disco.Services.Plugins.Features.RepairProvider;
using Disco.Services.Plugins.Features.WarrantyProvider;
using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using DiscoServicesJobs = Disco.Services.Interop.DiscoServices.Jobs;
using PublishJobResult = Disco.Models.Services.Interop.DiscoServices.PublishJobResult;

namespace Disco.Services
{
    public static class JobActionExtensions
    {

        #region Create
        public static bool CanCreate()
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Job.Actions.Create))
                return false;

            if (!UserService.CurrentAuthorization.HasAny(Claims.Job.Types.CreateHMisc, Claims.Job.Types.CreateHNWar, Claims.Job.Types.CreateHWar, Claims.Job.Types.CreateSApp, Claims.Job.Types.CreateSImg, Claims.Job.Types.CreateSOS, Claims.Job.Types.CreateUMgmt))
                return false;

            return true;
        }
        #endregion

        #region Device Held
        public static bool CanDeviceHeld(this Job j)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Job.Properties.DeviceHeld))
                return false;

            return (!j.ClosedDate.HasValue) && (j.DeviceSerialNumber != null) &&
                (!j.DeviceHeld.HasValue || j.DeviceReturnedDate.HasValue);
        }
        public static void OnDeviceHeld(this Job j, User Technician)
        {
            if (!j.CanDeviceHeld())
                throw new InvalidOperationException("Holding Device was Denied");

            j.DeviceHeld = DateTime.Now;
            j.DeviceHeldTechUserId = Technician.UserId;
            j.DeviceReadyForReturn = null;
            j.DeviceReadyForReturnTechUserId = null;
            j.DeviceReturnedDate = null;
            j.DeviceReturnedTechUserId = null;
        }
        #endregion

        #region Device Ready for Return
        public static bool CanDeviceReadyForReturn(this Job j)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Job.Properties.DeviceReadyForReturn))
                return false;

            return (!j.ClosedDate.HasValue) && j.DeviceHeld.HasValue &&
                !j.DeviceReadyForReturn.HasValue && !j.DeviceReturnedDate.HasValue;
        }
        public static void OnDeviceReadyForReturn(this Job j, DiscoDataContext Database, User Technician)
        {
            if (!j.CanDeviceReadyForReturn())
                throw new InvalidOperationException("Device Ready for Return was Denied");

            j.DeviceReadyForReturn = DateTime.Now;
            j.DeviceReadyForReturnTechUserId = Technician.UserId;

            // Evaluate OnDeviceReadyForReturnExpression Expression
            try
            {
                var result = j.EvaluateOnDeviceReadyForReturnExpression(Database);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    var jl = new JobLog()
                    {
                        Job = j,
                        TechUser = Technician,
                        Timestamp = DateTime.Now,
                        Comments = result
                    };
                    Database.JobLogs.Add(jl);
                }
            }
            catch (Exception ex)
            {
                SystemLog.LogException("Job Expression - OnDeviceReadyForReturnExpression", ex);
            }
        }
        #endregion

        #region Device Returned
        public static bool CanDeviceReturned(this Job j)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Job.Properties.DeviceReturned))
                return false;

            return (!j.ClosedDate.HasValue) && j.DeviceHeld.HasValue &&
                !j.DeviceReturnedDate.HasValue;
        }
        public static void OnDeviceReturned(this Job j, User Technician)
        {
            if (!j.CanDeviceReturned())
                throw new InvalidOperationException("Device Return was Denied");

            j.DeviceReturnedDate = DateTime.Now;
            j.DeviceReturnedTechUserId = Technician.UserId;
        }
        #endregion

        #region Waiting For User Action
        public static bool CanWaitingForUserAction(this Job j)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Job.Properties.WaitingForUserAction))
                return false;

            return !j.ClosedDate.HasValue && (j.UserId != null) && !j.WaitingForUserAction.HasValue;
        }
        public static void OnWaitingForUserAction(this Job j, DiscoDataContext Database, User Technician, string Reason)
        {
            if (!j.CanWaitingForUserAction())
                throw new InvalidOperationException("Waiting for User Action was Denied");

            j.WaitingForUserAction = DateTime.Now;

            // Write Log
            JobLog jobLog = new JobLog()
            {
                JobId = j.Id,
                TechUserId = Technician.UserId,
                Timestamp = DateTime.Now,
                Comments = string.Format("# Waiting on User Action\r\n{0}", string.IsNullOrWhiteSpace(Reason) ? "<no reason provided>" : Reason)
            };
            Database.JobLogs.Add(jobLog);
        }
        #endregion

        #region Not Waiting For User Action
        public static bool CanNotWaitingForUserAction(this Job j)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Job.Properties.NotWaitingForUserAction))
                return false;

            return j.WaitingForUserAction.HasValue;
        }
        public static void OnNotWaitingForUserAction(this Job j, DiscoDataContext Database, User Technician, string Resolution)
        {
            if (!j.CanNotWaitingForUserAction())
                throw new InvalidOperationException("Not Waiting for User Action was Denied");

            j.WaitingForUserAction = null;

            // Write Log
            JobLog jobLog = new JobLog()
            {
                JobId = j.Id,
                TechUserId = Technician.UserId,
                Timestamp = DateTime.Now,
                Comments = string.Format("# User Action Resolved\r\n{0}", string.IsNullOrWhiteSpace(Resolution) ? "<no comment provided>" : Resolution)
            };
            Database.JobLogs.Add(jobLog);
        }
        #endregion

        #region Log Warranty
        public static bool CanLogWarranty(this Job j)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Job.Actions.LogWarranty))
                return false;

            return !j.ClosedDate.HasValue &&
                (j.DeviceSerialNumber != null) &&
                j.JobTypeId == JobType.JobTypeIds.HWar &&
                !j.JobMetaWarranty.ExternalLoggedDate.HasValue;
        }
        public static void OnLogWarranty(this Job j, DiscoDataContext Database, string FaultDescription, List<JobAttachment> SendAttachments, PluginFeatureManifest WarrantyProviderDefinition, OrganisationAddress Address, User TechUser, Dictionary<string, string> WarrantyProviderProperties)
        {
            if (!j.CanLogWarranty())
                throw new InvalidOperationException("Lodge Warranty was Denied");

            PublishJobResult publishJobResult = null;

            using (WarrantyProviderFeature warrantyProvider = WarrantyProviderDefinition.CreateInstance<WarrantyProviderFeature>())
            {
                var warrantyProvider2 = warrantyProvider as WarrantyProvider2Feature;

                if (warrantyProvider2 == null && SendAttachments != null && SendAttachments.Count > 0)
                {
                    publishJobResult = DiscoServicesJobs.Publish(
                        Database,
                        j,
                        TechUser,
                        warrantyProvider.WarrantyProviderId,
                        null,
                        FaultDescription,
                        SendAttachments,
                        AttachmentDataStoreExtensions.RepositoryFilename);

                    if (!publishJobResult.Success)
                        throw new Exception(string.Format("Disco ICT Online Services failed with the following message: ", publishJobResult.ErrorMessage));

                    if (string.IsNullOrWhiteSpace(FaultDescription))
                        FaultDescription = publishJobResult.PublishMessage;
                    else
                        FaultDescription = string.Concat(FaultDescription, Environment.NewLine, "___", Environment.NewLine, publishJobResult.PublishMessage);
                }

                string submitDescription;

                if (string.IsNullOrWhiteSpace(FaultDescription))
                    submitDescription = j.GenerateFaultDescriptionFooter(Database, WarrantyProviderDefinition);
                else
                    submitDescription = string.Concat(FaultDescription, Environment.NewLine, Environment.NewLine, j.GenerateFaultDescriptionFooter(Database, WarrantyProviderDefinition));

                string providerRef;
                if (warrantyProvider2 != null)
                    providerRef = warrantyProvider2.SubmitJob(Database, j, Address, TechUser, FaultDescription, SendAttachments, WarrantyProviderProperties);
                else
                    providerRef = warrantyProvider.SubmitJob(Database, j, Address, TechUser, submitDescription, WarrantyProviderProperties);

                j.JobMetaWarranty.ExternalLoggedDate = DateTime.Now;
                j.JobMetaWarranty.ExternalName = warrantyProvider.WarrantyProviderId;

                if (providerRef != null && providerRef.Length > 100)
                    j.JobMetaWarranty.ExternalReference = providerRef.Substring(0, 100);
                else
                    j.JobMetaWarranty.ExternalReference = providerRef;

                // Write Log
                JobLog jobLog = new JobLog()
                {
                    JobId = j.Id,
                    TechUserId = TechUser.UserId,
                    Timestamp = DateTime.Now,
                    Comments = string.Format("# Warranty Claim Submitted\r\nProvider: **{0}**\r\nAddress: **{1}**\r\nReference: **{2}**\r\n___\r\n```{3}```", warrantyProvider.Manifest.Name, Address.Name, providerRef, FaultDescription)
                };
                Database.JobLogs.Add(jobLog);

                if (publishJobResult != null)
                {
                    try
                    {
                        DiscoServicesJobs.UpdateRecipientReference(Database, j, publishJobResult.Id, publishJobResult.Secret, j.JobMetaWarranty.ExternalReference);
                    }
                    catch (Exception) { } // Ignore Errors as this is not completely necessary
                }
            }
        }
        public static void OnLogWarranty(this Job j, DiscoDataContext Database, string FaultDescription, string ManualProviderName, string ManualProviderReference, OrganisationAddress Address, User TechUser)
        {
            if (!j.CanLogWarranty())
                throw new InvalidOperationException("Lodge Warranty was Denied");

            j.JobMetaWarranty.ExternalLoggedDate = DateTime.Now;
            j.JobMetaWarranty.ExternalName = ManualProviderName;

            if (ManualProviderReference != null && ManualProviderReference.Length > 100)
                j.JobMetaWarranty.ExternalReference = ManualProviderReference.Substring(0, 100);
            else
                j.JobMetaWarranty.ExternalReference = ManualProviderReference;

            // Write Log
            JobLog jobLog = new JobLog()
            {
                JobId = j.Id,
                TechUserId = TechUser.UserId,
                Timestamp = DateTime.Now,
                Comments = string.Format("# Manual Warranty Claim Submitted\r\nProvider: **{0}**\r\nAddress: **{1}**\r\nReference: **{2}**\r\n___\r\n```{3}```", ManualProviderName, Address.Name, ManualProviderReference ?? "<none>", FaultDescription)
            };
            Database.JobLogs.Add(jobLog);
        }
        #endregion

        #region Convert HWar to HNWar
        public static bool CanConvertHWarToHNWar(this Job j)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Job.Actions.ConvertHWarToHNWar))
                return false;

            return !j.ClosedDate.HasValue && (j.DeviceSerialNumber != null) &&
                j.JobTypeId == JobType.JobTypeIds.HWar && string.IsNullOrEmpty(j.JobMetaWarranty.ExternalReference);
        }
        public static void OnConvertHWarToHNWar(this Job j, DiscoDataContext Database)
        {
            if (!j.CanConvertHWarToHNWar())
                throw new InvalidOperationException("Convert HWar to HNWar was Denied");

            var techUser = UserService.CurrentUser;

            // Remove JobMetaWarranty
            if (j.JobMetaWarranty != null)
                Database.JobMetaWarranties.Remove(j.JobMetaWarranty);

            // Add JobMetaNonWarranty
            var metaHNWar = new JobMetaNonWarranty() { Job = j };
            Database.JobMetaNonWarranties.Add(metaHNWar);

            // Swap Job Sub Types
            List<string> jobSubTypes = j.JobSubTypes.Select(jst => jst.Id).ToList();
            j.JobSubTypes.Clear();
            foreach (var jst in Database.JobSubTypes.Where(i => i.JobTypeId == JobType.JobTypeIds.HNWar && jobSubTypes.Contains(i.Id)))
                j.JobSubTypes.Add(jst);

            // Add Components
            var components = Database.DeviceComponents.Include("JobSubTypes").Where(c => !c.DeviceModelId.HasValue || c.DeviceModelId == j.Device.DeviceModelId);
            var jobComponents = new List<DeviceComponent>();
            foreach (var component in components)
            {
                if (component.JobSubTypes.Count == 0)
                {
                    jobComponents.Add(component);
                }
                else
                {
                    foreach (var st in component.JobSubTypes)
                    {
                        foreach (var jst in j.JobSubTypes)
                        {
                            if (st.JobTypeId == jst.JobTypeId && st.Id == jst.Id)
                            {
                                jobComponents.Add(component);
                                break;
                            }
                        }
                        if (jobComponents.Contains(component))
                            break;
                    }
                }
            }
            foreach (var component in jobComponents)
            {
                Database.JobComponents.Add(new JobComponent()
                {
                    Job = j,
                    TechUserId = techUser.UserId,
                    Cost = component.Cost,
                    Description = component.Description
                });
            }

            // Write Log
            JobLog jobLog = new JobLog()
            {
                JobId = j.Id,
                TechUserId = techUser.UserId,
                Timestamp = DateTime.Now,
                Comments = string.Format("# Job Type Converted\r\nFrom: **{0}**\r\nTo: **{1}**", Database.JobTypes.Find(JobType.JobTypeIds.HWar), Database.JobTypes.Find(JobType.JobTypeIds.HNWar))
            };
            Database.JobLogs.Add(jobLog);

            j.JobTypeId = JobType.JobTypeIds.HNWar;
        }
        #endregion

        #region Warranty Completed
        public static bool CanWarrantyCompleted(this Job j)
        {
            return (j.JobTypeId == JobType.JobTypeIds.HWar) &&
                j.JobMetaWarranty.ExternalLoggedDate.HasValue &&
                !j.JobMetaWarranty.ExternalCompletedDate.HasValue;
        }
        public static void OnWarrantyCompleted(this Job j)
        {
            if (!j.CanWarrantyCompleted())
                throw new InvalidOperationException("Warranty Completed was Denied");

            j.JobMetaWarranty.ExternalCompletedDate = DateTime.Now;
        }
        #endregion

        #region Insurance Claim Form Sent
        [Obsolete("Use Log Insurance instead")]
        public static bool CanInsuranceClaimFormSent(this Job j)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Job.Properties.NonWarrantyProperties.InsuranceClaimFormSent))
                return false;

            return (j.JobTypeId == JobType.JobTypeIds.HNWar) &&
                j.JobMetaNonWarranty.IsInsuranceClaim &&
                !j.JobMetaInsurance.ClaimFormSentDate.HasValue;
        }
        [Obsolete("Use Log Insurance instead")]
        public static void OnInsuranceClaimFormSent(this Job j)
        {
            if (!j.CanInsuranceClaimFormSent())
                throw new InvalidOperationException("Insurance Claim Form Sent was Denied");

            var techUser = UserService.CurrentUser;

            j.JobMetaInsurance.ClaimFormSentDate = DateTime.Now;
            j.JobMetaInsurance.ClaimFormSentUserId = techUser.UserId;
        }
        #endregion

        #region Log Repair
        public static bool CanLogRepair(this Job j)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Job.Actions.LogRepair))
                return false;

            return (j.JobTypeId == JobType.JobTypeIds.HNWar) &&
                (j.DeviceSerialNumber != null) &&
                !j.JobMetaNonWarranty.RepairerLoggedDate.HasValue &&
                !j.JobMetaNonWarranty.RepairerCompletedDate.HasValue;
        }
        public static void OnLogRepair(this Job j, DiscoDataContext Database, string RepairDescription, List<JobAttachment> SendAttachments, PluginFeatureManifest RepairProviderDefinition, OrganisationAddress Address, User TechUser, Dictionary<string, string> RepairProviderProperties)
        {
            if (!j.CanLogRepair())
                throw new InvalidOperationException("Lodge Repair was Denied");

            PublishJobResult publishJobResult = null;

            using (RepairProviderFeature repairProvider = RepairProviderDefinition.CreateInstance<RepairProviderFeature>())
            {
                var repairProvider2 = repairProvider as RepairProvider2Feature;

                if (repairProvider2 == null && SendAttachments != null && SendAttachments.Count > 0)
                {
                    publishJobResult = DiscoServicesJobs.Publish(
                        Database,
                        j,
                        TechUser,
                        repairProvider.ProviderId,
                        null,
                        RepairDescription,
                        SendAttachments,
                        AttachmentDataStoreExtensions.RepositoryFilename);

                    if (!publishJobResult.Success)
                        throw new Exception(string.Format("Disco ICT Online Services failed with the following message: ", publishJobResult.ErrorMessage));

                    if (string.IsNullOrWhiteSpace(RepairDescription))
                        RepairDescription = publishJobResult.PublishMessage;
                    else
                        RepairDescription = string.Concat(RepairDescription, Environment.NewLine, "___", Environment.NewLine, publishJobResult.PublishMessage);
                }

                string submitDescription;

                if (string.IsNullOrWhiteSpace(RepairDescription))
                    submitDescription = j.GenerateFaultDescriptionFooter(Database, RepairProviderDefinition);
                else
                    submitDescription = string.Concat(RepairDescription, Environment.NewLine, Environment.NewLine, j.GenerateFaultDescriptionFooter(Database, RepairProviderDefinition));

                string providerRef;
                if (repairProvider2 != null)
                    providerRef = repairProvider2.SubmitJob(Database, j, Address, TechUser, submitDescription, SendAttachments, RepairProviderProperties);
                else
                    providerRef = repairProvider.SubmitJob(Database, j, Address, TechUser, submitDescription, RepairProviderProperties);

                j.JobMetaNonWarranty.RepairerLoggedDate = DateTime.Now;
                j.JobMetaNonWarranty.RepairerName = repairProvider.ProviderId;

                if (providerRef != null && providerRef.Length > 100)
                    j.JobMetaNonWarranty.RepairerReference = providerRef.Substring(0, 100);
                else
                    j.JobMetaNonWarranty.RepairerReference = providerRef;

                // Write Log
                JobLog jobLog = new JobLog()
                {
                    JobId = j.Id,
                    TechUserId = TechUser.UserId,
                    Timestamp = DateTime.Now,
                    Comments = string.Format("# Repair Request Submitted\r\nProvider: **{0}**\r\nAddress: **{1}**\r\nReference: **{2}**\r\n___\r\n```{3}```", repairProvider.Manifest.Name, Address.Name, providerRef, RepairDescription)
                };
                Database.JobLogs.Add(jobLog);

                if (publishJobResult != null)
                {
                    try
                    {
                        DiscoServicesJobs.UpdateRecipientReference(Database, j, publishJobResult.Id, publishJobResult.Secret, j.JobMetaNonWarranty.RepairerReference);
                    }
                    catch (Exception) { } // Ignore Errors as this is not completely necessary
                }
            }
        }
        public static void OnLogRepair(this Job j, DiscoDataContext Database, string FaultDescription, string ManualProviderName, string ManualProviderReference, OrganisationAddress Address, User TechUser)
        {
            if (!j.CanLogRepair())
                throw new InvalidOperationException("Lodge Repair was Denied");

            j.JobMetaNonWarranty.RepairerLoggedDate = DateTime.Now;
            j.JobMetaNonWarranty.RepairerName = ManualProviderName;

            if (ManualProviderReference != null && ManualProviderReference.Length > 100)
                j.JobMetaNonWarranty.RepairerReference = ManualProviderReference.Substring(0, 100);
            else
                j.JobMetaNonWarranty.RepairerReference = ManualProviderReference;

            // Write Log
            JobLog jobLog = new JobLog()
            {
                JobId = j.Id,
                TechUserId = TechUser.UserId,
                Timestamp = DateTime.Now,
                Comments = string.Format("# Manual Repair Request Submitted\r\nProvider: **{0}**\r\nAddress: **{1}**\r\nReference: **{2}**\r\n___\r\n```{3}```", ManualProviderName, Address.Name, ManualProviderReference ?? "<none>", FaultDescription)
            };
            Database.JobLogs.Add(jobLog);
        }
        #endregion

        #region Repair Complete
        public static bool CanRepairComplete(this Job j)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Job.Properties.NonWarrantyProperties.RepairerCompletedDate))
                return false;

            return (j.JobTypeId == JobType.JobTypeIds.HNWar) &&
                j.JobMetaNonWarranty.RepairerLoggedDate.HasValue &&
                !j.JobMetaNonWarranty.RepairerCompletedDate.HasValue;
        }
        public static void OnRepairComplete(this Job j)
        {
            if (!j.CanRepairComplete())
                throw new InvalidOperationException("Repair Complete was Denied");

            j.JobMetaNonWarranty.RepairerCompletedDate = DateTime.Now;
        }
        #endregion

        #region Log Insurance
        public static bool CanLogInsurance(this Job j)
        {
            if (!UserService.CurrentAuthorization.HasAny(Claims.Job.Actions.LogInsurance, Claims.Job.Properties.NonWarrantyProperties.InsuranceClaimFormSent))
                return false;

            return j.JobTypeId == JobType.JobTypeIds.HNWar &&
                j.DeviceSerialNumber != null &&
                j.JobMetaNonWarranty.IsInsuranceClaim &&
                !j.JobMetaInsurance.ClaimFormSentDate.HasValue;
        }
        public static void OnLogInsurance(this Job j, DiscoDataContext database, List<JobAttachment> attachments, PluginFeatureManifest providerDefinition, OrganisationAddress address, User techUser, Dictionary<string, string> providerProperties)
        {
            if (!j.CanLogInsurance())
                throw new InvalidOperationException("Lodge Insurance was Denied");

            using (var provider = providerDefinition.CreateInstance<InsuranceProviderFeature>())
            {
                var providerRef = provider.SubmitJob(database, j, address, techUser, attachments, providerProperties);

                j.JobMetaInsurance.Insurer = provider.ProviderId;
                j.JobMetaInsurance.ClaimFormSentDate = DateTime.Now;
                j.JobMetaInsurance.ClaimFormSentUserId = techUser.UserId;

                if (providerRef != null && providerRef.Length > 200)
                    j.JobMetaInsurance.InsurerReference = providerRef.Substring(0, 200);
                else
                    j.JobMetaInsurance.InsurerReference = providerRef;

                // Write Log
                var jobLog = new JobLog()
                {
                    JobId = j.Id,
                    TechUserId = techUser.UserId,
                    Timestamp = DateTime.Now,
                    Comments = $"# Insurance Claim Submitted\r\nProvider: **{provider.Manifest.Name}**\r\nAddress: **{address.Name}**\r\nReference: **{providerRef}**",
                };
                database.JobLogs.Add(jobLog);
            }
        }
        public static void OnLogInsurance(this Job j, DiscoDataContext database, string manualProviderName, string manualProviderReference, OrganisationAddress address, User techUser)
        {
            if (!j.CanLogInsurance())
                throw new InvalidOperationException("Lodge Insurance was Denied");

            j.JobMetaInsurance.Insurer = manualProviderName;
            j.JobMetaInsurance.ClaimFormSentDate = DateTime.Now;
            j.JobMetaInsurance.ClaimFormSentUserId = techUser.UserId;

            if (manualProviderReference != null && manualProviderReference.Length > 200)
                j.JobMetaInsurance.InsurerReference = manualProviderReference.Substring(0, 200);
            else
                j.JobMetaInsurance.InsurerReference = manualProviderReference;

            // Write Log
            JobLog jobLog = new JobLog()
            {
                JobId = j.Id,
                TechUserId = techUser.UserId,
                Timestamp = DateTime.Now,
                Comments = $"# Manual Insurance Request Submitted\r\nProvider: **{manualProviderName}**\r\nAddress: **{address.Name}**\r\nReference: **{manualProviderReference ?? "<none>"}**",
            };
            database.JobLogs.Add(jobLog);
        }
        #endregion

        #region Close
        public static void OnCloseNormally(this Job j, DiscoDataContext Database, User Technician)
        {
            if (!j.CanCloseNormally())
                throw new InvalidOperationException("Close was Denied");

            j.ClosedDate = DateTime.Now;
            j.ClosedTechUserId = Technician.UserId;
            j.ClosedTechUser = Technician;

            // Evaluate OnClose Expression
            try
            {
                var onCloseResult = j.EvaluateOnCloseExpression(Database);
                if (!string.IsNullOrWhiteSpace(onCloseResult))
                {
                    var jl = new JobLog()
                    {
                        Job = j,
                        TechUser = Technician,
                        Timestamp = DateTime.Now,
                        Comments = onCloseResult
                    };
                    Database.JobLogs.Add(jl);
                }
            }
            catch (Exception ex)
            {
                SystemLog.LogException("Job Expression - OnCloseExpression", ex);
            }
        }

        private static bool CanCloseNever(this Job j, JobQueueJob IgnoreJobQueueJob = null)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Job.Actions.Close))
                return true;

            if (j.ClosedDate.HasValue)
                return true; // Job already Closed

            if (j.DeviceHeld.HasValue && !j.DeviceReturnedDate.HasValue)
                return true; // Device not returned to User

            if (j.WaitingForUserAction.HasValue)
                return true; // Job waiting on User Action

            if (j.JobQueues != null)
            {
                if (IgnoreJobQueueJob == null)
                {
                    if (j.JobQueues.Any(jqj => !jqj.RemovedDate.HasValue))
                        return true; // Job associated with a Job Queue
                }
                else
                {
                    if (j.JobQueues.Any(jqj => jqj.Id != IgnoreJobQueueJob.Id && !jqj.RemovedDate.HasValue))
                        return true; // Job associated with a Job Queue
                }
            }

            return false;
        }

        public static bool CanCloseNormally(this Job j)
        {
            if (j.CanCloseNever())
                return false;

            return j.CanCloseNormallyInternal();
        }

        private static bool CanCloseNormallyInternal(this Job j)
        {
            switch (j.JobTypeId)
            {
                case JobType.JobTypeIds.HWar:

                    if (!string.IsNullOrEmpty(j.JobMetaWarranty.ExternalReference) && !j.JobMetaWarranty.ExternalCompletedDate.HasValue)
                        return false; // Job Logged (Warranty) but not completed

                    break;
                case JobType.JobTypeIds.HNWar:

                    if (j.JobMetaNonWarranty.RepairerLoggedDate.HasValue && !j.JobMetaNonWarranty.RepairerCompletedDate.HasValue)
                        return false; // Job Logged (Repair) but not completed

                    if (j.JobMetaNonWarranty.AccountingChargeRequiredDate.HasValue && !j.JobMetaNonWarranty.AccountingChargeAddedDate.HasValue)
                        return false; // Accounting Charge Required, but not added

                    if ((j.JobMetaNonWarranty.AccountingChargeRequiredDate.HasValue || j.JobMetaNonWarranty.AccountingChargeAddedDate.HasValue) && !j.JobMetaNonWarranty.AccountingChargePaidDate.HasValue)
                        return false; // Accounting Charge Required or Added, but not paid

                    if (j.JobMetaNonWarranty.IsInsuranceClaim && !j.JobMetaInsurance.ClaimFormSentDate.HasValue)
                        return false; // Is Insurance Claim, but claim form not sent

                    break;
            }

            return true;
        }

        public static bool CanCloseJobNormallyAfterRemoved(this JobQueueJob jqj)
        {
            if (jqj.Job.CanCloseNever(jqj))
                return false;

            return jqj.Job.CanCloseNormallyInternal();
        }
        #endregion

        #region Force Close
        public static bool CanCloseForced(this Job j)
        {
            List<string> reasons;

            return CanCloseForced(j, out reasons);
        }
        public static bool CanCloseForced(this Job j, out List<string> Reasons)
        {
            Reasons = null;

            if (!UserService.CurrentAuthorization.Has(Claims.Job.Actions.ForceClose))
                return false;

            if (j.CanCloseNever())
                return false;

            if (j.CanCloseNormally())
                return false;

            Reasons = new List<string>();

            switch (j.JobTypeId)
            {
                case JobType.JobTypeIds.HWar:
                    if (!string.IsNullOrEmpty(j.JobMetaWarranty.ExternalReference) && !j.JobMetaWarranty.ExternalCompletedDate.HasValue)
                        Reasons.Add("Warranty Job Not Completed"); // Job Logged (Warranty) but not completed
                    break;
                case JobType.JobTypeIds.HNWar:

                    if (j.JobMetaNonWarranty.RepairerLoggedDate.HasValue && !j.JobMetaNonWarranty.RepairerCompletedDate.HasValue)
                        Reasons.Add("Repair Job Not Completed"); // Job Logged (Repair) but not completed

                    if (j.JobMetaNonWarranty.AccountingChargeRequiredDate.HasValue && (!j.JobMetaNonWarranty.AccountingChargeAddedDate.HasValue && !j.JobMetaNonWarranty.AccountingChargePaidDate.HasValue))
                        Reasons.Add("Accounting Charge Required But Not Added Or Paid"); // Accounting Charge Required, but not added or paid
                    else if (j.JobMetaNonWarranty.AccountingChargeRequiredDate.HasValue && !j.JobMetaNonWarranty.AccountingChargeAddedDate.HasValue)
                        Reasons.Add("Accounting Charge Required But Not Added"); // Accounting Charge Required, but not added
                    else if (j.JobMetaNonWarranty.AccountingChargeAddedDate.HasValue && !j.JobMetaNonWarranty.AccountingChargePaidDate.HasValue)
                        Reasons.Add("Accounting Charge Added But Not Paid"); // Accounting Charge Added, but not paid
                    else if (j.JobMetaNonWarranty.AccountingChargeRequiredDate.HasValue && !j.JobMetaNonWarranty.AccountingChargePaidDate.HasValue)
                        Reasons.Add("Accounting Charge Required But Not Paid"); // Accounting Charge Required, but not paid

                    if (j.JobMetaNonWarranty.IsInsuranceClaim && !j.JobMetaInsurance.ClaimFormSentDate.HasValue)
                        Reasons.Add("Insurance Claim Form Not Sent"); // Is Insurance Claim, but claim form not sent

                    break;
            }

            return (Reasons.Count > 0);
        }
        public static void OnCloseForced(this Job j, DiscoDataContext Database, User Technician, string Reason)
        {
            if (!j.CanCloseForced())
                throw new InvalidOperationException("Force Close was Denied");

            // Write Log
            JobLog jobLog = new JobLog()
            {
                JobId = j.Id,
                TechUserId = Technician.UserId,
                Timestamp = DateTime.Now,
                Comments = $@"## Job Forcibly Closed

{(string.IsNullOrWhiteSpace(Reason) ? "<no reason provided>" : Reason)}"
            };
            Database.JobLogs.Add(jobLog);

            j.ClosedDate = DateTime.Now;
            j.ClosedTechUserId = Technician.UserId;
            j.ClosedTechUser = Technician;

            // Evaluate OnClose Expression
            try
            {
                var onCloseResult = j.EvaluateOnCloseExpression(Database);
                if (!string.IsNullOrWhiteSpace(onCloseResult))
                {
                    var jl = new JobLog()
                    {
                        Job = j,
                        TechUser = Technician,
                        Timestamp = DateTime.Now,
                        Comments = onCloseResult
                    };
                    Database.JobLogs.Add(jl);
                }
            }
            catch (Exception ex)
            {
                SystemLog.LogException("Job Expression - OnCloseExpression", ex);
            }
        }
        #endregion

        #region Reopen
        public static bool CanReopen(this Job j)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Job.Actions.Reopen))
                return false;

            return j.ClosedDate.HasValue;
        }
        public static void OnReopen(this Job j, DiscoDataContext database, User technician)
        {
            if (!j.CanReopen())
                throw new InvalidOperationException("Reopen was Denied");

            var log = new JobLog()
            {
                JobId = j.Id,
                TechUserId = technician.UserId,
                Timestamp = DateTime.Now,
                Comments = $@"## Job Re-Opened

Previously Closed by {j.ClosedTechUser.DisplayName} [`@{j.ClosedTechUser.FriendlyId()}`] at `{j.ClosedDate:yyyy-MM-dd HH:mm}`.",
            };
            database.JobLogs.Add(log);

            j.ClosedDate = null;
            j.ClosedTechUserId = null;
        }
        #endregion

        #region Delete
        public static bool CanDelete(this Job j)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Job.Actions.Delete))
                return false;

            return j.ClosedDate.HasValue;
        }
        public static void OnDelete(this Job j, DiscoDataContext Database)
        {
            // Job Sub Types
            j.JobSubTypes.Clear();

            // Job Attachments
            foreach (var ja in j.JobAttachments.ToArray())
                ja.OnDelete(Database);
            j.JobAttachments.Clear();

            // Job Components
            foreach (var jc in j.JobComponents.ToArray())
                Database.JobComponents.Remove(jc);
            j.JobComponents.Clear();

            // Job Queue Jobs
            foreach (var jqj in j.JobQueues.ToArray())
                Database.JobQueueJobs.Remove(jqj);
            j.JobQueues.Clear();

            // Job Logs
            foreach (var jl in j.JobLogs.ToArray())
                Database.JobLogs.Remove(jl);
            j.JobLogs.Clear();

            // Job Meta
            if (j.JobMetaInsurance != null)
            {
                Database.JobMetaInsurances.Remove(j.JobMetaInsurance);
                j.JobMetaInsurance = null;
            }
            if (j.JobMetaNonWarranty != null)
            {
                Database.JobMetaNonWarranties.Remove(j.JobMetaNonWarranty);
                j.JobMetaNonWarranty = null;
            }
            if (j.JobMetaWarranty != null)
            {
                Database.JobMetaWarranties.Remove(j.JobMetaWarranty);
                j.JobMetaWarranty = null;
            }

            // Job
            Database.Jobs.Remove(j);
        }
        #endregion

    }
}
