using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Documents;
using Disco.Services.Authorization;
using Disco.Services.Documents;
using Disco.Services.Expressions;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Logging;
using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services
{
    public static class DeviceExtensions
    {
        public static string ComputerNameRender(this Device device, DiscoDataContext Database, ADDomain Domain)
        {
            if (Domain == null)
                throw new ArgumentNullException("Domain");

            var deviceProfile = device.DeviceProfile;
            Expression computerNameTemplateExpression = null;
            computerNameTemplateExpression = ExpressionCache.GetOrCreateSingleExpressions(string.Format(DeviceProfileExtensions.ComputerNameExpressionCacheTemplate, deviceProfile.Id),
                () => Expression.TokenizeSingleDynamic(null, deviceProfile.ComputerNameTemplate, 0));
            var evaluatorVariables = Expression.StandardVariables(null, Database, UserService.CurrentUser, DateTime.Now, null, device);
            string rendered;
            try
            {
                rendered = computerNameTemplateExpression.EvaluateFirst<string>(device, evaluatorVariables);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred rendering the computer name: [{ex.GetType().Name}] {ex.Message}", ex.InnerException);
            }
            if (rendered == null || rendered.Length > 24)
            {
                throw new InvalidOperationException("The rendered computer name would be invalid or longer than 24 characters");
            }

            return $@"{Domain.NetBiosName}\{rendered}";
        }
        public static List<DocumentTemplate> AvailableDocumentTemplates(this Device d, DiscoDataContext Database, User User, DateTime TimeStamp)
        {
            List<DocumentTemplate> ats = Database.DocumentTemplates
                .Where(at => !at.IsHidden && at.Scope == DocumentTemplate.DocumentTemplateScopes.Device).ToList();

            return ats.Where(at => at.FilterExpressionMatches(d, Database, User, TimeStamp, DocumentState.DefaultState())).ToList();
        }
        public static List<DocumentTemplatePackage> AvailableDocumentTemplatePackages(this Device d, DiscoDataContext Database, User TechnicianUser)
        {
            return DocumentTemplatePackages.AvailablePackages(d, Database, TechnicianUser);
        }

        public static bool UpdateLastNetworkLogonDate(this Device Device)
        {
            return ADNetworkLogonDatesUpdateTask.UpdateLastNetworkLogonDate(Device);
        }

        public static Device AddOffline(this Device d, DiscoDataContext Database)
        {
            // Just Include:
            // - Serial Number
            // - Device Domain Id
            // - Asset Number
            // - Profile Id
            // - Assigned User Id
            // - Batch

            if (d.SerialNumber.Contains("/") || d.SerialNumber.Contains(@"\"))
                throw new ArgumentException(@"The device serial number cannot contain '/' or '\' characters.", nameof(d));

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
            var db = default(DeviceBatch);
            if (d.DeviceBatchId.HasValue)
                db = Database.DeviceBatches.Find(d.DeviceBatchId.Value);

            // Default Device Model
            var dm = default(DeviceModel);
            if (db != null && db.DefaultDeviceModelId.HasValue)
                dm = Database.DeviceModels.Find(db.DefaultDeviceModelId); // From Batch
            else
                dm = Database.DeviceModels.Find(1); // Default

            Device d2 = new Device()
            {
                SerialNumber = d.SerialNumber.ToUpper(),
                DeviceDomainId = d.DeviceDomainId,
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

        public static void Unassign(this DeviceUserAssignment dua, DiscoDataContext Database)
        {
            // Entity Framework 5 Bug
            //  all dates are handled as datetime2 which do not match the
            //  datetime type of the AssignedDate key
            using (var tempCommand = Database.Database.Connection.CreateCommand())
            {
                var paramDeviceSerialNumber = tempCommand.CreateParameter();
                paramDeviceSerialNumber.DbType = System.Data.DbType.String;
                paramDeviceSerialNumber.Direction = System.Data.ParameterDirection.Input;
                paramDeviceSerialNumber.ParameterName = "@DeviceSerialNumber";
                paramDeviceSerialNumber.Size = 60;
                paramDeviceSerialNumber.Value = dua.DeviceSerialNumber;

                var paramAssignedDate = tempCommand.CreateParameter();
                paramAssignedDate.DbType = System.Data.DbType.DateTime;
                paramAssignedDate.Direction = System.Data.ParameterDirection.Input;
                paramAssignedDate.ParameterName = "@AssignedDate";
                paramAssignedDate.Value = dua.AssignedDate;

                var paramUnassignedDate = tempCommand.CreateParameter();
                paramUnassignedDate.DbType = System.Data.DbType.DateTime;
                paramUnassignedDate.Direction = System.Data.ParameterDirection.Input;
                paramUnassignedDate.ParameterName = "@UnassignedDate";
                paramUnassignedDate.Value = DateTime.Now;

                Database.Database.ExecuteSqlCommand(
                    "update [dbo].[DeviceUserAssignments] set[UnassignedDate] = @UnassignedDate where(([DeviceSerialNumber] = @DeviceSerialNumber) and([AssignedDate] = @AssignedDate))",
                    paramUnassignedDate, paramDeviceSerialNumber, paramAssignedDate
                );
            }
        }

        public static DeviceUserAssignment AssignDevice(this Device d, DiscoDataContext Database, User u)
        {
            DeviceUserAssignment newDua = default;

            // Mark existing assignments as Unassigned
            foreach (var dua in Database.DeviceUserAssignments.Where(m => m.DeviceSerialNumber == d.SerialNumber && !m.UnassignedDate.HasValue))
            {
                dua.Unassign(Database);
            }

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
                {
                    try
                    {
                        adMachineAccount.SetDescription(d);
                    }
                    catch (Exception ex)
                    {
                        SystemLog.LogWarning($"Unable to update AD Machine Account Description for {d.DeviceDomainId}: {ex.Message}");
                    }
                }
            }

            return newDua;
        }

        public static string ReasonMessage(this DecommissionReasons r)
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
                case DecommissionReasons.Returned:
                    return "Returned";
                default:
                    return "Unknown";
            }
        }

        public static string ReasonMessage(this DecommissionReasons? r)
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
                return $"Decommissioned ({Device.DecommissionReason.ReasonMessage()})";

            if (!Device.EnrolledDate.HasValue)
                return "Not Enrolled";

            return "Active";
        }
    }
}
