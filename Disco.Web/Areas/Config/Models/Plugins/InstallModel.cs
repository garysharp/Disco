using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.Models.BI.Interop.Community;

namespace Disco.Web.Areas.Config.Models.Plugins
{
    public class InstallModel
    {
        public PluginLibraryUpdateResponse Catalogue { get; set; }
    }
}