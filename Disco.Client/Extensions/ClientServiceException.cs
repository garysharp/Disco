using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
