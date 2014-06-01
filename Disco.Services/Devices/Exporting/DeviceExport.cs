using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Exporting;
using Disco.Services.Tasks;
using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Devices.Exporting
{
    public static class DeviceExport
    {

        public static DeviceExportResult GenerateExport(DiscoDataContext Database, IQueryable<Device> Devices, DeviceExportOptions Options, IScheduledTaskStatus TaskStatus)
        {
            TaskStatus.UpdateStatus(15, "Building metadata and database query");
            var metadata = Options.BuildMetadata();

            if (metadata.Count == 0)
                throw new ArgumentException("At least one export field must be specified", "Options");

            // Update Users
            if (Options.AssignedUserDisplayName ||
                Options.AssignedUserSurname ||
                Options.AssignedUserGivenName ||
                Options.AssignedUserPhoneNumber ||
                Options.AssignedUserEmailAddress)
            {
                TaskStatus.UpdateStatus(20, "Updating Assigned User details");
                var users = Devices.Where(d => d.AssignedUserId != null).Select(d => d.AssignedUserId).Distinct().ToList();

                users.Select((userId, index) =>
                {
                    TaskStatus.UpdateStatus(20 + (((double)20 / users.Count) * index), string.Format("Updating Assigned User details: {0}", userId));
                    try
                    {
                        return UserService.GetUser(userId, Database);
                    }
                    catch (Exception) { return null; } // Ignore Errors
                }).ToList();
            }

            // Update Last Network Logon Date
            if (Options.DeviceLastNetworkLogon)
            {
                TaskStatus.UpdateStatus(40, "Updating device last network logon dates");
                try
                {
                    TaskStatus.IgnoreCurrentProcessChanges = true;
                    TaskStatus.ProgressMultiplier = 20 / 100;
                    TaskStatus.ProgressOffset = 40;

                    Interop.ActiveDirectory.ADTaskUpdateNetworkLogonDates.UpdateLastNetworkLogonDates(Database, TaskStatus);
                    Database.SaveChanges();

                    TaskStatus.IgnoreCurrentProcessChanges = false;
                    TaskStatus.ProgressMultiplier = 1;
                    TaskStatus.ProgressOffset = 0;
                }
                catch (Exception) { } // Ignore Errors
            }

            TaskStatus.UpdateStatus(60, "Extracting records from the database");

            var records = BuildRecords(Devices).ToList();

            var stream = new MemoryStream();

            TaskStatus.UpdateStatus(80, string.Format("Formatting {0} records for export", records.Count));

            using (StreamWriter writer = new StreamWriter(stream, Encoding.Default, 0x400, true))
            {
                // Header
                writer.Write('"');
                writer.Write(string.Join("\",\"", metadata.Select(m => m.Item2)));
                writer.Write('"');

                // Records
                foreach (var record in records)
                {
                    writer.WriteLine();
                    writer.Write(string.Join(",", metadata.Select(m =>
                    {
                        var value = m.Item3(record);
                        var isString = m.Item4;

                        if (value == null)
                            return null;
                        else if (!isString)
                            return value;
                        else if (Options.ExcelCsvFormat)
                            return string.Concat("=\"", value, "\"");
                        else
                            return string.Concat("\"", value, "\"");
                    })));
                }
            }

            stream.Position = 0;
            return new DeviceExportResult()
            {
                CsvResult = stream,
                RecordCount = records.Count
            };
        }
        public static DeviceExportResult GenerateExport(DiscoDataContext Database, IQueryable<Device> Devices, DeviceExportOptions Options)
        {
            return GenerateExport(Database, Devices, Options, ScheduledTaskMockStatus.Create());
        }

        public static DeviceExportResult GenerateExport(DiscoDataContext Database, DeviceExportOptions Options, IScheduledTaskStatus TaskStatus)
        {
            switch (Options.ExportType)
            {
                case DeviceExportTypes.All:
                    return GenerateExport(Database, Database.Devices, Options, TaskStatus);
                case DeviceExportTypes.Batch:
                    return GenerateExport(Database, Database.Devices.Where(d => d.DeviceBatchId == Options.ExportTypeTargetId), Options, TaskStatus);
                case DeviceExportTypes.Model:
                    return GenerateExport(Database, Database.Devices.Where(d => d.DeviceModelId == Options.ExportTypeTargetId), Options, TaskStatus);
                case DeviceExportTypes.Profile:
                    return GenerateExport(Database, Database.Devices.Where(d => d.DeviceProfileId == Options.ExportTypeTargetId), Options, TaskStatus);
                default:
                    throw new ArgumentException(string.Format("Unknown Device Export Type", Options.ExportType.ToString()), "Options");
            }
        }
        public static DeviceExportResult GenerateExport(DiscoDataContext Database, DeviceExportOptions Options)
        {
            return GenerateExport(Database, Options, ScheduledTaskMockStatus.Create());
        }

        private static IEnumerable<DeviceExportRecord> BuildRecords(IQueryable<Device> Devices)
        {
            var deviceDetailHardwareKeys = new List<string> {
                DeviceDetail.HardwareKeyLanMacAddress,
                DeviceDetail.HardwareKeyWLanMacAddress,
                DeviceDetail.HardwareKeyACAdapter,
                DeviceDetail.HardwareKeyBattery
            };

            return Devices.Select(d => new DeviceExportRecord()
            {
                Device = d,

                DeviceDetails = d.DeviceDetails.Where(dd => dd.Scope == DeviceDetail.ScopeHardware && deviceDetailHardwareKeys.Contains(dd.Key)),

                ModelId = d.DeviceModelId,
                ModelDescription = d.DeviceModel.Description,
                ModelManufacturer = d.DeviceModel.Manufacturer,
                ModelModel = d.DeviceModel.Model,
                ModelType = d.DeviceModel.ModelType,

                BatchId = d.DeviceBatchId,
                BatchName = d.DeviceBatch.Name,
                BatchPurchaseDate = d.DeviceBatch.PurchaseDate,
                BatchSupplier = d.DeviceBatch.Supplier,
                BatchUnitCost = d.DeviceBatch.UnitCost,
                BatchWarrantyValidUntilDate = d.DeviceBatch.WarrantyValidUntil,
                BatchInsuredDate = d.DeviceBatch.InsuredDate,
                BatchInsuranceSupplier = d.DeviceBatch.InsuranceSupplier,
                BatchInsuredUntilDate = d.DeviceBatch.InsuredUntil,

                ProfileId = d.DeviceProfileId,
                ProfileName = d.DeviceProfile.Name,
                ProfileShortName = d.DeviceProfile.ShortName,

                DeviceUserAssignment = d.DeviceUserAssignments.Where(dua => dua.UnassignedDate == null).FirstOrDefault(),
                AssignedUser = d.AssignedUser,

                JobsTotalCount = d.Jobs.Count(),
                JobsOpenCount = d.Jobs.Count(j => j.ClosedDate == null),

                AttachmentsCount = d.DeviceAttachments.Count(),

                DeviceCertificate = d.DeviceCertificates.Where(dc => dc.Enabled).FirstOrDefault()
            });
        }

        /// <returns>Tuple Format: Property Name, Column Name, Property Access, Escape CSV Value?</returns>
        private static List<Tuple<string, string, Func<DeviceExportRecord, string>, bool>> BuildMetadata(this DeviceExportOptions Options)
        {
            var allAssessors = BuildRecordAssessors().ToList();

            return typeof(DeviceExportOptions).GetProperties()
                .Where(p => p.PropertyType == typeof(bool))
                .Select(p => Tuple.Create(p, (DisplayAttribute)p.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault()))
                .Where(p => p.Item2 != null && (bool)p.Item1.GetValue(Options))
                .Select(p =>
                {
                    var accessor = allAssessors.First(i => i.Item1 == p.Item1.Name);
                    var columnName = (p.Item2.ShortName == "Device" || p.Item2.ShortName == "Details") ? p.Item2.Name : string.Format("{0} {1}", p.Item2.ShortName, p.Item2.Name);
                    return Tuple.Create(p.Item1.Name, columnName, accessor.Item2, accessor.Item3);
                }).ToList();
        }

        /// <returns>Tuple Format: Property Name, Property Access, Escape CSV Value?</returns>
        private static IEnumerable<Tuple<string, Func<DeviceExportRecord, string>, bool>> BuildRecordAssessors()
        {
            const string DateFormat = "yyyy-MM-dd";
            const string DateTimeFormat = DateFormat + " HH:mm:ss";

            // Device
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("DeviceSerialNumber", r => r.Device.SerialNumber, true);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("DeviceAssetNumber", r => r.Device.AssetNumber, true);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("DeviceLocation", r => r.Device.Location, true);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("DeviceComputerName", r => r.Device.DeviceDomainId, true);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("DeviceLastNetworkLogon", r => r.Device.LastNetworkLogonDate.HasValue ? r.Device.LastNetworkLogonDate.Value.ToString(DateTimeFormat) : null, false);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("DeviceCreatedDate", r => r.Device.CreatedDate.ToString(DateTimeFormat), false);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("DeviceFirstEnrolledDate", r => r.Device.EnrolledDate.HasValue ? r.Device.EnrolledDate.Value.ToString(DateTimeFormat) : null, false);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("DeviceLastEnrolledDate", r => r.Device.LastEnrolDate.HasValue ? r.Device.LastEnrolDate.Value.ToString(DateTimeFormat) : null, false);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("DeviceAllowUnauthenticatedEnrol", r => r.Device.AllowUnauthenticatedEnrol.ToString(), false);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("DeviceDecommissionedDate", r => r.Device.DecommissionedDate.HasValue ? r.Device.DecommissionedDate.Value.ToString(DateTimeFormat) : null, false);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("DeviceDecommissionedReason", r => r.Device.DecommissionReason.HasValue ? r.Device.DecommissionReason.Value.ToString() : null, true);

            // Details
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("DetailLanMacAddress", r => r.DeviceDetails.Where(dd => dd.Key == DeviceDetail.HardwareKeyLanMacAddress).Select(dd => dd.Value).FirstOrDefault(), true);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("DetailWLanMacAddress", r => r.DeviceDetails.Where(dd => dd.Key == DeviceDetail.HardwareKeyWLanMacAddress).Select(dd => dd.Value).FirstOrDefault(), true);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("DetailACAdapter", r => r.DeviceDetails.Where(dd => dd.Key == DeviceDetail.HardwareKeyACAdapter).Select(dd => dd.Value).FirstOrDefault(), true);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("DetailBattery", r => r.DeviceDetails.Where(dd => dd.Key == DeviceDetail.HardwareKeyBattery).Select(dd => dd.Value).FirstOrDefault(), true);

            // Model
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("ModelId", r => r.ModelId.HasValue ? r.ModelId.Value.ToString() : null, false);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("ModelDescription", r => r.ModelDescription, true);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("ModelManufacturer", r => r.ModelManufacturer, true);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("ModelModel", r => r.ModelModel, true);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("ModelType", r => r.ModelType, true);

            // Batch
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("BatchId", r => r.BatchId.HasValue ? r.BatchId.Value.ToString() : null, false);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("BatchName", r => r.BatchName, true);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("BatchPurchaseDate", r => r.BatchPurchaseDate.HasValue ? r.BatchPurchaseDate.Value.ToString(DateFormat) : null, false);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("BatchSupplier", r => r.BatchSupplier, true);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("BatchUnitCost", r => r.BatchUnitCost.HasValue ? r.BatchUnitCost.ToString() : null, false);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("BatchWarrantyValidUntilDate", r => r.BatchWarrantyValidUntilDate.HasValue ? r.BatchWarrantyValidUntilDate.Value.ToString(DateFormat) : null, false);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("BatchInsuredDate", r => r.BatchInsuredDate.HasValue ? r.BatchInsuredDate.Value.ToString(DateFormat) : null, false);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("BatchInsuranceSupplier", r => r.BatchInsuranceSupplier, true);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("BatchInsuredUntilDate", r => r.BatchInsuredUntilDate.HasValue ? r.BatchInsuredUntilDate.Value.ToString(DateFormat) : null, false);

            // Profile
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("ProfileId", r => r.ProfileId.ToString(), false);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("ProfileName", r => r.ProfileName, true);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("ProfileShortName", r => r.ProfileShortName, true);

            // User
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("AssignedUserId", r => r.AssignedUser != null ? r.AssignedUser.UserId : null, true);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("AssignedUserDate", r => r.DeviceUserAssignment != null ? r.DeviceUserAssignment.AssignedDate.ToString(DateTimeFormat) : null, false);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("AssignedUserDisplayName", r => r.AssignedUser != null ? r.AssignedUser.DisplayName : null, true);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("AssignedUserSurname", r => r.AssignedUser != null ? r.AssignedUser.Surname : null, true);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("AssignedUserGivenName", r => r.AssignedUser != null ? r.AssignedUser.GivenName : null, true);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("AssignedUserPhoneNumber", r => r.AssignedUser != null ? r.AssignedUser.PhoneNumber : null, true);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("AssignedUserEmailAddress", r => r.AssignedUser != null ? r.AssignedUser.EmailAddress : null, true);

            // Jobs
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("JobsTotalCount", r => r.JobsTotalCount.ToString(), false);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("JobsOpenCount", r => r.JobsOpenCount.ToString(), false);

            // Attachments
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("AttachmentsCount", r => r.AttachmentsCount.ToString(), false);

            // Certificates
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("CertificateName", r => r.DeviceCertificate != null ? r.DeviceCertificate.Name : null, true);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("CertificateAllocatedDate", r => r.DeviceCertificate != null && r.DeviceCertificate.AllocatedDate.HasValue ? r.DeviceCertificate.AllocatedDate.Value.ToString(DateTimeFormat) : null, false);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("CertificateExpirationDate", r => r.DeviceCertificate != null && r.DeviceCertificate.ExpirationDate.HasValue ? r.DeviceCertificate.ExpirationDate.Value.ToString(DateTimeFormat) : null, false);
            yield return new Tuple<string, Func<DeviceExportRecord, string>, bool>("CertificateProviderId", r => r.DeviceCertificate != null ? r.DeviceCertificate.ProviderId : null, true);
        }

    }
}
