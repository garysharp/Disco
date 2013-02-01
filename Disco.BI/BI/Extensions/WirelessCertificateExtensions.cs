using Disco.Models.Repository;
using System;
using System.Security.Cryptography.X509Certificates;
namespace Disco.BI.Extensions
{
	public static class WirelessCertificateExtensions
	{
		public static System.DateTime? CertificateExpirationDate(this DeviceCertificate wc)
		{
			if (wc.Content == null || wc.Content.Length == 0)
			{
				return null;
			}
			X509Certificate2 c = new X509Certificate2(wc.Content, "password");
			return c.NotAfter;
		}
	}
}
