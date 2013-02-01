using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using Disco.Data.Repository;
using Disco.Models.BI.Interop.Community;
using Disco.Services.Tasks;
using Newtonsoft.Json;

namespace Disco.BI.Interop.Community
{
    public static class UpdateCheck
    {
        private static string UpdateUrl(DiscoDataContext db)
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
                    return "http://hades3:9393/base/DiscoUpdate/V1";
                }
            }
            catch (Exception)
            {} // Ignore Errors

            return "http://discoict.com.au/base/DiscoUpdate/V1";
        }

        public static string CurrentDiscoVersion()
        {
            var AssemblyVersion = typeof(UpdateCheck).Assembly.GetName().Version;
            return string.Format("{0}.{1}.{2:0000}.{3:0000}", AssemblyVersion.Major, AssemblyVersion.Minor, AssemblyVersion.Build, AssemblyVersion.Revision);
        }

        public static UpdateResponse Check(DiscoDataContext db, bool UseProxy, ScheduledTaskStatus status = null)
        {
            if (status != null)
                status.UpdateStatus(10, "Building Update Request");

            var request = BuildRequest(db);
            //var requestJson = JsonConvert.SerializeObject(request);

            if (status != null)
                status.UpdateStatus(40, "Sending Request");

            var DiscoBIVersion = CurrentDiscoVersion();

            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(UpdateUrl(db));
            webRequest.ContentType = "application/json";
            webRequest.Method = WebRequestMethods.Http.Post;
            webRequest.UserAgent = string.Format("Disco/{0} (Update)", DiscoBIVersion);

            using (var wrStream = webRequest.GetRequestStream())
            {
                XmlSerializer xml = new XmlSerializer(typeof(UpdateRequestV1));
                xml.Serialize(wrStream, request);
            }
            if (status != null)
                status.UpdateStatus(50, "Waiting for Response");
            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                if (webResponse.StatusCode == HttpStatusCode.OK)
                {
                    if (status != null)
                        status.UpdateStatus(90, "Reading Response");
                    UpdateResponse result;
                    using (var wResStream = webResponse.GetResponseStream())
                    {
                        XmlSerializer xml = new XmlSerializer(typeof(UpdateResponse));
                        result = (UpdateResponse)xml.Deserialize(wResStream);
                    }
                    //var result = JsonConvert.DeserializeObject<UpdateResponse>(responseContent);
                    db.DiscoConfiguration.UpdateLastCheck = result;
                    db.SaveChanges();

                    status.SetFinishedMessage(string.Format("The update server reported Version {0} is the latest.", result.Version));
                    
                    return result;
                }
                else
                {
                    if (status != null)
                        status.SetTaskException(new WebException(string.Format("Server responded with: [{0}] {1}", webResponse.StatusCode, webResponse.StatusDescription)));
                    return null;
                }
            }
        }

        private static UpdateRequestV1 BuildRequest(DiscoDataContext db)
        {
            var m = new UpdateRequestV1();

            m.DeploymentId = db.DiscoConfiguration.DeploymentId;

            m.CurrentDiscoVersion = CurrentDiscoVersion();

            m.OrganisationName = db.DiscoConfiguration.OrganisationName;
            m.BroadbandDoeWanId = GetBroadbandDoeWanId();
            m.BetaDeployment = db.DiscoConfiguration.UpdateBetaDeployment;

            m.Stat_JobCounts = db.Jobs.GroupBy(j => j.JobTypeId).Select(g => new Disco.Models.BI.Interop.Community.UpdateRequestV1.Stat { Key = g.Key, Count = g.Count() }).ToList();
            m.Stat_OpenJobCounts = db.Jobs.Where(j => j.ClosedDate == null).GroupBy(j => j.JobTypeId).Select(g => new Disco.Models.BI.Interop.Community.UpdateRequestV1.Stat { Key = g.Key, Count = g.Count() }).ToList();
            var activeThreshold = DateTime.Now.AddDays(-60);
            m.Stat_ActiveDeviceModelCounts = db.DeviceModels.Select(dm => new Disco.Models.BI.Interop.Community.UpdateRequestV1.Stat { Key = dm.Manufacturer + ";" + dm.Model, Count = dm.Devices.Count(d => d.DecommissionedDate == null && (d.LastNetworkLogonDate == null || d.LastNetworkLogonDate > activeThreshold)) }).ToList();
            m.Stat_UserCounts = db.Users.GroupBy(u => u.Type).Select(g => new Disco.Models.BI.Interop.Community.UpdateRequestV1.Stat { Key = g.Key, Count = g.Count() }).ToList();

            return m;
        }

        #region DoE Query
        public static string GetBroadbandDoeWanId()
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
            XDocument doeWanIPSearchResult = TryDownloadDoeIPSearch(false);
            if (doeWanIPSearchResult == null)
                doeWanIPSearchResult = TryDownloadDoeIPSearch(true);

            if (doeWanIPSearchResult == null)
                return null;

            try
            {
                return doeWanIPSearchResult.Element("resultset").Element("site").Element("number").Value.ToLower();
            }
            catch (Exception)
            { return null; } // Fail on error
        }
        private static XDocument TryDownloadDoeIPSearch(bool useProxy)
        {
            try
            {
                var DiscoBIVersion = CurrentDiscoVersion();

                HttpWebRequest wReq = (HttpWebRequest)HttpWebRequest.Create("http://broadband.doe.wan/ipsearch/showresult.php");
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
        #endregion

    }
}
