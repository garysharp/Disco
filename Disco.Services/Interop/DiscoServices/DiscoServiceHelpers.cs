using System;

namespace Disco.Services.Interop.DiscoServices
{
    public static class DiscoServiceHelpers
    {
        [Obsolete]
        public static string CommunityUrl()
        {
            return "https://discoict.com.au/base/";
        }

        public static string ServicesUrl
        {
            get
            {
                return "https://services.discoict.com.au/";
            }
        }
    }
}