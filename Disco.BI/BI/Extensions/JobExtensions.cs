using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.Repository;
using Disco.Data.Repository;
using System.IO;
using Disco.Models.BI.DocumentTemplates;
using Disco.Services.Plugins;
using Disco.Models.BI.Job;

namespace Disco.BI.Extensions
{
    public static class JobExtensions
    {
        public static JobAttachment CreateAttachment(this Job Job, DiscoDataContext Database, User CreatorUser, string Filename, string MimeType, string Comments, Stream Content, DocumentTemplate DocumentTemplate = null, byte[] PdfThumbnail = null)
        {
            if (string.IsNullOrEmpty(MimeType) || MimeType.Equals("unknown/unknown", StringComparison.InvariantCultureIgnoreCase))
                MimeType = Interop.MimeTypes.ResolveMimeType(Filename);

            JobAttachment ja = new JobAttachment()
            {
                JobId = Job.Id,
                TechUserId = CreatorUser.Id,
                Filename = Filename,
                MimeType = MimeType,
                Timestamp = DateTime.Now,
                Comments = Comments
            };

            if (DocumentTemplate != null)
                ja.DocumentTemplateId = DocumentTemplate.Id;

            Database.JobAttachments.Add(ja);
            Database.SaveChanges();

            ja.SaveAttachment(Database, Content);
            Content.Position = 0;
            if (PdfThumbnail == null)
                ja.GenerateThumbnail(Database, Content);
            else
                ja.SaveThumbnailAttachment(Database, PdfThumbnail);

            return ja;
        }

        public static Tuple<string, string> Status(this Job j)
        {
            var statusId = j.CalculateStatusId();
            return new Tuple<string, string>(statusId, JobBI.Utilities.JobStatusDescription(statusId, j));
        }

        public static JobTableModel.JobTableItemModelIncludeStatus ToJobTableItemModelIncludeStatus(this Job j)
        {
            var i = new JobTableModel.JobTableItemModelIncludeStatus()
                    {
                        Id = j.Id,
                        OpenedDate = j.OpenedDate,
                        ClosedDate = j.ClosedDate,
                        TypeId = j.JobTypeId,
                        TypeDescription = j.JobType.Description,
                        Location = j.DeviceHeldLocation,

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
            }
            if (j.OpenedTechUser != null)
            {
                i.OpenedTechUserId = j.OpenedTechUserId;
                i.OpenedTechUserDisplayName = j.OpenedTechUser.DisplayName;
            }

            return i;
        }

        public static string CalculateStatusId(this Job j)
        {
            return j.ToJobTableItemModelIncludeStatus().CalculateStatusId();
        }

        public static string CalculateStatusId(this JobTableModel.JobTableItemModelIncludeStatus j)
        {
            if (j.ClosedDate.HasValue)
                return Job.JobStatusIds.Closed;

            if (j.TypeId == JobType.JobTypeIds.HWar)
            {
                if (!string.IsNullOrEmpty(j.JobMetaWarranty_ExternalReference) && !j.JobMetaWarranty_ExternalCompletedDate.HasValue)
                    return Job.JobStatusIds.AwaitingWarrantyRepair; // Job Logged - but not marked as completed
            }

            if (j.TypeId == JobType.JobTypeIds.HNWar)
            {
                if (j.JobMetaNonWarranty_RepairerLoggedDate.HasValue && !j.JobMetaNonWarranty_RepairerCompletedDate.HasValue)
                    return Job.JobStatusIds.AwaitingRepairs; // Repairs logged - but not complete
                if (j.JobMetaNonWarranty_AccountingChargeAddedDate.HasValue && !j.JobMetaNonWarranty_AccountingChargePaidDate.HasValue)
                    return Job.JobStatusIds.AwaitingAccountingPayment; // Accounting Charge Added, but not paid
                if (j.JobMetaNonWarranty_AccountingChargeRequiredDate.HasValue && (!j.JobMetaNonWarranty_AccountingChargePaidDate.HasValue || !j.JobMetaNonWarranty_AccountingChargeAddedDate.HasValue))
                    return Job.JobStatusIds.AwaitingAccountingCharge; // Accounting Charge Required, but not added or paid
                if (j.JobMetaNonWarranty_RepairerLoggedDate.HasValue && j.JobMetaNonWarranty_IsInsuranceClaim.Value && !j.JobMetaInsurance_ClaimFormSentDate.HasValue)
                    return Job.JobStatusIds.AwaitingInsuranceProcessing; // Is insurance claim, but no Claim Form Sent
            }

            if (j.WaitingForUserAction.HasValue)
                return Job.JobStatusIds.AwaitingUserAction; // Awaiting for User

            if (j.DeviceReadyForReturn.HasValue && !j.DeviceReturnedDate.HasValue)
                return Job.JobStatusIds.AwaitingDeviceReturn; // Device not returned to User

            return Job.JobStatusIds.Open;
        }

        public static List<DocumentTemplate> AvailableDocumentTemplates(this Job j, DiscoDataContext Database, User User, DateTime TimeStamp)
        {
            var dts = Database.DocumentTemplates.Include("JobSubTypes")
                .Where(dt => dt.Scope == DocumentTemplate.DocumentTemplateScopes.Job)
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
            return string.Format("Automation by Disco v{0}.{1}.{2:0000}.{3:0000} (Provider: {4} v{5})",
                versionDisco.Major, versionDisco.Minor, versionDisco.Build, versionDisco.Revision, WarrantyProviderDefinition.Id, WarrantyProviderDefinition.PluginManifest.Version.ToString(4));
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
                logBuilder.AppendLine("Updated Job Sub Types");
                if (removedSubTypes.Count > 0)
                {
                    logBuilder.AppendLine("Removed:");
                    foreach (var t in removedSubTypes)
                        logBuilder.Append("- ").AppendLine(t.ToString());
                }
                if (addedSubTypes.Count > 0)
                {
                    logBuilder.AppendLine("Added:");
                    foreach (var t in addedSubTypes)
                        logBuilder.Append("- ").AppendLine(t.ToString());
                }
                Database.JobLogs.Add(new JobLog()
                {
                    JobId = j.Id,
                    TechUserId = TechUser.Id,
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
                    if (!j.JobComponents.Any(jc => jc.Description.Equals(c.Description, StringComparison.InvariantCultureIgnoreCase)))
                    { // Job Component with matching Description doesn't exist.
                        Database.JobComponents.Add(new JobComponent()
                        {
                            Job = j,
                            TechUserId = TechUser.Id,
                            Cost = c.Cost,
                            Description = c.Description
                        });
                    }
                }
            }
        }

    }
}
