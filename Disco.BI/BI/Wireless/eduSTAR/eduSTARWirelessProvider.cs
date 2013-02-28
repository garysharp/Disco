using Disco.BI.Wireless.eduSTAR.eduSTARWirelessCertService;
using Disco.Data.Repository;
using Disco.Models.Repository;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;

namespace Disco.BI.Wireless.eduSTAR
{
    public class eduSTARWirelessProvider : BaseWirelessProvider
    {
        private class BulkLoadCertificatesContract
        {
            public int Start { get; set; }
            public int Count { get; set; }
        }
        private static object _BulkLoadThreadLock = new object();
        private static System.Threading.Thread _BulkLoadThread;
        public eduSTARWirelessProvider(DiscoDataContext dbContext)
            : base(dbContext)
        {
        }
        protected override void FillCertificateAutoBuffer()
        {
            int freeCertCount = this.dbContext.DeviceCertificates.Where(c => c.DeviceSerialNumber == null && c.Enabled).Count();
            if (freeCertCount <= this.dbContext.DiscoConfiguration.Wireless.CertificateAutoBufferLow)
            {
                this.BulkLoadCertificates(0);
            }
        }
        public override void FillCertificateBuffer(int Amount)
        {
            this.BulkLoadCertificates(Amount);
        }
        public override System.Collections.Generic.List<string> RemoveExistingCertificateNames()
        {
            return new System.Collections.Generic.List<string>
			{
				"(eduPaSS)", 
				"(CN=Computers, ?DC=services, ?DC=education, ?DC=vic, ?DC=gov, ?DC=au)"
			};
        }
        private void BulkLoadCertificates(int Amount = 0)
        {
            if (eduSTARWirelessProvider._BulkLoadThread == null)
            {
                lock (eduSTARWirelessProvider._BulkLoadThreadLock)
                {
                    if (eduSTARWirelessProvider._BulkLoadThread == null)
                    {
                        int start = 0;
                        if (this.dbContext.DeviceCertificates.Count() > 0)
                        {
                            start = this.dbContext.DeviceCertificates.Max(c => c.ProviderIndex) + 1;
                        }
                        int buffer = this.dbContext.DeviceCertificates.Count(c => c.DeviceSerialNumber == null && c.Enabled);
                        int count = this.dbContext.DiscoConfiguration.Wireless.CertificateAutoBufferMax - buffer;
                        if (Amount > 0)
                        {
                            count = Amount;
                        }
                        if (count > 0)
                        {
                            eduSTARWirelessProvider.BulkLoadCertificatesContract contract = new eduSTARWirelessProvider.BulkLoadCertificatesContract
                            {
                                Start = start,
                                Count = count
                            };
                            System.Threading.ParameterizedThreadStart threadStart = delegate(object a0)
                            {
                                this.BulkLoadCertificatesStart((eduSTARWirelessProvider.BulkLoadCertificatesContract)a0);
                            }
                            ;
                            eduSTARWirelessProvider._BulkLoadThread = new System.Threading.Thread(threadStart);
                            eduSTARWirelessProvider._BulkLoadThread.Start(contract);
                        }
                    }
                }
            }
        }
        private void BulkLoadCertificatesStart(eduSTARWirelessProvider.BulkLoadCertificatesContract contract)
        {
            try
            {
                WirelessCertificatesLog.LogRetrievalStarting(contract.Count, contract.Start, contract.Start + contract.Count - 1);
                WirelessCertificatesLog.LogCertificateRetrievalProgress(true, 0, string.Format("Starting Bulk Retrieval (Loading {0} Certificate/s)", contract.Count));
                DiscoDataContext dbLocalContext = new DiscoDataContext();
                try
                {
                    WirelessCertServiceSoapClient proxy = this.GetProxy();
                    try
                    {
                        int num = contract.Start + contract.Count - 1;
                        int index = contract.Start;
                        while (true)
                        {
                            int num2 = num;
                            if (index > num2)
                            {
                                break;
                            }
                            WirelessCertificatesLog.LogCertificateRetrievalProgress(true, (int)System.Math.Round(unchecked(((double)checked(index - contract.Start) + 0.5) / (double)contract.Count * 100.0)), string.Format("Retrieving Certificate {0} of {1}", index - contract.Start + 1, contract.Count));
                            DeviceCertificate cert = this.LoadCertificate(index, proxy, dbLocalContext);
                            dbLocalContext.DeviceCertificates.Add(cert);
                            dbLocalContext.SaveChanges();
                            WirelessCertificatesLog.LogRetrievalCertificateFinished(cert.Name);
                            index++;
                        }
                    }
                    finally
                    {
                        bool flag = proxy != null;
                        if (flag)
                        {
                            ((System.IDisposable)proxy).Dispose();
                        }
                    }
                }
                finally
                {
                    bool flag = dbLocalContext != null;
                    if (flag)
                    {
                        ((System.IDisposable)dbLocalContext).Dispose();
                    }
                }
            }
            catch (System.Exception ex)
            {
                WirelessCertificatesLog.LogRetrievalError(string.Format("[{0}] {1}", ex.GetType().Name, ex.Message));
                throw ex;
            }
            finally
            {
                lock (eduSTARWirelessProvider._BulkLoadThreadLock)
                {
                    eduSTARWirelessProvider._BulkLoadThread = null;
                }
                WirelessCertificatesLog.LogRetrievalFinished();
                WirelessCertificatesLog.LogCertificateRetrievalProgress(false, null, null);
            }
        }
        private DeviceCertificate LoadCertificate(int Index, DiscoDataContext dbContext)
        {
            DeviceCertificate LoadCertificate;
            try
            {
                WirelessCertServiceSoapClient proxy = this.GetProxy();
                try
                {
                    LoadCertificate = this.LoadCertificate(Index, proxy, dbContext);
                }
                finally
                {
                    bool flag = proxy != null;
                    if (flag)
                    {
                        ((System.IDisposable)proxy).Dispose();
                    }
                }
            }
            catch (System.Exception ex)
            {
                WirelessCertificatesLog.LogRetrievalCertificateError(Index.ToString(), string.Format("[{0}] {1}", ex.GetType().Name, ex.Message));
                throw ex;
            }
            return LoadCertificate;
        }
        private DeviceCertificate LoadCertificate(int Index, WirelessCertServiceSoapClient Proxy, DiscoDataContext dbContext)
        {
            bool flag = string.IsNullOrWhiteSpace(dbContext.DiscoConfiguration.Wireless.eduSTAR_ServiceAccountSchoolId);
            if (flag)
            {
                throw new System.ArgumentException("Wireless Certificates: Invalid ServiceAccount SchoolId");
            }
            flag = string.IsNullOrWhiteSpace(dbContext.DiscoConfiguration.Wireless.eduSTAR_ServiceAccountUsername);
            if (flag)
            {
                throw new System.ArgumentException("Wireless Certificates: Invalid ServiceAccount Username");
            }
            flag = string.IsNullOrWhiteSpace(dbContext.DiscoConfiguration.Wireless.eduSTAR_ServiceAccountPassword);
            if (flag)
            {
                throw new System.ArgumentException("Wireless Certificates: Invalid ServiceAccount Password");
            }
            DeviceCertificate cert = new DeviceCertificate
            {
                ProviderIndex = Index,
                Name = string.Format("{0}-{1}", dbContext.DiscoConfiguration.Wireless.eduSTAR_ServiceAccountSchoolId, Index.ToString("00000")),
                Enabled = true
            };
            WirelessCertificatesLog.LogRetrievalCertificateStarting(cert.Name);
            string response;
            try
            {
                response = Proxy.GetWirelessCert(dbContext.DiscoConfiguration.Wireless.eduSTAR_ServiceAccountSchoolId, cert.Name, "password", dbContext.DiscoConfiguration.Wireless.eduSTAR_ServiceAccountUsername, dbContext.DiscoConfiguration.Wireless.eduSTAR_ServiceAccountPassword);
            }
            catch (System.Exception ex)
            {
                WirelessCertificatesLog.LogRetrievalCertificateError(cert.Name, ex.Message);
                throw ex;
            }
            try
            {
                byte[] responseBytes = System.Convert.FromBase64String(response);
                System.IO.MemoryStream responseByteStream = new System.IO.MemoryStream(responseBytes);
                try
                {
                    ZipFile responseZip = ZipFile.Read(responseByteStream);
                    ZipEntry certFile = responseZip.FirstOrDefault((ZipEntry ze) => ze.FileName.EndsWith(".pfx", System.StringComparison.InvariantCultureIgnoreCase));
                    System.IO.MemoryStream certByteStream = new System.IO.MemoryStream();
                    try
                    {
                        certFile.Extract(certByteStream);
                        cert.Content = certByteStream.ToArray();
                    }
                    finally
                    {
                        flag = (certByteStream != null);
                        if (flag)
                        {
                            ((System.IDisposable)certByteStream).Dispose();
                        }
                    }
                }
                finally
                {
                    flag = (responseByteStream != null);
                    if (flag)
                    {
                        ((System.IDisposable)responseByteStream).Dispose();
                    }
                }
            }
            catch (System.Exception ex2)
            {
                if (response.Contains("Computer with this name already exists"))
                {
                    WirelessCertificatesLog.LogRetrievalCertificateWarning(cert.Name, "Already exists on eduSTAR server, disabling and skipping.");
                    cert.ExpirationDate = System.DateTime.Now;
                    cert.Enabled = false;
                    cert.Content = null;
                    return cert;
                }
                throw new System.InvalidOperationException(string.Format("Unable to Uncompress (Server returned: {0})", response), ex2);
            }
            try
            {
                X509Certificate2 x509Cert = new X509Certificate2(cert.Content, "password");
                cert.ExpirationDate = x509Cert.NotAfter;
            }
            catch (System.Exception ex3)
            {
                throw new System.InvalidOperationException("Invalid Certificate returned by Server", ex3);
            }
            return cert;
        }
        private WirelessCertServiceSoapClient GetProxy()
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            
            // Don't Use Proxy
            binding.UseDefaultWebProxy = false;
            binding.ProxyAddress = null;

            binding.Security.Mode = BasicHttpSecurityMode.Transport;
            binding.MaxReceivedMessageSize = 524288L;
            binding.ReaderQuotas.MaxStringContentLength = 524288;
            EndpointAddress endpointAddress = new EndpointAddress(new Uri("https://www.eduweb.vic.gov.au/edustar/WirelessCertWS/wirelesscertws.asmx"), new AddressHeader[0]);
            return new WirelessCertServiceSoapClient(binding, endpointAddress);
        }
    }
}
