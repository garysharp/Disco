using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Disco.ClientBootstrapper.Interop
{
    public static class CertificateInterop
    {
        private static List<string> _tempCerts;
        public static void RemoveTempCerts()
        {
            if (_tempCerts != null && _tempCerts.Count > 0)
            {
                Remove(StoreName.My, StoreLocation.LocalMachine, _tempCerts);
                // dont remove root/intermediate certs as they may be have installed by client
                //Remove(StoreName.CertificateAuthority, StoreLocation.LocalMachine, _tempCerts);
                //Remove(StoreName.Root, StoreLocation.LocalMachine, _tempCerts);
            }
        }
        public static async Task AddTempCerts(CancellationToken cancellationToken)
        {
            if (_tempCerts == null)
                _tempCerts = new List<string>();

            var inlineCertificateLocation = Path.GetDirectoryName(typeof(Program).Assembly.Location);

            // Root Certificates
            try
            {
                var CertFiles = Directory.EnumerateFiles(inlineCertificateLocation, "WLAN_Cert_Root_*.*").ToList();
                if (CertFiles.Count > 0)
                {
                    foreach (var certFile in CertFiles)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var cert = new X509Certificate2(File.ReadAllBytes(certFile), "password");
                        var result = Add(StoreName.Root, StoreLocation.LocalMachine, cert);
                        if (result)
                        {
                            if (Path.GetFileNameWithoutExtension(certFile).ToLower().Contains("temp"))
                                _tempCerts.Add(cert.SerialNumber);
                            Program.Status.UpdateStatus(null, null, $"Added Root Certificate: {cert.ShortSubjectName()}");
                            await Program.SleepThread(500, false, cancellationToken);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            // Intermediate Certificates
            try
            {
                var CertFiles = Directory.EnumerateFiles(inlineCertificateLocation, "WLAN_Cert_Intermediate_*.*").ToList();
                if (CertFiles.Count > 0)
                {
                    foreach (var certFile in CertFiles)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var cert = new X509Certificate2(File.ReadAllBytes(certFile), "password");
                        var result = Add(StoreName.CertificateAuthority, StoreLocation.LocalMachine, cert);
                        if (result)
                        {
                            if (Path.GetFileNameWithoutExtension(certFile).ToLower().Contains("temp"))
                                _tempCerts.Add(cert.SerialNumber);
                            Program.Status.UpdateStatus(null, null, $"Added Intermediate Certificate: {cert.ShortSubjectName()}");
                            await Program.SleepThread(500, false, cancellationToken);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            // Host/Personal Certificates
            try
            {
                var CertFiles = Directory.EnumerateFiles(inlineCertificateLocation, "WLAN_Cert_Personal_*.*").ToList();
                if (CertFiles.Count > 0)
                {
                    foreach (var certFile in CertFiles)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var cert = new X509Certificate2(File.ReadAllBytes(certFile), "password", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
                        var result = Add(StoreName.My, StoreLocation.LocalMachine, cert);
                        if (result)
                        {
                            if (Path.GetFileNameWithoutExtension(certFile).ToLower().Contains("temp"))
                                _tempCerts.Add(cert.SerialNumber);
                            Program.Status.UpdateStatus(null, null, $"Added Host Certificate: {cert.ShortSubjectName()}");
                            await Program.SleepThread(500, false, cancellationToken);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string ShortSubjectName(this X509Certificate2 Certificate)
        {
            string s = Certificate.Subject;
            if (string.IsNullOrWhiteSpace(s))
            {
                return $"Unknown Certificate: {Certificate.Thumbprint}";
            }
            else
            {
                if (s.Length > 3 && s.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
                {
                    var nameLength = s.IndexOf(',') - 3;
                    if (nameLength > 0)
                    {
                        return s.Substring(3, nameLength);
                    }
                    else
                    {
                        return s.Substring(3);
                    }
                }
                return s;
            }
        }

        public static bool Add(StoreName StoreName, StoreLocation StoreLocation, X509Certificate2 Certificate)
        {
            var certStore = new X509Store(StoreName, StoreLocation);
            bool certAlreadyExists = false;
            certStore.Open(OpenFlags.ReadWrite);
            foreach (var cert in certStore.Certificates)
            {
                if (cert.SerialNumber.Equals(Certificate.SerialNumber))
                {
                    certAlreadyExists = true;
                    break;
                }
            }
            if (!certAlreadyExists)
            {
                certStore.Add(Certificate);
            }
            certStore.Close();
            return !certAlreadyExists;
        }

        public static bool Remove(StoreName StoreName, StoreLocation StoreLocation, List<Regex> RegexMatches, string SerialException)
        {
            var certStore = new X509Store(StoreName, StoreLocation);
            var removeCerts = new List<X509Certificate2>();
            certStore.Open(OpenFlags.ReadWrite);
            foreach (var cert in certStore.Certificates)
            {
                if (!cert.SerialNumber.Equals(SerialException))
                {
                    foreach (var subjectRegex in RegexMatches)
                    {
                        if (subjectRegex.IsMatch(cert.Subject))
                        {
                            removeCerts.Add(cert);
                            break;
                        }
                    }
                }
            }
            foreach (var cert in removeCerts)
            {
                certStore.Remove(cert);
            }
            certStore.Close();
            return (removeCerts.Count > 0);
        }
        public static bool Remove(StoreName StoreName, StoreLocation StoreLocation, List<string> CertificateSerials)
        {
            var certStore = new X509Store(StoreName, StoreLocation);
            var removeCerts = new List<X509Certificate2>();
            certStore.Open(OpenFlags.ReadWrite);
            foreach (var cert in certStore.Certificates)
            {
                if (CertificateSerials.Contains(cert.SerialNumber))
                {
                    removeCerts.Add(cert);
                }
            }
            foreach (var cert in removeCerts)
            {
                certStore.Remove(cert);
            }
            certStore.Close();
            return (removeCerts.Count > 0);
        }

    }
}
