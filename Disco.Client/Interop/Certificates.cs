using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace Disco.Client.Interop
{
    public static class Certificates
    {

        public static string GetCertificateFriendlyName(X509Certificate2 Certificate)
        {
            string subject = Certificate.Subject;
            return subject.Substring(subject.IndexOf("=") + 1, subject.IndexOf(",") - subject.IndexOf("=") - 1);
        }

        public static List<string> GetCertificateSubjects(StoreName StoreName, StoreLocation StoreLocation)
        {
            X509Store certStore = new X509Store(StoreName, StoreLocation);
            certStore.Open(OpenFlags.ReadOnly);
            var certSubjects = certStore.Certificates.Cast<X509Certificate2>().Select(c => c.Subject).ToList();
            certStore.Close();
            return certSubjects;
        }

        public static bool AddCertificate(StoreName StoreName, StoreLocation StoreLocation, X509Certificate2 Certificate)
        {
            X509Store certStore = new X509Store(StoreName, StoreLocation);
            bool certAlreadyAdded = false;

            certStore.Open(OpenFlags.ReadWrite);

            try
            {
                foreach (X509Certificate2 cert in certStore.Certificates)
                {
                    if (cert.SerialNumber.Equals(Certificate.SerialNumber))
                    {
                        certAlreadyAdded = true;
                        break;
                    }
                }

                if (!certAlreadyAdded)
                {
                    Presentation.UpdateStatus("Enrolling Device", string.Format("Configuring Wireless Certificates{0}Adding Certificate: '{1}' from {2}@{3}", Environment.NewLine, GetCertificateFriendlyName(Certificate), StoreName.ToString(), StoreLocation.ToString()), true, -1, 3000);
                    certStore.Add(Certificate);
                }
            }
            catch (Exception) { throw; }
            finally
            {
                certStore.Close();
            }

            return !certAlreadyAdded;
        }

        public static List<string> RemoveCertificates(StoreName StoreName, StoreLocation StoreLocation, List<Regex> RegExMatchesSubject, X509Certificate2 CertificateException)
        {
            X509Store certStore = new X509Store(StoreName, StoreLocation);
            List<string> results = new List<string>();
            List<X509Certificate2> certStoreRemove = new List<X509Certificate2>();

            certStore.Open(OpenFlags.ReadWrite);

            try
            {
                foreach (X509Certificate2 cert in certStore.Certificates)
                {
                    if (!cert.SerialNumber.Equals(CertificateException.SerialNumber))
                    {
                        foreach (var subjectRegEx in RegExMatchesSubject)
                        {
                            if (subjectRegEx.IsMatch(cert.Subject))
                            {
                                certStoreRemove.Add(cert);
                                break;
                            }
                        }
                    }
                }

                foreach (var cert in certStoreRemove)
                {
                    results.Add(cert.Subject);

                    Presentation.UpdateStatus("Enrolling Device", string.Format("Configuring Wireless Certificates{0}Removing Certificate: '{1}' from {2}@{3}", Environment.NewLine, GetCertificateFriendlyName(cert), StoreName.ToString(), StoreLocation.ToString()), true, -1, 1500);
                    certStore.Remove(cert);
                }
            }
            catch (Exception) { throw; }
            finally
            {
                certStore.Close();
            }

            return results;
        }
    }
}
