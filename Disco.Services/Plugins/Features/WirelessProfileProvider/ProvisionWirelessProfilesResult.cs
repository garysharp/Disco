using Disco.Models.ClientServices;
using Disco.Models.Repository;
using System.Collections.Generic;

namespace Disco.Services.Plugins.Features.WirelessProfileProvider
{
    public class ProvisionWirelessProfilesResult
    {

        /// <summary>
        /// The <see cref="Device"/> associated with the provision result.
        /// </summary>
        public Device Device { get; set; }

        /// <summary>
        /// The <see cref="Enrol"/> associated with the provision result.
        /// </summary>
        public Enrol Enrolment { get; set; }

        /// <summary>
        /// A list of wireless profiles to add to the client device.
        /// If the wireless profile already exists it will be ignored.
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
