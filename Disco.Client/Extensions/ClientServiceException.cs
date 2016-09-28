using System;

namespace Disco.Client.Extensions
{
    public class ClientServiceException : Exception
    {
        public string ServiceFeature { get; private set; }

        public ClientServiceException(string ServiceFeature, string ServerMessage) : base(ServerMessage)
        {
            this.ServiceFeature = ServiceFeature;
        }
    }
}
