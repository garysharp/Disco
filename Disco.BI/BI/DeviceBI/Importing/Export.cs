using Disco.Models.BI.Device;
using Disco.Models.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.BI.DeviceBI.Importing
{
    public static class Export
    {
        private const string ExportHeader = "Serial Number,Device Model,Device Profile,Device Batch,Assigned User,Location,Asset Number";

        public static MemoryStream GenerateExport(IQueryable<Device> Devices)
        {
            var devices = Devices.Select(d => new ImportDevice()
            {
                SerialNumber = d.SerialNumber,
                DeviceModelId = d.DeviceModelId,
                DeviceProfileId = d.DeviceProfileId,
                DeviceBatchId = d.DeviceBatchId,
                AssignedUserId = d.AssignedUserId,
                Location = d.Location,
                AssetNumber = d.AssetNumber
            });

            MemoryStream exportStream = new MemoryStream();

            StreamWriter exportWriter = new StreamWriter(exportStream);
            // Write Header
            exportWriter.WriteLine(ExportHeader);

            foreach (var device in devices)
                device.ExportCsv(exportWriter);
            
            exportWriter.Flush();

            exportStream.Position = 0;
            return exportStream;
        }

        private static void ExportCsv(this ImportDevice device, StreamWriter writer)
        {
            // SERIAL NUMBER
            writer.Write('"');
            writer.Write(device.SerialNumber.Replace("\"", "\"\""));
            writer.Write('"');

            writer.Write(',');

            // DEVICE MODEL
            if (device.DeviceModelId.HasValue)
                writer.Write(device.DeviceModelId.Value);

            writer.Write(',');

            // DEVICE PROFILE
            writer.Write(device.DeviceProfileId);

            writer.Write(',');

            // DEVICE BATCH
            if (device.DeviceBatchId.HasValue)
                writer.Write(device.DeviceBatchId.Value);

            writer.Write(',');

            // ASSIGNED USER
            if (device.AssignedUserId != null)
            {
                writer.Write('"');
                writer.Write(device.AssignedUserId.Replace("\"", "\"\""));
                writer.Write('"');
            }

            writer.Write(',');

            // LOCATION
            if (!string.IsNullOrWhiteSpace(device.Location))
            {
                writer.Write('"');
                writer.Write(device.Location.Replace("\"", "\"\""));
                writer.Write('"');
            }

            writer.Write(',');

            // ASSET NUMBER
            if (!string.IsNullOrWhiteSpace(device.AssetNumber))
            {
                writer.Write('"');
                writer.Write(device.AssetNumber.Replace("\"", "\"\""));
                writer.Write('"');
            }

            writer.WriteLine();
        }

    }
}
