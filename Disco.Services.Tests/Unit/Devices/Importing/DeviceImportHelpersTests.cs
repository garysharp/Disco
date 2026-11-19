using Disco.Services.Devices.Importing;
using Xunit;

namespace Disco.Services.Tests.Unit.Devices.Importing
{
    public class DeviceImportHelpersTests
    {
        [Theory]
        [InlineData("001122aAbBcC")]
        [InlineData("00:11:22:aa:bb:cc")]
        [InlineData("00-11-22-AA-BB-CC")]
        [InlineData("0011.22aa.bbcc")]
        [InlineData("  00:11:22:aA:Bb:cC  ")]
        public void TryParseMacAddress_ValidInput_ReturnsCanonicalAddress(string value)
        {
            var success = DeviceImportHelpers.TryParseMacAddress(value, out var address);

            Assert.True(success);
            Assert.Equal("00:11:22:AA:BB:CC", address);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("00:11:22:33:44")]
        [InlineData("00:11:22:33:44:55:66")]
        [InlineData("00:11:22:33:44:GG")]
        [InlineData("00:11-22:33:44:55")]
        [InlineData("0:011:22:33:44:55")]
        [InlineData("0011.2233.4455.")]
        [InlineData("prefix001122334455")]
        [InlineData("001122334455 suffix")]
        public void TryParseMacAddress_InvalidInput_ReturnsFalseAndNull(string value)
        {
            var success = DeviceImportHelpers.TryParseMacAddress(value, out var address);

            Assert.False(success);
            Assert.Null(address);
        }
    }
}
