using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using Disco.Client.Extensions;
using Newtonsoft.Json;

namespace Disco.Client
{
    public static class ErrorReporting
    {
        private const string ServicePathTemplate = "http://DISCO:9292/Services/Client/ClientError";
        public static string DeviceIdentifier { get; set; }
        public static string EnrolmentSessionId { get; set; }

        public static void ReportError(Exception Ex, bool ReportToServer)
        {
            bool isClientServiceException = Ex is ClientServiceException;

            ErrorReport report = new ErrorReport()
            {
                DeviceIdentifier = DeviceIdentifier,
                SessionId = EnrolmentSessionId,
                JsonException = Ex.IntenseExceptionSerialization()
            };

            try
            {
                LogToFile(report);
            }
            catch (Exception) { }

            try
            {
                LogToEventLog(report);
            }
            catch (Exception) { }

            // Don't log server errors back to the server
            if (!isClientServiceException && ReportToServer)
            {
                try
                {
                    LogToServer(report);
                }
                catch (Exception) { }
            }

            try
            {
                Presentation.WriteFatalError(Ex);
            }
            catch (Exception) { }
        }

        #region Log Actions

        private static void LogToFile(ErrorReport report)
        {
            var logPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ErrorLog.csv");

            using (var streamWriter = File.AppendText(logPath))
            {
                streamWriter.Write(DateTime.Now.ToString("s"));
                streamWriter.Write(",");
                streamWriter.Write(report.DeviceIdentifier);
                streamWriter.Write(",\"");
                streamWriter.Write(report.JsonException);
                streamWriter.Write("\"");
                streamWriter.Flush();
            }
        }
        private static void LogToEventLog(ErrorReport report)
        {
            string eventSource = "Disco Client";

            if (!EventLog.SourceExists(eventSource))
                EventLog.CreateEventSource(eventSource, "Application");

            EventLog.WriteEntry(eventSource, report.JsonException, EventLogEntryType.Error, 400);
        }
        private static void LogToServer(ErrorReport report)
        {
            string reportJson = JsonConvert.SerializeObject(report);
            string reportResponse;

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(ServicePathTemplate);
            request.UserAgent = $"Disco-Client/{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}";
            request.ContentType = "application/json";
            request.Method = WebRequestMethods.Http.Post;
            request.UseDefaultCredentials = true;
            request.Timeout = 300000; // 5 Minutes

            using (StreamWriter requestWriter = new StreamWriter(request.GetRequestStream()))
            {
                requestWriter.Write(reportJson);
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader responseReader = new StreamReader(response.GetResponseStream()))
                {
                    reportResponse = responseReader.ReadToEnd();
                }
            }

            System.Diagnostics.Debug.WriteLine("Error Report Logged to Server; Response: {0}", reportResponse);
        }

        #endregion

        public class ErrorReport
        {
            public string SessionId { get; set; }
            public string DeviceIdentifier { get; set; }
            public string JsonException { get; set; }
        }

        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var ex = e.ExceptionObject as Exception;
                if (ex != null)
                {
                    ReportError(ex, true);
                }
            }
            catch (Exception)
            {
                // Igore failure to Log Errors
            }
        }

        #region Exception Serialization
        private static string IntenseExceptionSerialization(this Exception Ex)
        {
            try
            {
                return JsonConvert.SerializeObject(Ex);
            }
            catch (Exception)
            {
                try
                {
                    var encapsulatedEx = Ex.ToEncapsulatedException();
                    return JsonConvert.SerializeObject(encapsulatedEx);
                }
                catch (Exception)
                {
                    try
                    {
                        var encapsulatedEx = Ex.ToEncapsulatedException(0);
                        return JsonConvert.SerializeObject(encapsulatedEx);
                    }
                    catch (Exception)
                    {
                        return JsonConvert.SerializeObject(Ex.Message);
                    }
                }
            }
        }

        private static EncapsulatedException ToEncapsulatedException(this Exception ex, int InnerRecursionDepth = 4)
        {
            EncapsulatedException inner = null;
            if (InnerRecursionDepth > 0 && ex.InnerException != null)
                inner = ex.InnerException.ToEncapsulatedException(--InnerRecursionDepth);

            return new EncapsulatedException()
            {
                EncapsulatedType = ex.GetType().Name,
                Message = ex.Message,
                StackTrace = ex.StackTrace,
                InnerException = inner
            };
        }
        public class EncapsulatedException
        {
            public string Message { get; set; }
            public string EncapsulatedType { get; set; }
            public string StackTrace { get; set; }
            public EncapsulatedException InnerException { get; set; }
        }
        #endregion

    }
}
