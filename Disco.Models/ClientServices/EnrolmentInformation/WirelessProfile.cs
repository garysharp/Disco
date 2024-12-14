using System;

namespace Disco.Models.ClientServices.EnrolmentInformation
{
    public class WirelessProfile
    {

        /// <summary>
        /// The name of the wireless profile, typically the SSID
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The guid of the associated interface.
        /// </summary>
        public Guid? InterfaceGuid { get; set; }

        /// <summary>
        /// Indicates the profile is deployed via Group Policy and therefore read-only
        /// </summary>
        public bool? IsGroupPolicy { get; set; }

        /// <summary>
        /// Indicates the profile should be overwritten even if it already exists
        /// </summary>
        public bool? ForceDeployment { get; set; }

        /// <summary>
        /// The wireless profile XML definition
        /// </summary>
        public string ProfileXml { get; set; }

    }
}
