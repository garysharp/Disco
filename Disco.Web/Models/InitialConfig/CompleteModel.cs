using System;
using System.Net;

namespace Disco.Web.Models.InitialConfig
{
    public class CompleteModel
    {
        public Tuple<IPHostEntry, Exception> DiscoDnsTestResult { get; set; }
        public Exception RegistryDatabaseResult { get; set; }
        public Exception DiscoIctComAuWebResult { get; set; }

        public bool LaunchAllowed
        {
            get
            {
                return (RegistryDatabaseResult == null);
            }
        }

        public void PerformTests()
        {

            #region DNS for 'Disco'
            try
            {
                // Try and Resolve 'disco'
                DiscoDnsTestResult = Tuple.Create(Dns.GetHostEntry("disco"), (Exception)null);
            }
            catch (Exception ex)
            {
                DiscoDnsTestResult = Tuple.Create((IPHostEntry)null, ex);
            }
            #endregion

            #region Write Database to Registry
            try
            {
                // Make Connection String Persistent
                Data.Repository.DiscoDatabaseConnectionFactory.SetDiscoDataContextConnectionString(
                    Data.Repository.DiscoDatabaseConnectionFactory.DiscoDataContextConnectionString, true);
                RegistryDatabaseResult = null;
            }
            catch (Exception ex)
            {
                RegistryDatabaseResult = ex;
            }
            #endregion

            #region Communicate with https://discoict.com.au/
            try
            {
                Dns.GetHostEntry("discoict.com.au");
                try
                {
                    HttpWebRequest wReq = (HttpWebRequest)WebRequest.Create("https://discoict.com.au/");
                    // Added: 2013-02-08 G#
                    // Fix for Proxy Servers which dont support KeepAlive
                    wReq.KeepAlive = false;
                    // End Added: 2013-02-08 G#
                    wReq.Method = WebRequestMethods.Http.Get;
                    wReq.UserAgent = $"Disco/{DiscoApplication.Version} (Initial Config; Org: {DiscoApplication.OrganisationName})";
                    using (HttpWebResponse wRes = (HttpWebResponse)wReq.GetResponse())
                    {
                        if (wRes.StatusCode == HttpStatusCode.OK)
                        {
                            DiscoIctComAuWebResult = null;
                        }
                        else
                        {
                            DiscoIctComAuWebResult = new Exception($"Server returned response: [{wRes.StatusCode}] {wRes.StatusDescription}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    DiscoIctComAuWebResult = new WebException("DNS Resolved the host 'discoict.com.au' but could not establish a connection.", ex);
                }
            }
            catch (Exception ex)
            {
                DiscoIctComAuWebResult = new Exception("Could not resolve the name 'discoict.com.au'", ex);
            }
            #endregion

        }
    }
}