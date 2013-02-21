using System.Linq;
using Disco.BI.Interop.ActiveDirectory;
using Disco.Data.Configuration;
using Disco.Data.Repository;
using Disco.Models.BI.DocumentTemplates;
using Disco.Models.Repository;
using System.Collections.Generic;
using System;
using System.IO;
using Disco.Models.Interop.ActiveDirectory;

namespace Disco.BI.Extensions
{
    public static class DeviceExtensions
    {

        public static string ComputerNameRender(this Device device, DiscoDataContext context)
        {
            DeviceProfile deviceProfile = device.DeviceProfile;
            Expressions.Expression computerNameTemplateExpression = null;
            computerNameTemplateExpression = Expressions.ExpressionCache.GetValue(DeviceProfileExtensions.ComputerNameExpressionCacheModule, deviceProfile.Id.ToString(), () =>
            {
                // Removed 2012-06-14 G# - Properties moved to DeviceProfile model & DB Migrated in DBv3.
                //return Expressions.Expression.TokenizeSingleDynamic(null, deviceProfile.Configuration(context).ComputerNameTemplate, 0);
                return Expressions.Expression.TokenizeSingleDynamic(null, deviceProfile.ComputerNameTemplate, 0);
            });
            System.Collections.IDictionary evaluatorVariables = Expressions.Expression.StandardVariables(null, context, UserBI.UserCache.CurrentUser, System.DateTime.Now, null);
            string rendered;
            try
            {
                rendered = computerNameTemplateExpression.EvaluateFirst<string>(device, evaluatorVariables);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("An error occurred rendering the computer name: [{0}] {1}", ex.GetType().Name, ex.Message), ex.InnerException);
            }
            if (rendered == null || rendered.Length > 24)
            {
                throw new System.InvalidOperationException("The rendered computer name would be invalid or longer than 24 characters");
            }
            return rendered.ToString();
        }
        public static System.Collections.Generic.List<DocumentTemplate> AvailableDocumentTemplates(this Device d, DiscoDataContext Context, User User, System.DateTime TimeStamp)
        {
            List<DocumentTemplate> ats = Context.DocumentTemplates
                .Where(at => at.Scope == Disco.Models.Repository.DocumentTemplate.DocumentTemplateScopes.Device).ToList();

            return ats.Where(at => at.FilterExpressionMatches(d, Context, User, TimeStamp, DocumentState.DefaultState())).ToList();
        }

        public static bool UpdateLastNetworkLogonDate(this Device Device)
        {
            return ActiveDirectoryUpdateLastNetworkLogonDateJob.UpdateLastNetworkLogonDate(Device);
        }

        public static DeviceAttachment CreateAttachment(this Device Device, DiscoDataContext dbContext, User CreatorUser, string Filename, string MimeType, string Comments, Stream Content, DocumentTemplate DocumentTemplate = null, byte[] PdfThumbnail = null)
        {
            if (string.IsNullOrEmpty(MimeType) || MimeType.Equals("unknown/unknown", StringComparison.InvariantCultureIgnoreCase))
                MimeType = Interop.MimeTypes.ResolveMimeType(Filename);

            DeviceAttachment da = new DeviceAttachment()
            {
                DeviceSerialNumber = Device.SerialNumber,
                TechUserId = CreatorUser.Id,
                Filename = Filename,
                MimeType = MimeType,
                Timestamp = DateTime.Now,
                Comments = Comments
            };

            if (DocumentTemplate != null)
                da.DocumentTemplateId = DocumentTemplate.Id;

            dbContext.DeviceAttachments.Add(da);
            dbContext.SaveChanges();

            da.SaveAttachment(dbContext, Content);
            Content.Position = 0;
            if (PdfThumbnail == null)
                da.GenerateThumbnail(dbContext, Content);
            else
                da.SaveThumbnailAttachment(dbContext, PdfThumbnail);

            return da;
        }

        public static Device AddOffline(this Device d, DiscoDataContext dbContext)
        {
            // Just Include:
            // - Serial Number
            // - Asset Number
            // - Profile Id
            // - Assigned User Id
            // - Batch

            // Batch
            DeviceBatch db = default(DeviceBatch);
            if (d.DeviceBatchId.HasValue)
                db = dbContext.DeviceBatches.Find(d.DeviceBatchId.Value);

            // Default Device Model
            DeviceModel dm = default(DeviceModel);
            if (db != null && db.DefaultDeviceModelId.HasValue)
                dm = dbContext.DeviceModels.Find(db.DefaultDeviceModelId); // From Batch
            else
                dm = dbContext.DeviceModels.Find(1); // Default

            Device d2 = new Device()
            {
                SerialNumber = d.SerialNumber.ToUpper(),
                AssetNumber = d.AssetNumber,
                Location = d.Location,
                CreatedDate = DateTime.Now,
                DeviceProfileId = d.DeviceProfileId,
                DeviceProfile = dbContext.DeviceProfiles.Find(d.DeviceProfileId),
                AllowUnauthenticatedEnrol = true,
                DeviceModelId = dm.Id,
                DeviceModel = dm,
                DeviceBatchId = d.DeviceBatchId,
                DeviceBatch = db
            };

            dbContext.Devices.Add(d2);
            if (!string.IsNullOrEmpty(d.AssignedUserId))
            {
                User u = UserBI.UserCache.GetUser(d.AssignedUserId, dbContext);
                d2.AssignDevice(dbContext, u);
            }

            return d2;
        }

        public static DeviceUserAssignment AssignDevice(this Device d, DiscoDataContext dbContext, User u)
        {
            DeviceUserAssignment newDua = default(DeviceUserAssignment);

            // Mark existing assignments as Unassigned
            foreach (var dua in dbContext.DeviceUserAssignments.Where(m => m.DeviceSerialNumber == d.SerialNumber && !m.UnassignedDate.HasValue))
                dua.UnassignedDate = DateTime.Now;

            if (u != null)
            {
                // Add new Assignment
                newDua = new DeviceUserAssignment()
                {
                    DeviceSerialNumber = d.SerialNumber,
                    AssignedUserId = u.Id,
                    AssignedDate = DateTime.Now
                };
                dbContext.DeviceUserAssignments.Add(newDua);
                
                d.AssignedUserId = u.Id;
                d.AssignedUser = u;
            }
            else
            {
                d.AssignedUserId = null;
            }

            // Update AD Account
            if (!string.IsNullOrEmpty(d.ComputerName) && d.ComputerName.Length <= 24)
            {
                var adMachineAccount = Interop.ActiveDirectory.ActiveDirectory.GetMachineAccount(d.ComputerName);
                if (adMachineAccount != null)
                {
                    if (newDua == null)
                        adMachineAccount.SetDescription(string.Empty);
                    else
                        adMachineAccount.SetDescription(d);
                }
            }

            return newDua;
        }

        public static ActiveDirectoryMachineAccount ActiveDirectoryAccount(this Device Device, params string[] AdditionalProperties)
        {
            if (!string.IsNullOrEmpty(Device.ComputerName))
                return Interop.ActiveDirectory.ActiveDirectory.GetMachineAccount(Device.ComputerName, AdditionalProperties: AdditionalProperties);
            else
                return null;
        }

    }
}
