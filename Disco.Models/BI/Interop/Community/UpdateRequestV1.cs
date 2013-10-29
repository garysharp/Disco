using System;
using System.Collections.Generic;

namespace Disco.Models.BI.Interop.Community
{
    public class UpdateRequestV1 : UpdateRequestBase
    {
        public UpdateRequestV1()
        {
            this.RequestVersion = 1;
        }

        public string DeploymentId { get; set; }
        public string CurrentDiscoVersion { get; set; }
        public bool BetaDeployment { get; set; }

        public string OrganisationName { get; set; }
        public string BroadbandDoeWanId { get; set; }

        public List<Stat> Stat_JobCounts { get; set; }
        public List<Stat> Stat_OpenJobCounts { get; set; }
        public List<Stat> Stat_ActiveDeviceModelCounts { get; set; }
        public List<Stat> Stat_DeviceModelCounts { get; set; }
        public List<Stat> Stat_UserCounts { get; set; }

        public List<PluginRef> InstalledPlugins { get; set; }

        public List<Stat> Stat_JobWarrantyVendorCounts { get; set; }

        public class Stat
        {
            public string Key { get; set; }
            public int Count { get; set; }
        }

        public class PluginRef
        {
            public string Id { get; set; }
            public string Version { get; set; }
        }
    }
}
