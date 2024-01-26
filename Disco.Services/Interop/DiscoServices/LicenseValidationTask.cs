using Disco.Data.Repository;
using Disco.Services.Tasks;
using Newtonsoft.Json;
using Quartz;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Disco.Services.Interop.DiscoServices
{
    public class LicenseValidationTask : ScheduledTask
    {
        private const string jobMapLicenseKey = "License";
        public override string TaskName { get { return "License Validation"; } }

        public override void InitalizeScheduledTask(DiscoDataContext Database)
        {
            if (Database.DiscoConfiguration.LicenseKey != null)
            {
                // Trigger in 1 + 0-29 minutes
                var rng = new Random();
                var delay = rng.Next(30) + 1;

                TriggerBuilder triggerBuilder = TriggerBuilder.Create()
                    .StartAt(DateTimeOffset.Now.AddMinutes(delay));

                ScheduleTask(triggerBuilder);

                base.InitalizeScheduledTask(Database);
            }
        }

        public static ScheduledTaskStatus ScheduleNow(string license)
        {
            var taskStatus = RunningStatus;
            if (taskStatus != null)
                return taskStatus;
            else
            {
                var task = new LicenseValidationTask();

                var taskData = new JobDataMap() { { jobMapLicenseKey, license } };

                return task.ScheduleTask(taskData);
            }
        }

        public static ScheduledTaskStatus RunningStatus =>
            ScheduledTasks.GetTaskStatuses(typeof(LicenseValidationTask)).Where(ts => ts.IsRunning).FirstOrDefault();

        protected override void ExecuteTask()
        {
            var license = ExecutionContext.JobDetail.JobDataMap.GetString(jobMapLicenseKey);
            string orgName;
            Guid deploymentId;
            using (var database = new DiscoDataContext())
            {
                if (license == null)
                    license = database.DiscoConfiguration.LicenseKey;
                orgName = database.DiscoConfiguration.OrganisationName;
                deploymentId = Guid.Parse(database.DiscoConfiguration.DeploymentId);
            }
            if (!string.IsNullOrWhiteSpace(license))
            {
                var result = ValidateLicense(deploymentId, orgName, license, true, out var infrastructureError);

                string infrastructureError2 = null;
                if (result == null)
                    result = ValidateLicense(deploymentId, orgName, license, false, out infrastructureError2);

                if (result == null)
                {
                    var error = $"Validation failed. {infrastructureError ?? infrastructureError2}";
                    Status.SetTaskException(new Exception(error));
                }
                else
                {
                    if (result.IsValid)
                    {
                        using (var database = new DiscoDataContext())
                        {
                            database.DiscoConfiguration.LicenseKey = license;
                            database.DiscoConfiguration.LicenseExpiresOn = result.ExpiresOn;
                            database.DiscoConfiguration.LicenseError = null;
                            database.SaveChanges();
                        }
                        Status.UpdateStatus(100, "License validated");
                    }
                    else
                    {
                        var error = result.ErrorMessage ?? "Validation failed";
                        Status.SetTaskException(new Exception(error));
                    }
                }
            }
        }

        private LicenseResponseV1 ValidateLicense(Guid deploymentId, string organisationName, string license, bool useProxy, out string infrastructureError)
        {
            Status.UpdateStatus(10, $"Validating license for {organisationName}");

            var appVersion = typeof(LicenseValidationTask).Assembly.GetName().Version.ToString(4);
            var updateUrl = $"{DiscoServiceHelpers.ServicesUrl}API/License/V1";

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(updateUrl);

            // Fix for Proxy Servers which don't support KeepAlive
            request.KeepAlive = false;

            if (!useProxy)
                request.Proxy = new WebProxy();

            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = WebRequestMethods.Http.Post;
            request.UserAgent = $"Disco/{appVersion} (License)";

            using (var requestStream = request.GetRequestStream())
            {
                using (var writer = new StreamWriter(requestStream, Encoding.GetEncoding(28591)))
                {
                    writer.Write("deploymentId=");
                    writer.Write(Uri.EscapeDataString(deploymentId.ToString()));
                    writer.Write($"&license={Uri.EscapeDataString(license)}");
                }
            }

            Status.UpdateStatus(50, "Waiting for validation response");
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Status.UpdateStatus(90, "Reading validation response");
                    string validationJson;
                    LicenseResponseV1 updateResult;

                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var responseReader = new StreamReader(responseStream))
                        {
                            validationJson = responseReader.ReadToEnd();
                        }
                    }

                    updateResult = JsonConvert.DeserializeObject<LicenseResponseV1>(validationJson);

                    infrastructureError = null;
                    return updateResult;
                }
                else
                {
                    infrastructureError = $"Server responded with: [{response.StatusCode}] {response.StatusDescription}";
                    return null;
                }
            }
        }

        private class LicenseResponseV1
        {
            public bool IsValid { get; set; }
            public DateTime? ValidOn { get; set; }
            public DateTime? ExpiresOn { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}
