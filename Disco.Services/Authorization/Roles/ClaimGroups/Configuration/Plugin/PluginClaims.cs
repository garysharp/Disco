using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Authorization.Roles.ClaimGroups.Configuration.Plugin
{
    [ClaimDetails("Plugin", "Permissions related to Plugins")]
    public class PluginClaims : BaseRoleClaimGroup
    {
        [ClaimDetails("Show Plugins", "Can show plugins")]
        public bool Show { get; set; }

        [ClaimDetails("Install/Update Plugins", "Can install and update plugins")]
        public bool Install { get; set; }

        [ClaimDetails("Install/Update Local Plugins", "Can install and update locally uploaded plugins")]
        public bool InstallLocal { get; set; }

        [ClaimDetails("Uninstall Plugins", "Can uninstall plugins")]
        public bool Uninstall { get; set; }

        [ClaimDetails("Configure Plugins", "Can configure plugins")]
        public bool Configure { get; set; }
    }
}
