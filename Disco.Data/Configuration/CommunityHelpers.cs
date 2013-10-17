using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Data.Configuration
{
    public static class CommunityHelpers
    {
        public static string CommunityUrl()
        {
            // Special case for DiscoCommunity Hosting Network
            try
            {
                var ip = (from addr in Dns.GetHostEntry(Dns.GetHostName()).AddressList
                          where addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                          && addr.ToString().StartsWith("10.131.200.")
                          select addr).FirstOrDefault();
                if (ip != null)
                {
                    return "http://hades3:9393/base/";
                }
            }
            catch (Exception)
            { } // Ignore Errors

            return "https://discoict.com.au/base/";
        }
    }
}
