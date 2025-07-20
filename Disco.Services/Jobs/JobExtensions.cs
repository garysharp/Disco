using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Documents;
using Disco.Models.Services.Jobs.JobLists;
using Disco.Services.Authorization;
using Disco.Services.Documents;
using Disco.Services.Expressions;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Plugins;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disco.Services
{
    public static class JobExtensions
    {
        public static JobTableStatusItemModel ToJobTableStatusItemModel(this Job j)
        {
            var i = new JobTableStatusItemModel()
            {
                JobId = j.Id,
                OpenedDate = j.OpenedDate,
                ClosedDate = j.ClosedDate,
                JobTypeId = j.JobTypeId,
                JobTypeDescription = j.JobType.Description,
                DeviceHeldLocation = j.DeviceHeldLocation,
                Flags = j.Flags,

                WaitingForUserAction = j.WaitingForUserAction,
                DeviceReadyForReturn = j.DeviceReadyForReturn,
                DeviceHeld = j.DeviceHeld,
                DeviceReturnedDate = j.DeviceReturnedDate
            };

            if (j.Device != null)
            {
                i.DeviceSerialNumber = j.DeviceSerialNumber;
                i.DeviceModelDescription = j.Device.DeviceModel.Description;
                i.DeviceAddressId = j.Device.DeviceProfile.DefaultOrganisationAddress;

                if (j.JobMetaWarranty != null)
                {
                    i.JobMetaWarranty_ExternalReference = j.JobMetaWarranty.ExternalReference;
                    i.JobMetaWarranty_ExternalLoggedDate = j.JobMetaWarranty.ExternalLoggedDate;
                    i.JobMetaWarranty_ExternalCompletedDate = j.JobMetaWarranty.ExternalCompletedDate;
                    i.JobMetaWarranty_ExternalName = j.JobMetaWarranty.ExternalName;
                }
                if (j.JobMetaNonWarranty != null)
                {
                    i.JobMetaNonWarranty_RepairerLoggedDate = j.JobMetaNonWarranty.RepairerLoggedDate;
                    i.JobMetaNonWarranty_RepairerCompletedDate = j.JobMetaNonWarranty.RepairerCompletedDate;
                    i.JobMetaNonWarranty_AccountingChargeAddedDate = j.JobMetaNonWarranty.AccountingChargeAddedDate;
                    i.JobMetaNonWarranty_AccountingChargePaidDate = j.JobMetaNonWarranty.AccountingChargePaidDate;
                    i.JobMetaNonWarranty_AccountingChargeRequiredDate = j.JobMetaNonWarranty.AccountingChargeRequiredDate;
                    i.JobMetaNonWarranty_IsInsuranceClaim = j.JobMetaNonWarranty.IsInsuranceClaim;
                    i.JobMetaNonWarranty_RepairerName = j.JobMetaNonWarranty.RepairerName;
                    if (j.JobMetaInsurance != null)
                    {
                        i.JobMetaInsurance_ClaimFormSentDate = j.JobMetaInsurance.ClaimFormSentDate;
                    }
                }

            }
            if (j.User != null)
            {
                i.UserId = j.UserId;
                i.UserDisplayName = j.User.DisplayName;

                i.UserFriendlyId = ActiveDirectory.FriendlyAccountId(j.UserId);
            }
            if (j.OpenedTechUser != null)
            {
                i.OpenedTechUserId = j.OpenedTechUserId;
                i.OpenedTechUserFriendlyId = ActiveDirectory.FriendlyAccountId(j.OpenedTechUserId);
                i.OpenedTechUserDisplayName = j.OpenedTechUser.DisplayName;
            }

            return i;
        }

        public static string JobStatusDescription(string StatusId, Job j)
            => JobStatusDescription(StatusId, j?.DeviceHeld, j?.JobMetaWarranty?.ExternalName, j?.JobMetaNonWarranty?.RepairerName);

        public static string JobStatusDescription(string StatusId, JobTableStatusItemModel j)
            => JobStatusDescription(StatusId, j?.DeviceHeld, j?.JobMetaWarranty_ExternalName, j?.JobMetaNonWarranty_RepairerName);

        public static string JobStatusDescription(string statusId, DateTime? deviceHeld = null, string warrantyExternalName = null, string nonWarrantyRepairerName = null)
        {
            if (!Job.JobStatusIds.StatusDescriptions.TryGetValue(statusId, out var statusDescription))
                return "Unknown";

            switch (statusId)
            {
                case Job.JobStatusIds.AwaitingWarrantyRepair:
                    var warrantyName = string.Empty;
                    if (!string.IsNullOrWhiteSpace(warrantyExternalName))
                        warrantyName = $" ({warrantyExternalName})";

                    if (deviceHeld.HasValue)
                        return statusDescription + warrantyName;
                    else
                        return $"{statusDescription} - Not Held{warrantyExternalName}";
                case Job.JobStatusIds.AwaitingRepairs:
                    var repairerName = string.Empty;
                    if (!string.IsNullOrWhiteSpace(nonWarrantyRepairerName))
                        repairerName = $" ({nonWarrantyRepairerName})";

                    if (deviceHeld.HasValue)
                        return statusDescription + repairerName;
                    else
                        return $"{statusDescription} - Not Held{repairerName}";
                default:
                    return statusDescription;
            }
        }

        public static string CalculateStatusId(this Job j)
        {
            return CalculateStatusId(
                j.ClosedDate,
                j.JobTypeId,
                j.JobMetaWarranty?.ExternalLoggedDate,
                j.JobMetaWarranty?.ExternalCompletedDate,
                j.JobMetaNonWarranty?.RepairerLoggedDate,
                j.JobMetaNonWarranty?.RepairerCompletedDate,
                j.JobMetaNonWarranty?.AccountingChargeRequiredDate,
                j.JobMetaNonWarranty?.AccountingChargeAddedDate,
                j.JobMetaNonWarranty?.AccountingChargePaidDate,
                j.JobMetaNonWarranty?.IsInsuranceClaim,
                j.JobMetaInsurance?.ClaimFormSentDate,
                j.WaitingForUserAction,
                j.DeviceReadyForReturn,
                j.DeviceReturnedDate);
        }

        public static string CalculateStatusId(this JobTableStatusItemModel j)
        {
            return CalculateStatusId(
                j.ClosedDate,
                j.JobTypeId,
                j.JobMetaWarranty_ExternalLoggedDate,
                j.JobMetaWarranty_ExternalCompletedDate,
                j.JobMetaNonWarranty_RepairerLoggedDate,
                j.JobMetaNonWarranty_RepairerCompletedDate,
                j.JobMetaNonWarranty_AccountingChargeRequiredDate,
                j.JobMetaNonWarranty_AccountingChargeAddedDate,
                j.JobMetaNonWarranty_AccountingChargePaidDate,
                j.JobMetaNonWarranty_IsInsuranceClaim,
                j.JobMetaInsurance_ClaimFormSentDate,
                j.WaitingForUserAction,
                j.DeviceReadyForReturn,
                j.DeviceReturnedDate);
        }

        public static string CalculateStatusId(
            DateTime? closedDate,
            string jobTypeId,
            DateTime? warrantyExternallyLoggedDate,
            DateTime? warrantyExternallyCompletedDate,
            DateTime? nonWarrantyRepairerLoggedDate,
            DateTime? nonWarrantyRepairerCompletedDate,
            DateTime? nonWarrantyAccountingChargeRequiredDate,
            DateTime? nonWarrantyAccountintChargeAddedDate,
            DateTime? nonWarrantyAccountintChargePaidDate,
            bool? nonWarrantyIsInsuranceClaim,
            DateTime? insuranceClaimFormSentDate,
            DateTime? waitingForUserActionDate,
            DateTime? deviceReadyForReturnDate,
            DateTime? deviceReturnedDate)
        {
            if (closedDate.HasValue)
                return Job.JobStatusIds.Closed;

            if (jobTypeId == JobType.JobTypeIds.HWar)
            {
                if (warrantyExternallyLoggedDate.HasValue && !warrantyExternallyCompletedDate.HasValue)
                    return Job.JobStatusIds.AwaitingWarrantyRepair; // Job Logged - but not marked as completed
            }

            if (jobTypeId == JobType.JobTypeIds.HNWar)
            {
                if (nonWarrantyRepairerLoggedDate.HasValue && !nonWarrantyRepairerCompletedDate.HasValue)
                    return Job.JobStatusIds.AwaitingRepairs; // Repairs logged - but not complete
                if (nonWarrantyAccountintChargeAddedDate.HasValue && !nonWarrantyAccountintChargePaidDate.HasValue)
                    return Job.JobStatusIds.AwaitingAccountingPayment; // Accounting Charge Added, but not paid
                if (nonWarrantyAccountingChargeRequiredDate.HasValue && (!nonWarrantyAccountintChargePaidDate.HasValue || !nonWarrantyAccountintChargeAddedDate.HasValue))
                    return Job.JobStatusIds.AwaitingAccountingCharge; // Accounting Charge Required, but not added or paid
                if (nonWarrantyRepairerLoggedDate.HasValue && nonWarrantyIsInsuranceClaim.GetValueOrDefault() && !insuranceClaimFormSentDate.HasValue)
                    return Job.JobStatusIds.AwaitingInsuranceProcessing; // Is insurance claim, but no Claim Form Sent
            }

            if (waitingForUserActionDate.HasValue)
                return Job.JobStatusIds.AwaitingUserAction; // Awaiting for User

            if (deviceReadyForReturnDate.HasValue && !deviceReturnedDate.HasValue)
                return Job.JobStatusIds.AwaitingDeviceReturn; // Device not returned to User

            return Job.JobStatusIds.Open;
        }

        public static Tuple<string, string> Status(this Job j)
        {
            var statusId = j.CalculateStatusId();
            return Tuple.Create(statusId, JobStatusDescription(statusId, j));
        }

        public static List<DocumentTemplate> AvailableDocumentTemplates(this Job j, DiscoDataContext Database, User User, DateTime TimeStamp)
        {
            var dts = Database.DocumentTemplates.Include("JobSubTypes")
                .Where(dt => !dt.IsHidden && dt.Scope == DocumentTemplate.DocumentTemplateScopes.Job)
                .ToList();

            foreach (var dt in dts.ToArray())
            {
                if (dt.JobSubTypes.Count != 0)
                { // Filter Applied
                    bool match = false;
                    foreach (var st in j.JobSubTypes)
                    {
                        if (dt.JobSubTypes.Contains(st))
                        {
                            match = true;
                            break;
                        }
                    }
                    if (!match)
                        dts.Remove(dt);
                }
            }

            // Evaluate Filters
            dts = dts.Where(dt => dt.FilterExpressionMatches(j, Database, User, TimeStamp, DocumentState.DefaultState())).ToList();

            return dts;
        }
        public static List<DocumentTemplatePackage> AvailableDocumentTemplatePackages(this Job j, DiscoDataContext Database, User TechnicianUser)
        {
            return DocumentTemplatePackages.AvailablePackages(j, Database, TechnicianUser);
        }

        public static DateTime ValidateDateAfterOpened(this Job j, DateTime d)
        {
            if (d < j.OpenedDate)
            {
                if (d > j.OpenedDate.AddMinutes(-1))
                    return j.OpenedDate;
                else
                    throw new ArgumentException("The Date must be >= the Open Date.", "d");
            }
            return d;
        }

        public static string GenerateFaultDescription(this Job j, DiscoDataContext Database)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Faulty Components:");
            foreach (var jst in j.JobSubTypes)
                sb.Append("- ").AppendLine(jst.Description).AppendLine("   - ");

            return sb.ToString();
        }

        public static string GenerateFaultDescriptionFooter(this Job j, DiscoDataContext Database, PluginFeatureManifest WarrantyProviderDefinition)
        {
            var versionDisco = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return $"Automation by Disco ICT v{versionDisco.Major}.{versionDisco.Minor}.{versionDisco.Build:0000}.{versionDisco.Revision:0000} (Provider: {WarrantyProviderDefinition.Id} v{WarrantyProviderDefinition.PluginManifest.Version.ToString(4)})";
        }

        public static void UpdateSubTypes(this Job j, DiscoDataContext Database, List<JobSubType> SubTypes, bool AddComponents, User TechUser)
        {
            if (SubTypes == null || SubTypes.Count == 0)
                throw new ArgumentException("The Job must contain at least one Sub Type");

            List<JobSubType> addedSubTypes = new List<JobSubType>();
            List<JobSubType> removedSubTypes = new List<JobSubType>();

            // Removed Sub Types
            foreach (var t in j.JobSubTypes.ToArray())
                if (!SubTypes.Contains(t))
                {
                    removedSubTypes.Add(t);
                    j.JobSubTypes.Remove(t);
                }
            // Added Sub Types
            foreach (var t in SubTypes)
                if (!j.JobSubTypes.Contains(t))
                {
                    addedSubTypes.Add(t);
                    j.JobSubTypes.Add(t);
                }

            // Write Log
            if (addedSubTypes.Count > 0 || removedSubTypes.Count > 0)
            {
                StringBuilder logBuilder = new StringBuilder();
                logBuilder.AppendLine("# Updated Job Sub Types");
                if (removedSubTypes.Count > 0)
                {
                    logBuilder.AppendLine().AppendLine("Removed:");
                    foreach (var t in removedSubTypes)
                        logBuilder.Append("- **").Append(t.ToString()).AppendLine("**");
                }
                if (addedSubTypes.Count > 0)
                {
                    logBuilder.AppendLine().AppendLine("Added:");
                    foreach (var t in addedSubTypes)
                        logBuilder.Append("- **").Append(t.ToString()).AppendLine("**");
                }
                Database.JobLogs.Add(new JobLog()
                {
                    JobId = j.Id,
                    TechUserId = TechUser.UserId,
                    Timestamp = DateTime.Now,
                    Comments = logBuilder.ToString()
                });
            }

            // Add Components
            if (AddComponents && addedSubTypes.Count > 0 && j.DeviceSerialNumber != null)
            {
                var components = Database.DeviceComponents.Include("JobSubTypes").Where(c => !c.DeviceModelId.HasValue || c.DeviceModelId == j.Device.DeviceModelId);
                var addedComponents = new List<DeviceComponent>();
                foreach (var c in components)
                {
                    foreach (var st in c.JobSubTypes)
                    {
                        foreach (var jst in addedSubTypes)
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
                foreach (var c in addedComponents)
                {
                    if (!j.JobComponents.Any(jc => jc.Description.Equals(c.Description, StringComparison.OrdinalIgnoreCase)))
                    { // Job Component with matching Description doesn't exist.
                        Database.JobComponents.Add(new JobComponent()
                        {
                            Job = j,
                            TechUserId = TechUser.UserId,
                            Cost = c.Cost,
                            Description = c.Description
                        });
                    }
                }
            }
        }

        private static List<string> FilterCreatableTypePermissions(AuthorizationToken Authorization)
        {
            if (!Authorization.HasAll(Claims.Job.Types.CreateHMisc, Claims.Job.Types.CreateHNWar, Claims.Job.Types.CreateHWar, Claims.Job.Types.CreateSApp, Claims.Job.Types.CreateSImg, Claims.Job.Types.CreateSOS, Claims.Job.Types.CreateUMgmt))
            {
                // Must Filter
                List<string> allowedTypes = new List<string>(6);
                if (Authorization.Has(Claims.Job.Types.CreateHMisc))
                    allowedTypes.Add(JobType.JobTypeIds.HMisc);
                if (Authorization.Has(Claims.Job.Types.CreateHNWar))
                    allowedTypes.Add(JobType.JobTypeIds.HNWar);
                if (Authorization.Has(Claims.Job.Types.CreateHWar))
                    allowedTypes.Add(JobType.JobTypeIds.HWar);
                if (Authorization.Has(Claims.Job.Types.CreateSApp))
                    allowedTypes.Add(JobType.JobTypeIds.SApp);
                if (Authorization.Has(Claims.Job.Types.CreateSImg))
                    allowedTypes.Add(JobType.JobTypeIds.SImg);
                if (Authorization.Has(Claims.Job.Types.CreateSOS))
                    allowedTypes.Add(JobType.JobTypeIds.SOS);
                if (Authorization.Has(Claims.Job.Types.CreateUMgmt))
                    allowedTypes.Add(JobType.JobTypeIds.UMgmt);

                return allowedTypes;
            }
            return null;
        }

        public static IQueryable<JobType> FilterCreatableTypePermissions(this IQueryable<JobType> JobTypes, AuthorizationToken Authorization)
        {
            var allowedTypes = FilterCreatableTypePermissions(Authorization);

            if (allowedTypes != null)
            {
                return JobTypes.Where(jt => allowedTypes.Contains(jt.Id));
            }

            return JobTypes;
        }

        #region Expressions

        public static string EvaluateOnCreateExpression(this Job job, DiscoDataContext Database)
        {
            if (!string.IsNullOrEmpty(Database.DiscoConfiguration.JobPreferences.OnCreateExpression))
            {
                Expression compiledExpression = Jobs.Jobs.OnCreateExpressionFromCache(Database);
                IDictionary evaluatorVariables = Expression.StandardVariables(null, Database, job.OpenedTechUser, DateTime.Now, null, job);
                object result = compiledExpression.EvaluateFirst<object>(job, evaluatorVariables);
                if (result == null)
                    return null;
                else
                    return result.ToString();
            }
            return null;
        }

        public static string EvaluateOnDeviceReadyForReturnExpression(this Job job, DiscoDataContext Database)
        {
            if (!string.IsNullOrEmpty(Database.DiscoConfiguration.JobPreferences.OnDeviceReadyForReturnExpression))
            {
                Expression compiledExpression = Jobs.Jobs.OnDeviceReadyForReturnExpressionFromCache(Database);
                IDictionary evaluatorVariables = Expression.StandardVariables(null, Database, job.OpenedTechUser, DateTime.Now, null, job);
                object result = compiledExpression.EvaluateFirst<object>(job, evaluatorVariables);
                if (result == null)
                    return null;
                else
                    return result.ToString();
            }
            return null;
        }

        public static string EvaluateOnCloseExpression(this Job job, DiscoDataContext Database)
        {
            if (!string.IsNullOrEmpty(Database.DiscoConfiguration.JobPreferences.OnCloseExpression))
            {
                Expression compiledExpression = Jobs.Jobs.OnCloseExpressionFromCache(Database);
                IDictionary evaluatorVariables = Expression.StandardVariables(null, Database, job.OpenedTechUser, DateTime.Now, null, job);
                object result = compiledExpression.EvaluateFirst<object>(job, evaluatorVariables);
                if (result == null)
                    return null;
                else
                    return result.ToString();
            }
            return null;
        }

        #endregion

    }
}
