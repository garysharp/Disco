using Disco.Client.Extensions;
using Disco.Client.Interop;
using Disco.Models.ClientServices;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace Disco.Client
{
    public static class Program
    {
        public static bool IsAuthenticated { get; set; }
        public static bool RebootRequired { get; set; }
        public static bool AllowUninstall { get; set; }
        public static int BootstrapperVersion { get; private set; } = 1;
        public static int BootstrapperProcessId { get; private set; } = -1;
        public static Uri ServerUrl { get; private set; }

        [STAThread]
        public static void Main(string[] args)
        {
            bool keepProcessing;

#if DEBUG
            if (args != null && args.Any(a => a.Equals("debug", StringComparison.OrdinalIgnoreCase)))
            {
                do
                {
                    Console.WriteLine("Waiting for Debugger to Attach");
                    System.Threading.Thread.Sleep(1000);
                } while (!Debugger.IsAttached);
            }
#endif

            // Initialize Environment Settings
            SetupEnvironment(args);

            if (ServerUrl == null)
                keepProcessing = DiscoverDiscoIct();

            // Report to Bootstrapper
            Presentation.WriteBanner();

            // WhoAmI Phase
            keepProcessing = WhoAmI();

            // Enrol Phase
            if (keepProcessing)
                keepProcessing = Enrol();

            // End conversation with Bootstrapper
            Presentation.WriteFooter(RebootRequired, AllowUninstall, !keepProcessing);
        }

        public static void SetupEnvironment(string[] args)
        {
            // Hookup Unhandled Error Handling
            AppDomain.CurrentDomain.UnhandledException += ErrorReporting.CurrentDomain_UnhandledException;

            // Ignore Local Proxies
            WebRequest.DefaultWebProxy = new WebProxy();
            // Override Http 100 Continue Behaviour
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            // Assume success unless otherwise notified
            AllowUninstall = true;

            if (args != null && args.Length == 3)
            {
                // Parse Bootstrapper Version
                int parsedVersion;
                if (int.TryParse(args[0], out parsedVersion))
                    BootstrapperVersion = parsedVersion;
                // Parse Bootstrapper Process ID
                int parsedProcessId;
                if (int.TryParse(args[1], out parsedProcessId))
                    BootstrapperProcessId = parsedProcessId;
                // Parse Server URL
                Uri parsedUri;
                if (Uri.TryCreate(args[2], UriKind.Absolute, out parsedUri))
                    ServerUrl = parsedUri;
            }
            else
            {
                BootstrapperVersion = 1;
                BootstrapperProcessId = -1;
                ServerUrl = null;
            }

            // Detect Disco.Bootstrapper - Create Enable UI Delay if Running
            Presentation.DelayUI = false;
            try
            {
                if (BootstrapperProcessId != -1)
                {
                    var parentProcess = Process.GetProcessById(BootstrapperProcessId);
                    Presentation.DelayUI = !parentProcess.HasExited;
                }
            }
            catch (Exception)
            {
            }
        }

        public static bool DiscoverDiscoIct()
        {
            try
            {
                Presentation.UpdateStatus("Detecting Disco ICT", "Locating Disco ICT Server, Please wait...", true, -1);
                Presentation.TryDelay(3000);
                ServerUrl = EndpointDiscovery.DiscoverServer(null).Item1;

                // Complete
                return true;
            }
            catch (Exception ex)
            {
                ErrorReporting.ReportError(ex, false);
            }
            return false;
        }

        public static bool WhoAmI()
        {
            try
            {

                WhoAmIResponse response;
                WhoAmI request;

                // Build Request
                request = new WhoAmI();

                // Send Request
                Presentation.UpdateStatus("Connecting to Preparation Server", "Determining connection authentication, please wait...", true, -1);
                response = request.Post(true);

                // Process Response
                response.Process();

                // Complete
                return true;
            }
            catch (WebException webEx)
            {
                // Check for 'Unauthenticated' Connection
                if ((webEx.Status == WebExceptionStatus.ProtocolError) && ((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.Unauthorized)
                {
                    WhoAmIExtensions.UnauthenticatedResponse();
                    return true;
                }
                else
                {
                    // Some other Web Error
                    ErrorReporting.ReportError(webEx, false);
                }
            }
            catch (Exception ex)
            {
                ErrorReporting.ReportError(ex, true);
            }
            return false;
        }

        public static bool Enrol()
        {
            try
            {
                Enrol request;
                EnrolResponse response = null;

                // Build Request
                Presentation.UpdateStatus("Enrolling Device", "Building enrolment request and preparing to send data to the server.", true, -1);
                request = new Enrol();
                request.Build();

                var startTime = DateTimeOffset.Now;
                do
                {
                    // Send Request
                    Presentation.UpdateStatus("Enrolling Device", "Sending the enrolment request to the server.", true, -1);
                    response = request.Post(IsAuthenticated);

                    // Process Response
                    Presentation.UpdateStatus("Enrolling Device", "Processing the enrolment response from the server.", true, -1);
                    response.Process();

                    if (response.IsPending)
                    {
                        request.PendingSessionId = response.SessionId;
                        request.PendingAuthorization = response.PendingAuthorization;

                        // Session Pending
                        var totalSeconds = (response.PendingTimeout - startTime).TotalSeconds;
                        var secondsConsumed = (DateTimeOffset.Now - startTime).TotalSeconds;
                        var progress = (int)((secondsConsumed / totalSeconds) * 100);

                        Presentation.UpdateStatus($"Pending Device Enrolment Approval: {response.PendingIdentifier}", $"Server: {Program.ServerUrl}{Environment.NewLine}Reason: {response.PendingReason}", true, progress);
                        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(10));
                    }
                    else
                    {
                        // Session Complete
                        break;
                    }
                } while (true);

                // Complete
                return true;
            }
            catch (Exception ex)
            {
                ErrorReporting.ReportError(ex, true);
                return false;
            }
        }

    }
}
