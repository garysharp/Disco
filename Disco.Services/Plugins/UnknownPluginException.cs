using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disco.Services.Plugins
{
    public class UnknownPluginException : Exception
    {
        private string _pluginRequested;

        public string PluginRequested
        {
            get
            {
                return _pluginRequested;
            }
        }

        public UnknownPluginException(string PluginRequested)
        {
            this._pluginRequested = PluginRequested;
        }
        public UnknownPluginException(string PluginRequested, string Message) : base(Message)
        {
            this._pluginRequested = PluginRequested;
        }

        public override string Message
        {
            get
            {
                return string.Format("Unknown Plugin Id: [{0}]", _pluginRequested);
            }
        }
    }
}
