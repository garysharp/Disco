using Disco.Data.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services.Devices.Importing.Intune;
using Disco.Services.Tasks;
using Effort;
using System;
using System.Data.Common;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Transactions;
using Xunit;

namespace Disco.Services.Tests.Integration.Devices.Importing.Intune
{
    public abstract class IntegrationTestScope : IDisposable
    {
        private readonly EffortDiscoDataContextFactory factory;
        private readonly IDisposable factoryScope;
        private readonly TransactionScope transaction;

        protected IntegrationTestScope()
        {
            factory = new EffortDiscoDataContextFactory();
            factoryScope = DiscoDataContextProvider.Push(factory);
            transaction = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled);
        }

        public void Dispose()
        {
            transaction.Dispose();
            factoryScope.Dispose();
            factory.Dispose();
        }
    }

    public sealed class EffortDiscoDataContextFactory : IDiscoDataContextFactory, IDisposable
    {
        private readonly DbConnection connection;

        public EffortDiscoDataContextFactory()
        {
            connection = DbConnectionFactory.CreateTransient();
        }

        public DiscoDataContext Create()
        {
            return new DiscoDataContext(connection, false);
        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }

    public class IntuneDeviceImportIntegrationTests : IntegrationTestScope
    {
        [Fact]
        [Trait("Category", "Integration")]
        public void BeginImport_WithValidCsv_UpdatesStatusToValidation()
        {
            var csv = BuildIntuneLikeCsv("242767", "152631");
            var status = ScheduledTaskMockStatus.Create("Intune import integration test");
            status.ProgressMultiplier = 1;

            var import = IntuneDeviceImport.Create("testfile.csv", CreateMemoryStream(csv), new IntuneDeviceImportConfiguration());

            import.Parse(status);

            Assert.Equal((byte)25, status.Progress);
            Assert.Equal("Parsing: SN-A1B2C3D4", status.CurrentDescription);

            var record = import.Records[0];

            Assert.Equal("SN-A1B2C3D4", record.DeviceSerialNumber);

            var computerNameField = Assert.Single(record.Fields, f => f is IntuneDeviceComputerNameImportField);
            Assert.Equal("LAB-LAPTOP-001", computerNameField.RawParsedValue);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void BeginImport_WithValidZip_UpdatesStatusToValidation()
        {
            var csv = BuildIntuneLikeCsv("242767", "152631");

            var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                var entry = archive.CreateEntry($"DevicesWithInventory_{Guid.NewGuid():d}.csv");
                using (var entryStream = entry.Open())
                using (var writer = new StreamWriter(entryStream, Encoding.UTF8))
                {
                    writer.Write(csv);
                }
            }
            stream.Position = 0;

            var status = ScheduledTaskMockStatus.Create("Intune import integration test");
            status.ProgressMultiplier = 1;

            var import = IntuneDeviceImport.Create("testfile.zip", stream, new IntuneDeviceImportConfiguration());

            import.Parse(status);

            Assert.Equal((byte)25, status.Progress);
            Assert.Equal("Parsing: SN-A1B2C3D4", status.CurrentDescription);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void BeginImport_WithInvalidNumericValue_ThrowsFormatException()
        {
            var csv = BuildIntuneLikeCsv("invalid-number", "152631");
            var status = ScheduledTaskMockStatus.Create("Intune import integration test");
            status.ProgressMultiplier = 1;

            var import = IntuneDeviceImport.Create("testfile.csv", CreateMemoryStream(csv), new IntuneDeviceImportConfiguration());

            Assert.Throws<FormatException>(() => import.Parse(status));
            Assert.Equal((byte)5, status.Progress);
            Assert.Equal("Processing import file", status.CurrentDescription);
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
