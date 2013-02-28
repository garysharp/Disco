using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disco.Data.Configuration.Modules
{
    public class WirelessConfiguration : ConfigurationBase
    {
        public const string Provider_eduSTAR = "eduSTAR";
        public const string Provider_eduPaSS = "eduPaSS";

        public WirelessConfiguration(ConfigurationContext Context) : base(Context) { }
        
        public override string Scope
        {
            get { return "Wireless"; }
        }

        public int CertificateAutoBufferMax
        {
            get
            {
                return this.GetValue("CertificateAutoBufferMax", 50);
            }
            set
            {
                this.SetValue("CertificateAutoBufferMax", value);
            }
        }
        public int CertificateAutoBufferLow
        {
            get
            {
                return this.GetValue("CertificateAutoBufferLow", 10);
            }
            set
            {
                this.SetValue("CertificateAutoBufferLow", value);
            }
        }
        public string Provider
        {
            get
            {
                return this.GetValue("Provider", Provider_eduSTAR);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");
                if (value.Equals(Provider_eduSTAR, StringComparison.InvariantCultureIgnoreCase))
                    this.SetValue("Provider", Provider_eduSTAR);
                else
                    throw new NotSupportedException(string.Format("Unsupported Wireless Provider: ", value));
            }
        }

        #region eduSTAR Configuration

        public string eduSTAR_Scope
        {
            get { return "Wireless_eduSTAR"; }
        }

        public string eduSTAR_ServiceAccountSchoolId
        {
            get
            {
                return this.Context.GetConfigurationValue<string>(this.eduSTAR_Scope, "ServiceAccountSchoolId", null);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");
                this.Context.SetConfigurationValue(this.eduSTAR_Scope, "ServiceAccountSchoolId", value);
            }
        }
        public string eduSTAR_ServiceAccountUsername
        {
            get
            {
                return this.Context.GetConfigurationValue<string>(this.eduSTAR_Scope, "ServiceAccountUsername", null);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");
                this.Context.SetConfigurationValue(this.eduSTAR_Scope, "ServiceAccountUsername", value);
            }
        }
        public string eduSTAR_ServiceAccountPassword
        {
            get
            {
                return ConfigurationContext.DeobsfucateValue(this.Context.GetConfigurationValue<string>(this.eduSTAR_Scope, "ServiceAccountPassword", null));
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");
                this.Context.SetConfigurationValue(this.eduSTAR_Scope, "ServiceAccountPassword", ConfigurationContext.ObsfucateValue(value));
            }
        }

        #endregion

    }
}
