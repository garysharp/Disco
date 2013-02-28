using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.Config.Models.DocumentTemplate
{
    public class ExpressionBrowserModel
    {
        public string DeviceType { get; set; }
        public string UserType { get; set; }
        public string JobType { get; set; }

        //public string DataExtType { get; set; }
        //public string DeviceExtType { get; set; }
        //public string UserExtType { get; set; }

        public Dictionary<string, string> Variables { get; set; }
        public Dictionary<string, string> ExtensionLibraries { get; set; }
    }
}