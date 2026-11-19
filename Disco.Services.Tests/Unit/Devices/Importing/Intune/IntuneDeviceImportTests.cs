using Disco.Services.Devices.Importing.Intune;
using Disco.Services.Extensions;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace Disco.Services.Tests.Unit.Devices.Importing.Intune
{
    public class IntuneCsvDeviceRecordTests
    {
        [Fact]
        public void Parse_WithIntuneLikeCsv_MapsExpectedFields()
        {
            var csv = BuildIntuneLikeCsv("242767", "152631");

            IntuneCsvDeviceRecord record;
            using (var stream = CreateMemoryStream(csv))
            {
                record = Csv.Parse<IntuneCsvDeviceRecord>(stream).Single();
            }

            Assert.Equal("dev-001", record.DeviceId);
            Assert.Equal("LAB-LAPTOP-001", record.DeviceName);
            Assert.Equal("SN-A1B2C3D4", record.SerialNumber);
            Assert.Equal("Contoso", record.Manufacturer);
            Assert.Equal("Model-14", record.Model);
            Assert.Equal("AA11BB22CC33", record.WiFiMac);
            Assert.Equal(242767L, record.TotalStorage);
            Assert.Equal(152631L, record.FreeStorage);
            Assert.Equal("student001@example.edu", record.PrimaryUserUpn);
            Assert.Equal("Windows", record.OS);
            Assert.Equal("ProEducation", record.SkuFamily);
            Assert.Equal("DD44EE55FF66", record.EthernetMac);
            Assert.Equal("X64", record.ProcessorArchitecture);
        }

        [Fact]
        public void Parse_WithIntuneLikeDateFormat_ParsesDateTimes()
        {
            var csv = BuildIntuneLikeCsv("242767", "152631");

            IntuneCsvDeviceRecord record;
            using (var stream = CreateMemoryStream(csv))
            {
                record = Csv.Parse<IntuneCsvDeviceRecord>(stream).Single();
            }

            var enrolmentUtc = record.EnrolmentDate.ToUniversalTime();
            Assert.Equal(2026, enrolmentUtc.Year);
            Assert.Equal(8, enrolmentUtc.Month);
            Assert.Equal(5, enrolmentUtc.Day);
            Assert.Equal(14, enrolmentUtc.Hour);
            Assert.Equal(18, enrolmentUtc.Minute);
            Assert.Equal(38, enrolmentUtc.Second);

            var checkInUtc = record.LastCheckIn.ToUniversalTime();
            Assert.Equal(2026, checkInUtc.Year);
            Assert.Equal(10, checkInUtc.Month);
            Assert.Equal(24, checkInUtc.Day);
            Assert.Equal(21, checkInUtc.Hour);
            Assert.Equal(17, checkInUtc.Minute);
            Assert.Equal(13, checkInUtc.Second);
        }

        private static MemoryStream CreateMemoryStream(string csv)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(csv));
        }

        private static string BuildIntuneLikeCsv(string totalStorage, string freeStorage)
        {
            return "Device ID,Device name,Enrollment date,Last check-in,OS version,Serial number,Manufacturer,Model,Wi-Fi MAC,Total storage,Free storage,Primary user UPN,OS,SkuFamily,EthernetMAC,ProcessorArchitecture\r\n" +
                   $"dev-001,LAB-LAPTOP-001,2026-08-05 14:18:38.0000000,2026-10-24 21:17:13.4425533,10.0.26300.9711,SN-A1B2C3D4,Contoso,Model-14,AA11BB22CC33,{totalStorage},{freeStorage},student001@example.edu,Windows,ProEducation,DD44EE55FF66,X64\r\n";
        }
    }
}
