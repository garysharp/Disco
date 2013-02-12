using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disco.Data.Repository;
using Disco.Services.Plugins;
using Disco.Services.Tasks;
using Disco.Models.BI.Interop.Community;
using System.Net;
using System.Xml.Serialization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Disco.BI.Interop.Community
{
    public class PluginLibraryUpdateTask : ScheduledTask
    {
        public override string TaskName { get { return "Disco Community - Update Plugin Library"; } }

        protected override void ExecuteTask()
        {
            PluginLibraryUpdateRequest updateRequestBody;
            PluginLibraryUpdateResponse updateResult;
            string catalogueFile;
            PluginLibraryCompatibilityRequest compatRequestBody;
            PluginLibraryCompatibilityResponse compatResult;
            string compatibilityFile;

            var DiscoBIVersion = UpdateCheck.CurrentDiscoVersion();
            HttpWebRequest webRequest;

            #region Update
            
            Status.UpdateStatus(1, "Updating Plugin Library Catalogue", "Building Request");
            
            using (DiscoDataContext dbContext = new DiscoDataContext())
            {
                catalogueFile = Plugins.CatalogueFile(dbContext);

                updateRequestBody = new PluginLibraryUpdateRequest()
                {
                    DeploymentId = dbContext.DiscoConfiguration.DeploymentId,
                    HostVersion = typeof(Plugins).Assembly.GetName().Version.ToString(4)
                };
            }

            Status.UpdateStatus(10, "Sending Request");

            webRequest = (HttpWebRequest)HttpWebRequest.Create(PluginLibraryUpdateUrl());
            webRequest.KeepAlive = false;

            webRequest.ContentType = "application/json";
            webRequest.Method = WebRequestMethods.Http.Post;
            webRequest.UserAgent = string.Format("Disco/{0} (PluginLibrary)", DiscoBIVersion);

            using (var wrStream = webRequest.GetRequestStream())
            {
                XmlSerializer xml = new XmlSerializer(typeof(PluginLibraryUpdateRequest));
                xml.Serialize(wrStream, updateRequestBody);
            }

            Status.UpdateStatus(20, "Waiting for Response");

            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                if (webResponse.StatusCode == HttpStatusCode.OK)
                {
                    Status.UpdateStatus(45, "Reading Response");
                    using (var wResStream = webResponse.GetResponseStream())
                    {
                        XmlSerializer xml = new XmlSerializer(typeof(PluginLibraryUpdateResponse));
                        updateResult = (PluginLibraryUpdateResponse)xml.Deserialize(wResStream);
                    }
                }
                else
                {
                    Status.SetTaskException(new WebException(string.Format("Server responded with: [{0}] {1}", webResponse.StatusCode, webResponse.StatusDescription)));
                    return;
                }
            }

            if (!Directory.Exists(Path.GetDirectoryName(catalogueFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(catalogueFile));

            using (FileStream fs = new FileStream(catalogueFile, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter fsWriter = new StreamWriter(fs))
                {
                    fsWriter.Write(JsonConvert.SerializeObject(updateResult));
                    fsWriter.Flush();
                }
            }
            #endregion

            #region Compatibility

            Status.UpdateStatus(50, "Updating Plugin Library Compatibility", "Building Request");

            using (DiscoDataContext dbContext = new DiscoDataContext())
            {
                compatibilityFile = Plugins.CompatibilityFile(dbContext);

                compatRequestBody = new PluginLibraryCompatibilityRequest()
                {
                    DeploymentId = dbContext.DiscoConfiguration.DeploymentId,
                    HostVersion = typeof(Plugins).Assembly.GetName().Version.ToString(4)
                };
            }

            Status.UpdateStatus(60, "Sending Request");

            webRequest = (HttpWebRequest)HttpWebRequest.Create(PluginLibraryCompatibilityUrl());
            webRequest.KeepAlive = false;

            webRequest.ContentType = "application/json";
            webRequest.Method = WebRequestMethods.Http.Post;
            webRequest.UserAgent = string.Format("Disco/{0} (PluginLibrary)", DiscoBIVersion);

            using (var wrStream = webRequest.GetRequestStream())
            {
                XmlSerializer xml = new XmlSerializer(typeof(PluginLibraryCompatibilityRequest));
                xml.Serialize(wrStream, compatRequestBody);
            }

            Status.UpdateStatus(70, "Waiting for Response");

            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                if (webResponse.StatusCode == HttpStatusCode.OK)
                {
                    Status.UpdateStatus(95, "Reading Response");
                    using (var wResStream = webResponse.GetResponseStream())
                    {
                        XmlSerializer xml = new XmlSerializer(typeof(PluginLibraryCompatibilityResponse));
                        compatResult = (PluginLibraryCompatibilityResponse)xml.Deserialize(wResStream);
                    }
                }
                else
                {
                    Status.SetTaskException(new WebException(string.Format("Server responded with: [{0}] {1}", webResponse.StatusCode, webResponse.StatusDescription)));
                    return;
                }
            }

            if (!Directory.Exists(Path.GetDirectoryName(compatibilityFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(compatibilityFile));

            using (FileStream fs = new FileStream(compatibilityFile, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter fsWriter = new StreamWriter(fs))
                {
                    fsWriter.Write(JsonConvert.SerializeObject(compatResult));
                    fsWriter.Flush();
                }
            }
            #endregion




            Status.SetFinishedMessage("The Plugin Library Catalogue was updated.");
        }

        private static string PluginLibraryUpdateUrl()
        {
            return string.Concat(CommunityHelpers.CommunityUrl(), "DiscoPluginLibrary/V1");
        }
        private static string PluginLibraryCompatibilityUrl()
        {
            return string.Concat(CommunityHelpers.CommunityUrl(), "DiscoPluginLibrary/CompatibilityV1");
        }

        public static ScheduledTaskStatus ScheduleNow()
        {

            var taskStatus = ScheduledTasks.GetTaskStatuses(typeof(PluginLibraryUpdateTask)).Where(ts => ts.IsRunning).FirstOrDefault();
            if (taskStatus != null)
                return taskStatus;
            else
            {
                var t = new PluginLibraryUpdateTask();
                return t.ScheduleTask();
            }
        }
        public static ScheduledTaskStatus RunningStatus
        {
            get
            {
                return ScheduledTasks.GetTaskStatuses(typeof(PluginLibraryUpdateTask)).Where(ts => ts.IsRunning).FirstOrDefault();
            }
        }
    }
}
