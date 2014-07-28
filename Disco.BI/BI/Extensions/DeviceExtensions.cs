using System.Linq;
using Disco.Data.Configuration;
using Disco.Data.Repository;
using Disco.Models.BI.DocumentTemplates;
using Disco.Models.Repository;
using System.Collections.Generic;
using System;
using System.IO;
using Disco.Services.Users;
using Disco.Services.Authorization;
using Disco.Services.Interop.ActiveDirectory;
using Exceptionless;

namespace Disco.BI.Extensions
{
    public static class DeviceExtensions
    {

        public static string ComputerNameRender(this Device device, DiscoDataContext Database, ADDomain Domain)
        {
            if (Domain == null)
                throw new ArgumentNullException("Domain");

            DeviceProfile deviceProfile = device.DeviceProfile;
            Expressions.Expression computerNameTemplateExpression = null;
            computerNameTemplateExpression = Expressions.ExpressionCache.GetValue(DeviceProfileExtensions.ComputerNameExpressionCacheModule, deviceProfile.Id.ToString(), () =>
            {
                // Removed 2012-06-14 G# - Properties moved to DeviceProfile model & DB Migrated in DBv3.
                //return Expressions.Expression.TokenizeSingleDynamic(null, deviceProfile.Configuration(context).ComputerNameTemplate, 0);
                return Expressions.Expression.TokenizeSingleDynamic(null, deviceProfile.ComputerNameTemplate, 0);
            });
            System.Collections.IDictionary evaluatorVariables = Expressions.Expression.StandardVariables(null, Database, UserService.CurrentUser, System.DateTime.Now, null);
            string rendered;
            try
            {
                rendered = computerNameTemplateExpression.EvaluateFirst<string>(device, evaluatorVariables);
            }
            catch (Exception ex)
            {
                ex.ToExceptionless().AddObject(deviceProfile.ComputerNameTemplate, "ComputerNameTemplate").Submit();
                throw new InvalidOperationException(string.Format("An error occurred rendering the computer name: [{0}] {1}", ex.GetType().Name, ex.Message), ex.InnerException);
            }
            if (rendered == null || rendered.Length > 24)
            {
                throw new System.InvalidOperationException("The rendered computer name would be invalid or longer than 24 characters");
            }

            return string.Format(@"{0}\{1}", Domain.NetBiosName, rendered);
        }
        public static System.Collections.Generic.List<DocumentTemplate> AvailableDocumentTemplates(this Device d, DiscoDataContext Database, User User, System.DateTime TimeStamp)
        {
            List<DocumentTemplate> ats = Database.DocumentTemplates
                .Where(at => at.Scope == Disco.Models.Repository.DocumentTemplate.DocumentTemplateScopes.Device).ToList();

            return ats.Where(at => at.FilterExpressionMatches(d, Database, User, TimeStamp, DocumentState.DefaultState())).ToList();
        }

        public static bool UpdateLastNetworkLogonDate(this Device Device)
        {
            return Disco.Services.Interop.ActiveDirectory.ADNetworkLogonDatesUpdateTask.UpdateLastNetworkLogonDate(Device);
        }

        public static DeviceAttachment CreateAttachment(this Device Device, DiscoDataContext Database, User CreatorUser, string Filename, string MimeType, string Comments, Stream Content, DocumentTemplate DocumentTemplate = null, byte[] PdfThumbnail = null)
        {
            if (string.IsNullOrEmpty(MimeType) || MimeType.Equals("unknown/unknown", StringComparison.OrdinalIgnoreCase))
                MimeType = Interop.MimeTypes.ResolveMimeType(Filename);

            DeviceAttachment da = new DeviceAttachment()
            {
                DeviceSerialNumber = Device.SerialNumber,
                TechUserId = CreatorUser.UserId,
                Filename = Filename,
                MimeType = MimeType,
                Timestamp = DateTime.Now,
                Comments = Comments
            };

            if (DocumentTemplate != null)
                da.DocumentTemplateId = DocumentTemplate.Id;

            Database.DeviceAttachments.Add(da);
            Database.SaveChanges();

            da.SaveAttachment(Database, Content);
            Content.Position = 0;
            if (PdfThumbnail == null)
                da.GenerateThumbnail(Database, Content);
            else
                da.SaveThumbnailAttachment(Database, PdfThumbnail);

            return da;
        }

        public static Device AddOffline(this Device d, DiscoDataContext Database)
        {
            // Just Include:
            // - Serial Number
            // - Asset Number
            // - Profile Id
            // - Assigned User Id
            // - Batch

            // Enforce Authorization
            var auth = UserService.CurrentAuthorization;
            if (!auth.Has(Claims.Device.Properties.AssetNumber))
                d.AssetNumber = null;
            if (!auth.Has(Claims.Device.Properties.Location))
                d.Location = null;
            if (!auth.Has(Claims.Device.Properties.DeviceBatch))
                d.DeviceBatchId = null;
            if (!auth.Has(Claims.Device.Properties.DeviceProfile))
                d.DeviceProfileId = Database.DiscoConfiguration.DeviceProfiles.DefaultAddDeviceOfflineDeviceProfileId;
            if (!auth.Has(Claims.Device.Actions.AssignUser))
                d.AssignedUserId = null;


            // Batch
            DeviceBatch db = default(DeviceBatch);
            if (d.DeviceBatchId.HasValue)
                db = Database.DeviceBatches.Find(d.DeviceBatchId.Value);

            // Default Device Model
            DeviceModel dm = default(DeviceModel);
            if (db != null && db.DefaultDeviceModelId.HasValue)
                dm = Database.DeviceModels.Find(db.DefaultDeviceModelId); // From Batch
            else
                dm = Database.DeviceModels.Find(1); // Default

            Device d2 = new Device()
            {
                SerialNumber = d.SerialNumber.ToUpper(),
                AssetNumber = d.AssetNumber,
                Location = d.Location,
                CreatedDate = DateTime.Now,
                DeviceProfileId = d.DeviceProfileId,
                DeviceProfile = Database.DeviceProfiles.Find(d.DeviceProfileId),
                AllowUnauthenticatedEnrol = true,
                DeviceModelId = dm.Id,
                DeviceModel = dm,
                DeviceBatchId = d.DeviceBatchId,
                DeviceBatch = db
            };

            Database.Devices.Add(d2);
            if (!string.IsNullOrEmpty(d.AssignedUserId))
            {
                User u = UserService.GetUser(ActiveDirectory.ParseDomainAccountId(d.AssignedUserId), Database, true);
                d2.AssignDevice(Database, u);
            }

            return d2;
        }

        public static DeviceUserAssignment AssignDevice(this Device d, DiscoDataContext Database, User u)
        {
            DeviceUserAssignment newDua = default(DeviceUserAssignment);

            // Mark existing assignments as Unassigned
            foreach (var dua in Database.DeviceUserAssignments.Where(m => m.DeviceSerialNumber == d.SerialNumber && !m.UnassignedDate.HasValue))
                dua.UnassignedDate = DateTime.Now;

            if (u != null)
            {
                // Add new Assignment
                newDua = new DeviceUserAssignment()
                {
                    DeviceSerialNumber = d.SerialNumber,
                    AssignedUserId = u.UserId,
                    AssignedDate = DateTime.Now
                };
                Database.DeviceUserAssignments.Add(newDua);

                d.AssignedUserId = u.UserId;
                d.AssignedUser = u;
            }
            else
            {
                d.AssignedUserId = null;
            }

            // Update AD Account
            if (ActiveDirectory.IsValidDomainAccountId(d.DeviceDomainId))
            {
                var adMachineAccount = ActiveDirectory.RetrieveADMachineAccount(d.DeviceDomainId);
                if (adMachineAccount != null)
                    adMachineAccount.SetDescription(d);
            }

            return newDua;
        }

        public static ADMachineAccount ActiveDirectoryAccount(this Device Device, params string[] AdditionalProperties)
        {
            if (ActiveDirectory.IsValidDomainAccountId(Device.DeviceDomainId))
                return ActiveDirectory.RetrieveADMachineAccount(Device.DeviceDomainId, AdditionalProperties: AdditionalProperties);
            else
                return null;
        }

        public static string ReasonMessage(this Disco.Models.Repository.DecommissionReasons r)
        {
            switch (r)
            {
                case DecommissionReasons.EndOfLife:
                    return "End of Life";
                case DecommissionReasons.Sold:
                    return "Sold";
                case DecommissionReasons.Stolen:
                    return "Stolen";
                case DecommissionReasons.Lost:
                    return "Lost";
                case DecommissionReasons.Damaged:
                    return "Damaged";
                case DecommissionReasons.Donated:
                    return "Donated";
                default:
                    return "Unknown";
            }
        }

        public static string ReasonMessage(this Disco.Models.Repository.DecommissionReasons? r)
        {
            if (!r.HasValue)
                return "Not Decommissioned";

            return r.Value.ReasonMessage();
        }

        public static string StatusCode(this Device Device)
        {
            if (Device.DecommissionedDate.HasValue)
                return "Decommissioned";

            if (!Device.EnrolledDate.HasValue)
                return "NotEnrolled";

            return "Active";
        }

        public static string Status(this Device Device)
        {
            if (Device.DecommissionedDate.HasValue)
                return string.Format("Decommissioned ({0})", Device.DecommissionReason.ReasonMessage());

            if (!Device.EnrolledDate.HasValue)
                return "Not Enrolled";

            return "Active";
        }
    }
}
