using Disco.Models.ClientServices.EnrolmentInformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Disco.Client.Interop
{
    public static class Certificates
    {
        public static List<Certificate> GetAllCertificates()
        {
            var certificates = new List<Certificate>();

            // Trusted Root Certificates
            certificates.AddRange(GetCertificates(StoreName.Root, "TrustedRoot"));

            // Intermediate Certificates
            certificates.AddRange(GetCertificates(StoreName.CertificateAuthority, "Intermediate"));

            // Personal Certificates
            certificates.AddRange(GetCertificates(StoreName.My, "Personal"));

            return certificates;
        }

        private static IEnumerable<Certificate> GetCertificates(StoreName StoreName, string StoreDescription)
        {
            var store = new X509Store(StoreName, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            try
            {
                foreach (var certificate in store.Certificates)
                {
                    yield return new Certificate()
                    {
                        Store = StoreDescription,
                        SubjectName = certificate.SubjectName.Name,
                        Thumbprint = certificate.Thumbprint,
                        FriendlyName = certificate.FriendlyName,
                        DnsName = certificate.GetNameInfo(X509NameType.DnsName, false),
                        Version = certificate.Version,
                        SignatureAlgorithm = certificate.SignatureAlgorithm.FriendlyName,
                        Issuer = certificate.IssuerName.Name,
                        NotAfter = certificate.NotAfter,
                        NotBefore = certificate.NotBefore,
                        HasPrivateKey = certificate.HasPrivateKey
                    };
                }
            }
            finally
            {
                store.Close();
            }

        }

        public static void Apply(this CertificateStore EnrolStore)
        {
            if (EnrolStore != null)
            {
                // Apply Trusted Root
                ApplyToStore(StoreName.Root, EnrolStore.TrustedRootCertificates, EnrolStore.TrustedRootRemoveThumbprints);

                // Apply Intermediate
                ApplyToStore(StoreName.CertificateAuthority, EnrolStore.IntermediateCertificates, EnrolStore.IntermediateRemoveThumbprints);

                // Apply Personal
                ApplyToStore(StoreName.My, EnrolStore.PersonalCertificates, EnrolStore.PersonalRemoveThumbprints);
            }
        }

        private static void ApplyToStore(StoreName StoreName, List<byte[]> Certificates, List<string> RemoveThumbprints)
        {

            if ((Certificates != null && Certificates.Count > 0) ||
                (RemoveThumbprints != null && RemoveThumbprints.Count > 0))
            {
                var store = new X509Store(StoreName, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                try
                {
                    var addedThumbprints = new List<string>();
                    var existingThumbprints = store.Certificates.Cast<X509Certificate2>().GroupBy(c => c.Thumbprint).ToDictionary(c => c.Key, c => c.ToList(), StringComparer.OrdinalIgnoreCase);

                    // Add
                    if (Certificates != null && Certificates.Count > 0)
                    {
                        foreach (var certificateBytes in Certificates)
                        {
                            var certificate = new X509Certificate2(certificateBytes, "password", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);

                            // check if it already exists
                            if (!existingThumbprints.ContainsKey(certificate.Thumbprint) && !addedThumbprints.Contains(certificate.Thumbprint))
                            {
                                Presentation.UpdateStatus("Enrolling Device", $"Configuring Certificates\r\nAdding Certificate: '{certificate.GetNameInfo(X509NameType.DnsName, false)}' from {store.Name}@{store.Location}", true, -1, 1000);
                                store.Add(certificate);
                                addedThumbprints.Add(certificate.Thumbprint);
                            }
                        }
                    }

                    // Remove
                    if (RemoveThumbprints != null && RemoveThumbprints.Count > 0)
                    {
                        foreach (var thumbprint in RemoveThumbprints)
                        {
                            List<X509Certificate2> certificates;
                            if (existingThumbprints.TryGetValue(thumbprint, out certificates) && !addedThumbprints.Contains(thumbprint))
                            {
                                foreach (var certificate in certificates)
                                {
                                    Presentation.UpdateStatus("Enrolling Device", $"Configuring Certificates\r\nRemoving Certificate: '{certificate.GetNameInfo(X509NameType.DnsName, false)}' from {store.Name}@{store.Location}", true, -1, 1000);
                                    store.Remove(certificate);
                                    existingThumbprints.Remove(thumbprint);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    store.Close();
                }
            }

        }

    }
}
