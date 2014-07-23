using Disco.Services.Interop.DiscoServices;
using System;
using System.IO;
using System.Net;
using System.Xml.Linq;

namespace Disco.Services.Interop.VicEduDept
{
    public class VicSmart
    {

        /// <summary>
        /// Queries DoE VicSmart Service to detect the current site.
        /// </summary>
        /// <returns>A Tuple where Item1 is the Site Number and Item2 is the Site Name</returns>
        public static Tuple<string, string> WhoAmI()
        {
            // DnsQuery for broadband.doe.wan
            IPHostEntry doeWanDnsEntry;
            try
            {
                doeWanDnsEntry = Dns.GetHostEntry("broadband.doe.wan");
            }
            catch (Exception)
            { return null; } // Fail on error

            // Try using IPSearch feature
            XDocument doeWanIPSearchResult = TryIPWhoAmISearch(false);
            if (doeWanIPSearchResult == null)
                doeWanIPSearchResult = TryIPWhoAmISearch(true);

            if (doeWanIPSearchResult == null)
                return null;

            try
            {
                var site = doeWanIPSearchResult.Element("resultset").Element("site");
                var siteNumber = site.Element("number").Value.ToLower();
                var siteName = site.Element("name").Value.ToLower();

                return Tuple.Create(siteNumber, siteName);
            }
            catch (Exception)
            { return null; } // Fail on error
        }

        private static XDocument TryIPWhoAmISearch(bool useProxy)
        {
            try
            {
                var DiscoBIVersion = UpdateQuery.CurrentDiscoVersionFormatted();

                HttpWebRequest wReq = (HttpWebRequest)HttpWebRequest.Create("http://broadband.doe.wan/ipsearch/showresult.php");

                // Fix for Proxy Servers which don't support KeepAlive
                wReq.KeepAlive = false;

                if (!useProxy)
                    wReq.Proxy = new WebProxy(); // Empty Proxy Config
                wReq.Method = WebRequestMethods.Http.Post;
                wReq.ContentType = "application/x-www-form-urlencoded";
                wReq.UserAgent = string.Format("Disco/{0}", DiscoBIVersion);
                using (var wrStream = wReq.GetRequestStream())
                {
                    using (var wrStreamWriter = new StreamWriter(wrStream))
                    {
                        wrStreamWriter.Write("mode=whoami");
                    }
                }
                using (HttpWebResponse wRes = (HttpWebResponse)wReq.GetResponse())
                {
                    if (wRes.StatusCode == HttpStatusCode.OK)
                    {
                        using (var wResStream = wRes.GetResponseStream())
                        {
                            return XDocument.Load(wResStream);
                        }
                    }
                    else
                        return null;
                }
            }
            catch (Exception)
            { return null; } // Fail on error
        }

    }
}