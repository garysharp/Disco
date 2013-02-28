using Disco.BI.Wireless.eduSTAR;
using Disco.Data.Configuration;
using Disco.Data.Repository;
using Disco.BI.Extensions;
using Disco.Models.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
namespace Disco.BI.Wireless
{
    public abstract class BaseWirelessProvider
    {
        protected DiscoDataContext dbContext;
        private static object _CertificateAllocateLock = System.Runtime.CompilerServices.RuntimeHelpers.GetObjectValue(new object());
        public static BaseWirelessProvider GetProvider(DiscoDataContext dbContext)
        {
            string provider = dbContext.DiscoConfiguration.Wireless.Provider;
            if (provider == "eduSTAR")
            {
                return new eduSTARWirelessProvider(dbContext);
            }
            throw new System.NotSupportedException(string.Format("Wireless Provider Not Supported: '{0}'", dbContext.DiscoConfiguration.Wireless.Provider));
        }
        protected BaseWirelessProvider(DiscoDataContext dbContext)
        {
            this.dbContext = dbContext;
        }
        private DeviceCertificate CertificateAllocate(ref Device repoDevice)
        {
            lock (BaseWirelessProvider._CertificateAllocateLock)
            {
                this.FillCertificateAutoBuffer();
                int timeout = 60;
                int freeCertCount = this.dbContext.DeviceCertificates.Where(c => c.DeviceSerialNumber == null && c.Enabled).Count();
                while (!(freeCertCount > 0 | timeout <= 0))
                {
                    System.Threading.Thread.Sleep(500);
                    freeCertCount = this.dbContext.DeviceCertificates.Where(c => c.DeviceSerialNumber == null && c.Enabled).Count();
                    timeout--;
                }
                DeviceCertificate cert = this.dbContext.DeviceCertificates.Where(c => c.DeviceSerialNumber == null && c.Enabled).FirstOrDefault();
                if (cert == null)
                {
                    WirelessCertificatesLog.LogAllocationFailed(repoDevice.SerialNumber);
                    throw new System.InvalidOperationException("Unable to Allocate a Wireless Certificate");
                }
                WirelessCertificatesLog.LogAllocated(cert.Name, repoDevice.SerialNumber);
                cert.DeviceSerialNumber = repoDevice.SerialNumber;
                cert.AllocatedDate = System.DateTime.Now;
                this.dbContext.SaveChanges();
                return cert;
            }
        }
        public DeviceCertificate Enrol(Device repoDevice)
        {
            DeviceCertificate allocatedCert = this.dbContext.DeviceCertificates.Where(c => c.DeviceSerialNumber == repoDevice.SerialNumber && c.Enabled).FirstOrDefault();
            if (allocatedCert != null)
            {
                return allocatedCert;
            }
            
            // Removed 2012-06-14 G# - Properties moved to DeviceProfile model & DB Migrated in DBv3.
            //if (repoDevice.DeviceProfile.Configuration(this.dbContext).AllocateWirelessCertificate)
            if (repoDevice.DeviceProfile.AllocateCertificate)
            {
                allocatedCert = this.CertificateAllocate(ref repoDevice);
                return allocatedCert;
            }
            else
            {
                return null;
            }
        }
        protected abstract void FillCertificateAutoBuffer();
        public abstract void FillCertificateBuffer(int Amount);
        public abstract System.Collections.Generic.List<string> RemoveExistingCertificateNames();
    }
}
