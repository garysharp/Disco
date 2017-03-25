using System;

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
            _pluginRequested = PluginRequested;
        }
        public UnknownPluginException(string PluginRequested, string Message) : base(Message)
        {
            _pluginRequested = PluginRequested;
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
