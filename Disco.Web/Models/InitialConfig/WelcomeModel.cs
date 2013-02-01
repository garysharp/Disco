using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Net;
using System.Xml.Linq;
using System.IO;
using System.Globalization;

namespace Disco.Web.Models.InitialConfig
{
    public class WelcomeModel
    {
        [Required(ErrorMessage="The Organisation Name is required.")]
        public string OrganisationName { get; set; }


        private static string _OrganisationNameCache;
        public bool AutodetectOrganisation()
        {
            if (_OrganisationNameCache != null)
            {
                this.OrganisationName = _OrganisationNameCache;
                return true;
            }

            // DnsQuery for broadband.doe.wan
            IPHostEntry doeWanDnsEntry;
            try
            {
                doeWanDnsEntry = Dns.GetHostEntry("broadband.doe.wan");
            }
            catch (Exception)
            { return false; } // Fail on error

            // Try using IPSearch feature
            XDocument doeWanIPSearchResult = TryDownloadDoeIPSearch(true);
            if (doeWanIPSearchResult == null)
                doeWanIPSearchResult = TryDownloadDoeIPSearch(false);

            if (doeWanIPSearchResult == null)
                return false;

            try
            {
                var siteName = doeWanIPSearchResult.Element("resultset").Element("site").Element("name").Value.ToLower();
                this.OrganisationName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(siteName);
                _OrganisationNameCache = this.OrganisationName;
                return true;
            }
            catch (Exception)
            { return false; } // Fail on error
        }
        private XDocument TryDownloadDoeIPSearch(bool useProxy)
        {
            try
            {
                HttpWebRequest wReq = (HttpWebRequest)HttpWebRequest.Create("http://broadband.doe.wan/ipsearch/showresult.php");
                if (!useProxy)
                    wReq.Proxy = new WebProxy(); // Empty Proxy Config
                wReq.Method = WebRequestMethods.Http.Post;
                wReq.ContentType = "application/x-www-form-urlencoded";
                wReq.UserAgent = string.Format("Disco/{0}", DiscoApplication.Version);
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