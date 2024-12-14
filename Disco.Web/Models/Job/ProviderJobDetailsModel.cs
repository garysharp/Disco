using System;

namespace Disco.Web.Models.Job
{
    public class ProviderJobDetailsModel
    {
        public Type ViewType { get; set; }
        public object ViewModel { get; set; }

        public Exception JobDetailsException { get; set; }
        public bool JobDetailsSupported { get; set; }
        public string JobDetailsNotSupportedMessage { get; set; }
    }
}