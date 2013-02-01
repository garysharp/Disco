using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disco.Services.Plugins
{
    public class PluginWebControllerException : Exception
    {
        public PluginWebControllerException(string PluginId, string PluginAction, Exception InnerException)
            : base(string.Format("An error occurred executing the Disco Plugin [{0}] Web Controller Action: [{0}]", PluginId, PluginAction), InnerException)
        {
        }
    }
}
