using Disco.Data.Repository;
using Disco.Models.ClientServices;
using Disco.Models.Services.Devices;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Devices.Enrolment;
using Disco.Services.Web;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Web.Mvc;

namespace Disco.Web.Areas.Services.Controllers
{
    [OutputCache(Duration = 0)]
    public partial class ClientController : BaseController
    {
        [DiscoAuthorize(Claims.Config.Enrolment.DownloadBootstrapper)]
        public virtual ActionResult Bootstrapper()
        {
            return File(Links.ClientBin.Disco_ClientBootstrapper_exe, "application/x-msdownload", "Disco.ClientBootstrapper.exe");
        }

        public virtual ActionResult PreparationClient()
        {
            var discoveryMethodHeader = Request.Headers["X-DiscoICT-Discovery"];
            if (!string.IsNullOrEmpty(discoveryMethodHeader) && Enum.TryParse<DeviceEnrolmentServerDiscoveryMethod>(discoveryMethodHeader, out var discoveryMethod))
                WindowsDeviceEnrolment.IncrementDiscoveryMethod(discoveryMethod);

            if (!CheckLegacyEnrollmentDiscovery())
                return BadRequest("Enrollment Legacy Discovery is disabled. Please use secure connection (HTTPS) for device enrollment.");

            return File(Links.ClientBin.PreparationClient_zip, "application/x-msdownload", "PreparationClient.zip");
        }

        public virtual ActionResult Unauthenticated(string feature)
        {
            if (!CheckLegacyEnrollmentDiscovery())
                return BadRequest("Enrollment Legacy Discovery is disabled. Please use secure connection (HTTPS) for device enrollment.");

            if (string.IsNullOrEmpty(feature))
            {
                return Json(null);
            }
            switch (feature.ToLower())
            {
                case "enrol":
                    {
                        // Ensure supported version
                        if (Request.UserAgent.StartsWith(@"Disco-Client/", StringComparison.OrdinalIgnoreCase))
                        {
                            if (Version.TryParse(Request.UserAgent.Substring(13), out var clientVersion))
                            {
                                if (clientVersion < new Version(2, 2))
                                {
                                    return BadRequest("Disco ICT Client not compatible");
                                }
                            }
                        }

                        var serializer = new JsonSerializer();
                        Enrol enrolRequest;

                        Request.InputStream.Position = 0;
                        using (var streamReader = new StreamReader(Request.InputStream))
                        {
                            using (var jsonReader = new JsonTextReader(streamReader))
                            {
                                enrolRequest = serializer.Deserialize<Enrol>(jsonReader);
                            }
                        }

                        EnrolResponse enrolResponse = enrolRequest.BuildResponse();
                        return Json(enrolResponse);
                    }
                case "macenrol":
                    {
                        WindowsDeviceEnrolment.IncrementDiscoveryMethod(DeviceEnrolmentServerDiscoveryMethod.Mac);
                        var Binder = ModelBinders.Binders.GetBinder(typeof(MacEnrol));
                        var BinderContext = new ModelBindingContext()
                        {
                            ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(MacEnrol)),
                            ValueProvider = ValueProvider
                        };
                        MacEnrol enrolRequest = (MacEnrol)Binder.BindModel(ControllerContext, BinderContext);

                        MacEnrolResponse enrolResponse = enrolRequest.BuildResponse();

                        return Json(enrolResponse, JsonRequestBehavior.AllowGet);
                    }
                case "macsecureenrol":
                    {
                        WindowsDeviceEnrolment.IncrementDiscoveryMethod(DeviceEnrolmentServerDiscoveryMethod.MacSecure);
                        using (var database = new DiscoDataContext())
                        {
                            var host = HttpContext.Request.UserHostAddress;
                            MacSecureEnrolResponse enrolResponse = MacDeviceEnrolment.SecureEnrol(database, host);
                            database.SaveChanges();
                            return Json(enrolResponse, JsonRequestBehavior.AllowGet);
                        }
                    }
            }
            throw new MissingMethodException($"Unknown Feature: {feature}");
        }

        [Authorize]
        public virtual ActionResult Authenticated(string feature)
        {
            if (!CheckLegacyEnrollmentDiscovery())
                return BadRequest("Enrollment Legacy Discovery is disabled. Please use secure connection (HTTPS) for device enrollment.");

            if (string.IsNullOrEmpty(feature))
            {
                WhoAmIResponse whoAmIResponse = new WhoAmI().BuildResponse();
                return Json(whoAmIResponse, JsonRequestBehavior.AllowGet);
            }
            switch (feature.ToLower())
            {
                case "whoami":
                    {
                        WhoAmIResponse whoAmIResponse = new WhoAmI().BuildResponse();
                        return Json(whoAmIResponse, JsonRequestBehavior.AllowGet);
                    }
                case "enrol":
                    {
                        // Ensure supported version
                        if (Request.UserAgent.StartsWith(@"Disco-Client/", StringComparison.OrdinalIgnoreCase))
                        {
                            if (Version.TryParse(Request.UserAgent.Substring(13), out var clientVersion))
                            {
                                if (clientVersion < new Version(2, 2))
                                {
                                    return BadRequest("Disco ICT Client not compatible");
                                }
                            }
                        }

                        var serializer = new JsonSerializer();
                        Enrol enrolRequest;

                        Request.InputStream.Position = 0;
                        using (var streamReader = new StreamReader(Request.InputStream))
                        {
                            using (var jsonReader = new JsonTextReader(streamReader))
                            {
                                enrolRequest = serializer.Deserialize<Enrol>(jsonReader);
                            }
                        }

                        EnrolResponse enrolResponse = enrolRequest.BuildResponse();
                        return Json(enrolResponse);
                    }
            }
            throw new MissingMethodException($"Unknown Feature: {feature}");
        }

        public virtual ActionResult ClientError(string SessionId, string DeviceIdentifier, string JsonException)
        {

            string clientVersion = Request.UserAgent;
            string clientIP = Request.UserHostAddress;
            string errorMessage;

            try
            {
                var ex = JsonConvert.DeserializeObject<Exception>(JsonException);
                errorMessage = ex.Message;
            }
            catch (Exception)
            {
                try
                {
                    dynamic ex = JsonConvert.DeserializeObject(JsonException);
                    errorMessage = ex.Message;
                }
                catch (Exception)
                {
                    errorMessage = "Unable to determine the error message; Export log for more information";
                }
            }

            if (string.IsNullOrEmpty(SessionId))
                EnrolmentLog.LogClientError(clientIP, DeviceIdentifier, clientVersion, errorMessage, JsonException);
            else
                EnrolmentLog.LogSessionClientError(SessionId, clientIP, DeviceIdentifier, clientVersion, errorMessage, JsonException);

            return Content("Error Message Logged");
        }

        private bool CheckLegacyEnrollmentDiscovery()
        {
            if (!Request.IsSecureConnection)
            {
                using (DiscoDataContext database = new DiscoDataContext())
                {
                    if (database.DiscoConfiguration.Devices.EnrollmentLegacyDiscoveryDisabled)
                    {
                        EnrolmentLog.LogClientError(Request.UserHostAddress, Request.UserHostName, string.Empty, "Enrollment Legacy Discovery is disabled. Please use secure connection (HTTPS) for device enrollment.", string.Empty);
                        return false;
                    }
                }
            }
            return true;
        }

    }
}
