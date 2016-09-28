using System.Collections.Generic;

namespace Disco.Models.ClientServices.EnrolmentInformation
{
    public class WirelessProfileStore
    {

        /// <summary>
        /// A list of wireless profiles to add to the client device.
        /// </summary>
        public List<WirelessProfile> Profiles { get; set; }

        /// <summary>
        /// A list of transformations to be applied to existing XML wireless profiles found on the client device.
        /// </summary>
        public List<WirelessProfileTransformation> Transformations { get; set; }

        /// <summary>
        /// A list of wireless profile names to be removed from the client device.
        /// </summary>
        public List<string> RemoveNames { get; set; }

    }
}
