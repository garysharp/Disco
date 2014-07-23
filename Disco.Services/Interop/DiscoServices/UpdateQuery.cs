using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Interop.DiscoServices;
using Disco.Services.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Disco.Services.Interop.DiscoServices
{
    using StatisticInt = UpdateRequestV2.StatisticInt;
    using StatisticJob = UpdateRequestV2.StatisticJob;
    using StatisticString = UpdateRequestV2.StatisticString;

    public static class UpdateQuery
    {
        private static string UpdateUrl()
        {
            return string.Concat(DiscoServiceHelpers.ServicesUrl, "API/Update/V2");
        }

        public static Version CurrentDiscoVersion()
        {
            return typeof(UpdateQuery).Assembly.GetName().Version;
        }

        public static string CurrentDiscoVersionFormatted()
        {
            var v = CurrentDiscoVersion();
            return FormatVersion(v);
        }

        public static string FormatVersion(Version Version)
        {
            return string.Format("{0}.{1}.{2:0000}.{3:0000}", Version.Major, Version.Minor, Version.Build, Version.Revision);
        }

        public static string HashDeploymentData(DiscoDataContext Database, string Data)
        {
            if (Data == null)
                return null;

            string clearText = Database.DiscoConfiguration.DeploymentSecret + Data;
            byte[] clearTextBytes = Encoding.Unicode.GetBytes(clearText);
            byte[] hashBytes;

            using (var hashAlgorithm = SHA1.Create())
            {
                hashBytes = hashAlgorithm.ComputeHash(clearTextBytes);
            }

            return Convert.ToBase64String(hashBytes);
        }

        public static UpdateResponseV2 Check(DiscoDataContext Database, bool UseProxy, IScheduledTaskStatus Status)
        {
            Status.UpdateStatus(10, "Gathering statistics and building update request");

            var updateRequest = BuildRequest(Database);
            var updateRequestJson = JsonConvert.SerializeObject(updateRequest);

            Status.UpdateStatus(40, "Sending statistics and update request");

            var discoVersion = CurrentDiscoVersionFormatted();

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(UpdateUrl());

            // Fix for Proxy Servers which don't support KeepAlive
            request.KeepAlive = false;

            if (!UseProxy)
                request.Proxy = new WebProxy();

            request.ContentType = "application/json; charset=utf-8; encoding=gzip";
            request.Method = WebRequestMethods.Http.Post;
            request.UserAgent = string.Format("Disco/{0} (Update)", discoVersion);

            using (var requestStream = request.GetRequestStream())
            {
                using (var compressedStream = new GZipStream(requestStream, CompressionLevel.Optimal))
                {
                    using (var requestStreamWriter = new StreamWriter(compressedStream, Encoding.UTF8))
                    {
                        requestStreamWriter.Write(updateRequestJson);
                        requestStreamWriter.Flush();
                    }
                }
            }

            Status.UpdateStatus(50, "Waiting for update response");
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Status.UpdateStatus(90, "Reading update response");
                    string updateResultJson;
                    UpdateResponseV2 updateResult;

                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var responseReader = new StreamReader(responseStream))
                        {
                            updateResultJson = responseReader.ReadToEnd();
                        }
                    }

                    updateResult = JsonConvert.DeserializeObject<UpdateResponseV2>(updateResultJson);

                    Database.DiscoConfiguration.UpdateLastCheckResponse = updateResult;
                    Database.SaveChanges();

                    Status.SetFinishedMessage(string.Format("The update server reported Version {0} is the latest.", updateResult.LatestVersion));

                    return updateResult;
                }
                else
                {
                    Status.SetTaskException(new WebException(string.Format("Server responded with: [{0}] {1}", response.StatusCode, response.StatusDescription)));
                    return null;
                }
            }
        }

        private static UpdateRequestV2 BuildRequest(DiscoDataContext Database)
        {
            var lastUpdate = Database.DiscoConfiguration.UpdateLastCheckResponse;
            var m = new UpdateRequestV2();

            m.DeploymentId = Guid.Parse(Database.DiscoConfiguration.DeploymentId);
            m.RequestDate = DateTime.Now;
            m.VersionCurrent = CurrentDiscoVersionFormatted();
            m.IsBetaDeployment = Database.DiscoConfiguration.UpdateBetaDeployment;

            m.OrganisationName = Database.DiscoConfiguration.OrganisationName;

            var whoAmIResponse = VicEduDept.VicSmart.WhoAmI();
            if (whoAmIResponse != null && !string.IsNullOrWhiteSpace(whoAmIResponse.Item1))
                m.BroadbandDoeWanId = whoAmIResponse.Item1;

            m.Stat_JobCounts = Database.Jobs.GroupBy(j => j.JobTypeId).Select(g => new StatisticInt() { K = g.Key, V = g.Count() }).ToList();
            m.Stat_OpenJobCounts = Database.Jobs.Where(j => j.ClosedDate == null).GroupBy(j => j.JobTypeId).Select(g => new StatisticInt() { K = g.Key, V = g.Count() }).ToList();
            m.Stat_DeviceModelCounts = Database.DeviceModels.Select(dm => new StatisticInt() { K = dm.Manufacturer + ";" + dm.Model, V = dm.Devices.Count(d => d.DecommissionedDate == null) }).ToList();
            var activeThreshold = DateTime.Now.AddDays(-60);
            m.Stat_ActiveDeviceModelCounts = Database.DeviceModels.Select(dm => new StatisticInt() { K = dm.Manufacturer + ";" + dm.Model, V = dm.Devices.Count(d => d.DecommissionedDate == null && (d.LastNetworkLogonDate == null || d.LastNetworkLogonDate > activeThreshold)) }).ToList();
            m.Stat_UserCounts = new List<StatisticInt>() {
                new StatisticInt() { K = "All", V = Database.Users.Count() },
                new StatisticInt() { K = "Assigned Current", V  = Database.Users.Where(u => u.DeviceUserAssignments.Any(dua => !dua.UnassignedDate.HasValue)).Count() },
                new StatisticInt() { K = "Assigned Ever", V = Database.Users.Where(u => u.DeviceUserAssignments.Any()).Count() },
                new StatisticInt() { K = "Job Technicians", V = Database.Jobs.Select(j => j.OpenedTechUserId).Distinct().ToList().Concat(Database.Jobs.Select(j => j.ClosedTechUserId).Distinct().ToList()).Distinct().Count() },
                new StatisticInt() { K = "Job Users", V = Database.Jobs.Where(j => j.UserId != null).Select(j => j.UserId).Distinct().Count() }
            };

            IQueryable<Job> jobs;
            if (lastUpdate == null)
                jobs = Database.Jobs;
            else
            {
                var lastUpdateDate = lastUpdate.UpdateResponseDate.Date;
                jobs = Database.Jobs.Where(j => j.OpenedDate >= lastUpdateDate || (j.ClosedDate.HasValue && j.ClosedDate.Value >= lastUpdateDate));
            }

            var reportedJobs = jobs.Select(j => new
            {
                Id = j.Id,
                OpenedDate = j.OpenedDate,
                ClosedDate = j.ClosedDate,
                JobType = j.JobTypeId,
                JobSubTypes = j.JobSubTypes.Select(jst => jst.Id),
                DeviceModelManufacturer = j.Device.DeviceModel.Manufacturer,
                DeviceModelModel = j.Device.DeviceModel.Model,
                DeviceSerialNumber = j.DeviceSerialNumber,
                UserId = j.UserId,
                JobTechnicianId = j.OpenedTechUserId,
                WarrantyRepairer = j.JobMetaWarranty.ExternalName,
                WarrantyRepairerLoggedDate = j.JobMetaWarranty.ExternalLoggedDate,
                WarrantyRepairerCompletedDate = j.JobMetaWarranty.ExternalCompletedDate,
                Repairer = j.JobMetaNonWarranty.RepairerName,
                RepairerLoggedDate = j.JobMetaNonWarranty.RepairerLoggedDate,
                RepairerCompletedDate = j.JobMetaNonWarranty.RepairerCompletedDate,
            }).ToList();

            m.Stat_Jobs = reportedJobs.Select(j => new StatisticJob()
            {
                I = j.Id,
                OD = j.OpenedDate,
                CD = j.ClosedDate,
                T = j.JobType,
                ST = j.JobSubTypes == null ? null : string.Join(";", j.JobSubTypes),
                D = HashDeploymentData(Database, j.DeviceSerialNumber),
                U = HashDeploymentData(Database, j.UserId),
                TI = HashDeploymentData(Database, j.JobTechnicianId),
                DM = string.Format("{0};{1}", j.DeviceModelManufacturer, j.DeviceModelModel),
                R = j.JobType == JobType.JobTypeIds.HWar ? j.WarrantyRepairer : j.Repairer,
                RL = j.JobType == JobType.JobTypeIds.HWar ? j.WarrantyRepairerLoggedDate : j.RepairerLoggedDate,
                RC = j.JobType == JobType.JobTypeIds.HWar ? j.WarrantyRepairerCompletedDate : j.RepairerCompletedDate
            }).ToList();

            m.InstalledPlugins = Disco.Services.Plugins.Plugins.GetPlugins().Select(manifest => new StatisticString() { K = manifest.Id, V = manifest.VersionFormatted }).ToList();

            return m;
        }
    }
}
