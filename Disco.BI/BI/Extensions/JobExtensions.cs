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
using Disco.Services.Authorization;

namespace Disco.BI.Extensions
{
    public static class JobExtensions
    {
        public static JobAttachment CreateAttachment(this Job Job, DiscoDataContext Database, User CreatorUser, string Filename, string MimeType, string Comments, Stream Content, DocumentTemplate DocumentTemplate = null, byte[] PdfThumbnail = null)
        {
            if (string.IsNullOrEmpty(MimeType) || MimeType.Equals("unknown/unknown", StringComparison.OrdinalIgnoreCase))
                MimeType = Interop.MimeTypes.ResolveMimeType(Filename);

            JobAttachment ja = new JobAttachment()
            {
                JobId = Job.Id,
                TechUserId = CreatorUser.UserId,
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
    }
}
