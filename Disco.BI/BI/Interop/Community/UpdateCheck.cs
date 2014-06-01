using Disco.Data.Repository;
using Disco.Models.BI.Interop.Community;
using Disco.Models.Repository;
using Disco.Services.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Disco.BI.Interop.Community
{
    public static class UpdateCheck
    {
        private static string UpdateUrl()
        {
            return string.Concat(Disco.Data.Configuration.CommunityHelpers.CommunityUrl(), "DiscoUpdate/V1");
        }

        public static Version CurrentDiscoVersion()
        {
            return typeof(UpdateCheck).Assembly.GetName().Version;
        }
        public static string CurrentDiscoVersionFormatted()
        {
            var v = CurrentDiscoVersion();
            return string.Format("{0}.{1}.{2:0000}.{3:0000}", v.Major, v.Minor, v.Build, v.Revision);
        }

        public static UpdateResponse Check(DiscoDataContext Database, bool UseProxy, IScheduledTaskStatus status)
        {
            status.UpdateStatus(10, "Building Update Request");

            var request = BuildRequest(Database);

            status.UpdateStatus(40, "Sending Request");

            var DiscoBIVersion = CurrentDiscoVersionFormatted();

            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(UpdateUrl());

            // Added: 2013-02-08 G#
            // Fix for Proxy Servers which dont support KeepAlive
            webRequest.KeepAlive = false;
            // End Added: 2013-02-08 G#

            if (!UseProxy)
                webRequest.Proxy = new WebProxy();

            webRequest.ContentType = "application/json";
            webRequest.Method = WebRequestMethods.Http.Post;
            webRequest.UserAgent = string.Format("Disco/{0} (Update)", DiscoBIVersion);

            using (var wrStream = webRequest.GetRequestStream())
            {
                XmlSerializer xml = new XmlSerializer(typeof(UpdateRequestV1));
                xml.Serialize(wrStream, request);
            }
            status.UpdateStatus(50, "Waiting for Response");
            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                if (webResponse.StatusCode == HttpStatusCode.OK)
                {
                    status.UpdateStatus(90, "Reading Response");
                    UpdateResponse result;
                    using (var wResStream = webResponse.GetResponseStream())
                    {
                        XmlSerializer xml = new XmlSerializer(typeof(UpdateResponse));
                        result = (UpdateResponse)xml.Deserialize(wResStream);
                    }
                    Database.DiscoConfiguration.UpdateLastCheck = result;
                    Database.SaveChanges();

                    status.SetFinishedMessage(string.Format("The update server reported Version {0} is the latest.", result.Version));

                    return result;
                }
                else
                {
                    status.SetTaskException(new WebException(string.Format("Server responded with: [{0}] {1}", webResponse.StatusCode, webResponse.StatusDescription)));
                    return null;
                }
            }
        }

        private static UpdateRequestV1 BuildRequest(DiscoDataContext Database)
        {
            var m = new UpdateRequestV1();

            m.DeploymentId = Database.DiscoConfiguration.DeploymentId;

            m.CurrentDiscoVersion = CurrentDiscoVersionFormatted();

            m.OrganisationName = Database.DiscoConfiguration.OrganisationName;
            m.BroadbandDoeWanId = GetBroadbandDoeWanId();
            m.BetaDeployment = Database.DiscoConfiguration.UpdateBetaDeployment;

            m.Stat_JobCounts = Database.Jobs.GroupBy(j => j.JobTypeId).Select(g => new Disco.Models.BI.Interop.Community.UpdateRequestV1.Stat { Key = g.Key, Count = g.Count() }).ToList();
            m.Stat_OpenJobCounts = Database.Jobs.Where(j => j.ClosedDate == null).GroupBy(j => j.JobTypeId).Select(g => new Disco.Models.BI.Interop.Community.UpdateRequestV1.Stat { Key = g.Key, Count = g.Count() }).ToList();
            m.Stat_DeviceModelCounts = Database.DeviceModels.Select(dm => new Disco.Models.BI.Interop.Community.UpdateRequestV1.Stat { Key = dm.Manufacturer + ";" + dm.Model, Count = dm.Devices.Count(d => d.DecommissionedDate == null) }).ToList();
            var activeThreshold = DateTime.Now.AddDays(-60);
            m.Stat_ActiveDeviceModelCounts = Database.DeviceModels.Select(dm => new Disco.Models.BI.Interop.Community.UpdateRequestV1.Stat { Key = dm.Manufacturer + ";" + dm.Model, Count = dm.Devices.Count(d => d.DecommissionedDate == null && (d.LastNetworkLogonDate == null || d.LastNetworkLogonDate > activeThreshold)) }).ToList();
            m.Stat_UserCounts = new List<UpdateRequestV1.Stat>() {
                new UpdateRequestV1.Stat(){
                     Key = "All Users",
                     Count = Database.Users.Count()
                }
            };

            m.Stat_JobWarrantyVendorCounts = Database.Jobs.Where(j => j.JobTypeId == JobType.JobTypeIds.HWar && j.JobMetaWarranty.ExternalLoggedDate.HasValue && j.JobMetaWarranty.ExternalName != null).GroupBy(j => j.JobMetaWarranty.ExternalName).Select(g => new Disco.Models.BI.Interop.Community.UpdateRequestV1.Stat { Key = g.Key ?? "<Unknown>", Count = g.Count() }).ToList();

            m.InstalledPlugins = Disco.Services.Plugins.Plugins.GetPlugins().Select(manifest => new Disco.Models.BI.Interop.Community.UpdateRequestV1.PluginRef { Id = manifest.Id, Version = manifest.VersionFormatted }).ToList();

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
                var DiscoBIVersion = CurrentDiscoVersionFormatted();

                HttpWebRequest wReq = (HttpWebRequest)HttpWebRequest.Create("http://broadband.doe.wan/ipsearch/showresult.php");
                // Added: 2013-02-08 G#
                // Fix for Proxy Servers which dont support KeepAlive
                wReq.KeepAlive = false;
                // End Added: 2013-02-08 G#
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
