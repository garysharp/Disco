using System;

namespace Disco.Services.Plugins.Features.WirelessProfileProvider
{
    public class WirelessProfile
    {

        /// <summary>
        /// The name of the wireless profile, typically the SSID
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The guid of the associated interface. Null to deploy to all supported interfaces.
        /// </summary>
        public Guid? InterfaceGuid { get; set; }

        /// <summary>
        /// Indicates the profile should be overwritten even if it already exists
        /// </summary>
        public bool ForceDeployment { get; set; }

        /// <summary>
        /// The wireless profile XML definition
        /// </summary>
        public string ProfileXml { get; set; }

    }
}
