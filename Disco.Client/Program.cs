using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Disco.Models.ClientServices;
using Disco.Client.Extensions;

namespace Disco.Client
{
    public static class Program
    {
        public static bool IsAuthenticated { get; set; }
        public static bool RebootRequired { get; set; }
        public static bool AllowUninstall { get; set; }

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
                } while (!System.Diagnostics.Debugger.IsAttached);
            }
#endif

            // Initialize Environment Settings
            SetupEnvironment();

            // Report to Bootstrapper
            Presentation.WriteBanner();

            // WhoAmI Phase
            keepProcessing = Program.WhoAmI();

            // Enrol Phase
            if (keepProcessing)
                keepProcessing = Program.Enrol();

            // End conversation with Bootstrapper
            Presentation.WriteFooter(Program.RebootRequired, Program.AllowUninstall, !keepProcessing);
        }

        public static void SetupEnvironment()
        {
            // Hookup Unhandled Error Handling
            AppDomain.CurrentDomain.UnhandledException += ErrorReporting.CurrentDomain_UnhandledException;

            // Ignore Local Proxies
            WebRequest.DefaultWebProxy = new WebProxy();
            // Override Http 100 Continue Behaviour
            ServicePointManager.Expect100Continue = false;

            // Assume success unless otherwise notified
            AllowUninstall = true;

            // Detect Disco.Bootstrapper - Create Enable UI Delay if Running
            try
            {
                Presentation.DelayUI = (System.Diagnostics.Process.GetProcessesByName("Disco.ClientBootstrapper").Length > 0);
            }
            catch (Exception)
            {
                Presentation.DelayUI = true; // Add Delays on Error
            }
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

                // Send Request
                Presentation.UpdateStatus("Enrolling Device", "Sending the enrolment request to the server.", true, -1);
                response = request.Post(Program.IsAuthenticated);

                // Process Response
                Presentation.UpdateStatus("Enrolling Device", "Processing the enrolment response from the server.", true, -1);
                response.Process();

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
