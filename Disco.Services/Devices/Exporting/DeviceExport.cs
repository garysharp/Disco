using ClosedXML.Excel;
using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Exporting;
using Disco.Services.Tasks;
using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

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
                    TaskStatus.ProgressMultiplier = (double)20 / 100;
                    TaskStatus.ProgressOffset = 40;

                    Interop.ActiveDirectory.ADNetworkLogonDatesUpdateTask.UpdateLastNetworkLogonDates(Database, TaskStatus);
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

            if (Options.ExcelFormat)
            {
                WriteXlsx(stream, metadata, records);
            }
            else
            {
                WriteCSV(stream, metadata, records);
            }

            stream.Position = 0;
            return new DeviceExportResult()
            {
                Result = stream,
                RecordCount = records.Count
            };
        }

        public static DeviceExportResult GenerateExport(DiscoDataContext Database, IQueryable<Device> Devices, DeviceExportOptions Options)
        {
            return GenerateExport(Database, Devices, Options, ScheduledTaskMockStatus.Create("Device Export"));
        }

        public static DeviceExportResult GenerateExport(DiscoDataContext Database, DeviceExportOptions Options, IScheduledTaskStatus TaskStatus)
        {
            switch (Options.ExportType)
            {
                case DeviceExportTypes.All:
                    return GenerateExport(Database, Database.Devices, Options, TaskStatus);
                case DeviceExportTypes.Batch:
                    if (Options.ExportTypeTargetId.HasValue && Options.ExportTypeTargetId.Value > 0)
                        return GenerateExport(Database, Database.Devices.Where(d => d.DeviceBatchId == Options.ExportTypeTargetId), Options, TaskStatus);
                    else
                        return GenerateExport(Database, Database.Devices.Where(d => d.DeviceBatchId == null), Options, TaskStatus);
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
            return GenerateExport(Database, Options, ScheduledTaskMockStatus.Create("Device Export"));
        }

        private static void WriteCSV(Stream OutputStream, List<DeviceExportFieldMetadata> Metadata, List<DeviceExportRecord> Records)
        {
            using (StreamWriter writer = new StreamWriter(OutputStream, Encoding.Default, 0x400, true))
            {
                // Header
                writer.Write('"');
                writer.Write(string.Join("\",\"", Metadata.Select(m => m.ColumnName)));
                writer.Write('"');

                // Records
                foreach (var record in Records)
                {
                    writer.WriteLine();
                    for (int i = 0; i < Metadata.Count; i++)
                    {
                        if (i != 0)
                        {
                            writer.Write(',');
                        }
                        var value = Metadata[i].Accessor(record);
                        writer.Write(Metadata[i].CsvEncoder(value));
                    }
                }
            }
        }

        private static void WriteXlsx(Stream OutputStream, List<DeviceExportFieldMetadata> Metadata, List<DeviceExportRecord> Records)
        {
            // Create DataTable
            var dataTable = new DataTable();
            foreach (var field in Metadata)
            {
                dataTable.Columns.Add(field.ColumnName, field.ValueType);
            }
            foreach (var record in Records)
            {
                dataTable.Rows.Add(Metadata.Select(m => m.Accessor(record)).ToArray());
            }

            using (var xlWorkbook = new XLWorkbook())
            {
                var sheet = xlWorkbook.AddWorksheet("DeviceExport");
                var table = sheet.Cell(1, 1).InsertTable(dataTable, "Devices");
                table.Theme = XLTableTheme.TableStyleMedium2;

                table.Columns().ForEach(c => c.WorksheetColumn().AdjustToContents(2, 15, 30));

                xlWorkbook.SaveAs(OutputStream);
            }
        }

        private static IEnumerable<DeviceExportRecord> BuildRecords(IQueryable<Device> Devices)
        {
            var deviceDetailHardwareKeys = new List<string> {
                DeviceDetail.HardwareKeyLanMacAddress,
                DeviceDetail.HardwareKeyWLanMacAddress,
                DeviceDetail.HardwareKeyACAdapter,
                DeviceDetail.HardwareKeyBattery,
                DeviceDetail.HardwareKeyKeyboard
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

        private static List<DeviceExportFieldMetadata> BuildMetadata(this DeviceExportOptions Options)
        {
            var allAssessors = BuildRecordAccessors().ToList();

            return typeof(DeviceExportOptions).GetProperties()
                .Where(p => p.PropertyType == typeof(bool))
                .Select(p => new
                {
                    property = p,
                    details = (DisplayAttribute)p.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault()
                })
                .Where(p => p.details != null && (bool)p.property.GetValue(Options))
                .Select(p =>
                {
                    var fieldMetadata = allAssessors.First(i => i.Name == p.property.Name);
                    fieldMetadata.ColumnName = (p.details.ShortName == "Device" || p.details.ShortName == "Details") ? p.details.Name : $"{p.details.ShortName} {p.details.Name}";
                    return fieldMetadata;
                }).ToList();
        }

        private static IEnumerable<DeviceExportFieldMetadata> BuildRecordAccessors()
        {
            const string DateFormat = "yyyy-MM-dd";
            const string DateTimeFormat = DateFormat + " HH:mm:ss";

            Func<object, string> csvStringEncoded = (o) => o == null ? null : $"\"{((string)o).Replace("\"", "\"\"")}\"";
            Func<object, string> csvToStringEncoded = (o) => o == null ? null : o.ToString();
            Func<object, string> csvCurrencyEncoded = (o) => ((decimal?)o).HasValue ? ((decimal?)o).Value.ToString("C") : null;
            Func<object, string> csvDateEncoded = (o) => ((DateTime)o).ToString(DateFormat);
            Func<object, string> csvDateTimeEncoded = (o) => ((DateTime)o).ToString(DateTimeFormat);
            Func<object, string> csvNullableDateEncoded = (o) => ((DateTime?)o).HasValue ? csvDateEncoded(o) : null;
            Func<object, string> csvNullableDateTimeEncoded = (o) => ((DateTime?)o).HasValue ? csvDateTimeEncoded(o) : null;

            // Device
            yield return new DeviceExportFieldMetadata("DeviceSerialNumber", typeof(string), r => r.Device.SerialNumber, csvStringEncoded);
            yield return new DeviceExportFieldMetadata("DeviceAssetNumber", typeof(string), r => r.Device.AssetNumber, csvStringEncoded);
            yield return new DeviceExportFieldMetadata("DeviceLocation", typeof(string), r => r.Device.Location, csvStringEncoded);
            yield return new DeviceExportFieldMetadata("DeviceComputerName", typeof(string), r => r.Device.DeviceDomainId, csvStringEncoded);
            yield return new DeviceExportFieldMetadata("DeviceLastNetworkLogon", typeof(DateTime), r => r.Device.LastNetworkLogonDate, csvNullableDateTimeEncoded);
            yield return new DeviceExportFieldMetadata("DeviceCreatedDate", typeof(DateTime), r => r.Device.CreatedDate, csvDateTimeEncoded);
            yield return new DeviceExportFieldMetadata("DeviceFirstEnrolledDate", typeof(DateTime), r => r.Device.EnrolledDate, csvNullableDateTimeEncoded);
            yield return new DeviceExportFieldMetadata("DeviceLastEnrolledDate", typeof(DateTime), r => r.Device.LastEnrolDate, csvNullableDateTimeEncoded);
            yield return new DeviceExportFieldMetadata("DeviceAllowUnauthenticatedEnrol", typeof(bool), r => r.Device.AllowUnauthenticatedEnrol, csvToStringEncoded);
            yield return new DeviceExportFieldMetadata("DeviceDecommissionedDate", typeof(DateTime), r => r.Device.DecommissionedDate, csvNullableDateTimeEncoded);
            yield return new DeviceExportFieldMetadata("DeviceDecommissionedReason", typeof(string), r => r.Device.DecommissionReason, csvToStringEncoded);

            // Details
            yield return new DeviceExportFieldMetadata("DetailLanMacAddress", typeof(string), r => r.DeviceDetails.Where(dd => dd.Key == DeviceDetail.HardwareKeyLanMacAddress).Select(dd => dd.Value).FirstOrDefault(), csvStringEncoded);
            yield return new DeviceExportFieldMetadata("DetailWLanMacAddress", typeof(string), r => r.DeviceDetails.Where(dd => dd.Key == DeviceDetail.HardwareKeyWLanMacAddress).Select(dd => dd.Value).FirstOrDefault(), csvStringEncoded);
            yield return new DeviceExportFieldMetadata("DetailACAdapter", typeof(string), r => r.DeviceDetails.Where(dd => dd.Key == DeviceDetail.HardwareKeyACAdapter).Select(dd => dd.Value).FirstOrDefault(), csvStringEncoded);
            yield return new DeviceExportFieldMetadata("DetailBattery", typeof(string), r => r.DeviceDetails.Where(dd => dd.Key == DeviceDetail.HardwareKeyBattery).Select(dd => dd.Value).FirstOrDefault(), csvStringEncoded);
            yield return new DeviceExportFieldMetadata("DetailKeyboard", typeof(string), r => r.DeviceDetails.Where(dd => dd.Key == DeviceDetail.HardwareKeyKeyboard).Select(dd => dd.Value).FirstOrDefault(), csvStringEncoded);

            // Model
            yield return new DeviceExportFieldMetadata("ModelId", typeof(int), r => r.ModelId, csvToStringEncoded);
            yield return new DeviceExportFieldMetadata("ModelDescription", typeof(string), r => r.ModelDescription, csvStringEncoded);
            yield return new DeviceExportFieldMetadata("ModelManufacturer", typeof(string), r => r.ModelManufacturer, csvStringEncoded);
            yield return new DeviceExportFieldMetadata("ModelModel", typeof(string), r => r.ModelModel, csvStringEncoded);
            yield return new DeviceExportFieldMetadata("ModelType", typeof(string), r => r.ModelType, csvStringEncoded);

            // Batch
            yield return new DeviceExportFieldMetadata("BatchId", typeof(int), r => r.BatchId, csvToStringEncoded);
            yield return new DeviceExportFieldMetadata("BatchName", typeof(string), r => r.BatchName, csvStringEncoded);
            yield return new DeviceExportFieldMetadata("BatchPurchaseDate", typeof(DateTime), r => r.BatchPurchaseDate, csvNullableDateEncoded);
            yield return new DeviceExportFieldMetadata("BatchSupplier", typeof(string), r => r.BatchSupplier, csvStringEncoded);
            yield return new DeviceExportFieldMetadata("BatchUnitCost", typeof(decimal), r => r.BatchUnitCost, csvCurrencyEncoded);
            yield return new DeviceExportFieldMetadata("BatchWarrantyValidUntilDate", typeof(DateTime), r => r.BatchWarrantyValidUntilDate, csvNullableDateEncoded);
            yield return new DeviceExportFieldMetadata("BatchInsuredDate", typeof(DateTime), r => r.BatchInsuredDate, csvNullableDateEncoded);
            yield return new DeviceExportFieldMetadata("BatchInsuranceSupplier", typeof(string), r => r.BatchInsuranceSupplier, csvStringEncoded);
            yield return new DeviceExportFieldMetadata("BatchInsuredUntilDate", typeof(DateTime), r => r.BatchInsuredUntilDate, csvNullableDateEncoded);

            // Profile
            yield return new DeviceExportFieldMetadata("ProfileId", typeof(int), r => r.ProfileId, csvToStringEncoded);
            yield return new DeviceExportFieldMetadata("ProfileName", typeof(string), r => r.ProfileName, csvStringEncoded);
            yield return new DeviceExportFieldMetadata("ProfileShortName", typeof(string), r => r.ProfileShortName, csvStringEncoded);

            // User
            yield return new DeviceExportFieldMetadata("AssignedUserId", typeof(string), r => r.AssignedUser?.UserId, csvStringEncoded);
            yield return new DeviceExportFieldMetadata("AssignedUserDate", typeof(DateTime), r => r.DeviceUserAssignment?.AssignedDate, csvNullableDateTimeEncoded);
            yield return new DeviceExportFieldMetadata("AssignedUserDisplayName", typeof(string), r => r.AssignedUser?.DisplayName, csvStringEncoded);
            yield return new DeviceExportFieldMetadata("AssignedUserSurname", typeof(string), r => r.AssignedUser?.Surname, csvStringEncoded);
            yield return new DeviceExportFieldMetadata("AssignedUserGivenName", typeof(string), r => r.AssignedUser?.GivenName, csvStringEncoded);
            yield return new DeviceExportFieldMetadata("AssignedUserPhoneNumber", typeof(string), r => r.AssignedUser?.PhoneNumber, csvStringEncoded);
            yield return new DeviceExportFieldMetadata("AssignedUserEmailAddress", typeof(string), r => r.AssignedUser?.EmailAddress, csvStringEncoded);

            // Jobs
            yield return new DeviceExportFieldMetadata("JobsTotalCount", typeof(int), r => r.JobsTotalCount, csvToStringEncoded);
            yield return new DeviceExportFieldMetadata("JobsOpenCount", typeof(int), r => r.JobsOpenCount, csvToStringEncoded);

            // Attachments
            yield return new DeviceExportFieldMetadata("AttachmentsCount", typeof(int), r => r.AttachmentsCount, csvToStringEncoded);

            // Certificates
            yield return new DeviceExportFieldMetadata("CertificateName", typeof(string), r => r.DeviceCertificate?.Name, csvStringEncoded);
            yield return new DeviceExportFieldMetadata("CertificateAllocatedDate", typeof(DateTime), r => r.DeviceCertificate?.AllocatedDate, csvNullableDateTimeEncoded);
            yield return new DeviceExportFieldMetadata("CertificateExpirationDate", typeof(DateTime), r => r.DeviceCertificate?.ExpirationDate, csvNullableDateTimeEncoded);
            yield return new DeviceExportFieldMetadata("CertificateProviderId", typeof(string), r => r.DeviceCertificate?.ProviderId, csvStringEncoded);
        }

    }
}
