using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;

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
                DiscoDnsTestResult = new Tuple<IPHostEntry, Exception>(Dns.GetHostEntry("disco"), null);
            }
            catch (Exception ex)
            {
                DiscoDnsTestResult = new Tuple<IPHostEntry, Exception>(null, ex);
            }
            #endregion

            #region Write Database to Registry
            try
            {
                // Make Connection String Persistent
                Disco.Data.Repository.DiscoDatabaseConnectionFactory.SetDiscoDataContextConnectionString(
                    Disco.Data.Repository.DiscoDatabaseConnectionFactory.DiscoDataContextConnectionString, true);
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
                    HttpWebRequest wReq = (HttpWebRequest)HttpWebRequest.Create("https://discoict.com.au/");
                    // Added: 2013-02-08 G#
                    // Fix for Proxy Servers which dont support KeepAlive
                    wReq.KeepAlive = false;
                    // End Added: 2013-02-08 G#
                    wReq.Method = WebRequestMethods.Http.Get;
                    wReq.UserAgent = string.Format("Disco/{0} (Initial Config; Org: {1})", DiscoApplication.Version, DiscoApplication.OrganisationName);
                    using (HttpWebResponse wRes = (HttpWebResponse)wReq.GetResponse())
                    {
                        if (wRes.StatusCode == HttpStatusCode.OK)
                        {
                            DiscoIctComAuWebResult = null;
                        }
                        else
                        {
                            DiscoIctComAuWebResult = new Exception(string.Format("Server returned response: [{0}] {1}", wRes.StatusCode, wRes.StatusDescription));
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
                throw;
            }
            #endregion

        }
    }
}