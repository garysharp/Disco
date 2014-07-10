using Disco.Data.Repository;
using Disco.Models.BI.Config;
using Disco.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Disco.Services.Plugins.Features.RepairProvider
{
    [PluginFeatureCategory(DisplayName = "Repair Providers")]
    public abstract class RepairProviderFeature : PluginFeature
    {
        /// <summary>
        /// The repairer identifier. Used to link this provider to any <see cref="Disco.Models.Repository.RepairerName"/>. This identifier is used to automatically set the RepairerName when a job is submitted using this plugin.
        /// </summary>
        public abstract string ProviderId { get; }
        
        #region Job Submission

        /// <summary>
        /// Called when a user selects this plugin to repair and allows a plugin to inject a View to collect additional information.
        /// </summary>
        /// <returns>A Tuple consisting of the Razor View type and a View Model</returns>
        public virtual Tuple<Type, dynamic> SubmitJobBegin(DiscoDataContext Database, Controller controller, Job Job, OrganisationAddress Address, User TechUser)
        {
            return null;
        }

        /// <summary>
        /// Called after the RepairDescription is completed and allows the plugin to parse any data collected from SubmitJobBegin.
        /// </summary>
        /// <returns>A Dictionary of key/value items which are persisted throughout the submission and passed into the final SubmitJob method.</returns>
        public virtual Dictionary<string, string> SubmitJobParseProperties(DiscoDataContext Database, FormCollection form, Controller controller, Job Job, OrganisationAddress Address, User TechUser, string RepairDescription)
        {
            return null;
        }

        /// <summary>
        /// Plugins are required to disclose any information that will be transmitted to any external party. This method is expected to return a clear list all data which will be transmitted.
        /// </summary>
        /// <returns>A Dictionary of key/value items which contain all information which will be disclosed to the plugin provider.</returns>
        public abstract Dictionary<string, string> SubmitJobDiscloseInfo(DiscoDataContext Database, Job Job, OrganisationAddress Address, User TechUser, string RepairDescription, Dictionary<string, string> ProviderProperties);

        /// <summary>
        /// Called when the plugin should submit the job to the external party.
        /// </summary>
        /// <returns>A reference number/identifier from the external party which is stored in <see cref="Disco.Models.Repository.RepairerReference"/></returns>
        public abstract string SubmitJob(DiscoDataContext Database, Job Job, OrganisationAddress Address, User TechUser, string RepairDescription, Dictionary<string, string> ProviderProperties);

        #endregion

        #region Job Details

        /// <summary>
        /// <see cref="true"/> when additional Job Details are supported by the external party. When <see cref="true"/>, JobDetailsViewModel must be implemented.
        /// </summary>
        public abstract bool JobDetailsSupported { get; }

        /// <summary>
        /// Called when a job repair information is shown. Allows a plugin to inject a View to display additional information.
        /// </summary>
        /// <returns>A Tuple consisting of the Razor View type and a View Model</returns>
        public virtual Tuple<Type, dynamic> JobDetails(DiscoDataContext Database, Controller controller, Job Job)
        {
            return null;
        } 

        #endregion

        public static PluginFeatureManifest FindPluginFeature(string PluginIdOrRepairProviderId)
        {
            var defs = Plugins.GetPluginFeatures(typeof(RepairProviderFeature));
            var def = defs.FirstOrDefault(d => d.PluginManifest.Id.Equals(PluginIdOrRepairProviderId, StringComparison.OrdinalIgnoreCase));
            if (def != null)
                return def;
            else
                foreach (var d in defs)
                {
                    using (var providerInstance = d.CreateInstance<RepairProviderFeature>())
                    {
                        if (providerInstance.ProviderId != null && providerInstance.ProviderId.Equals(PluginIdOrRepairProviderId, StringComparison.OrdinalIgnoreCase))
                        {
                            return d;
                        }
                    }
                }

            return null;
        }
    }
}
