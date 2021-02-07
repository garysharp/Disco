using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Disco.Models.Services.Plugins.Details
{
    public class DetailsResult
    {
        public DateTime GatheredOn { get; private set; }
        public DateTime ExpiresOn { get; private set; }
        public Dictionary<string, string> Details { get; }

        public bool SetExpiration(DateTime expireOn)
        {
            if (ExpiresOn > expireOn)
            {
                // only set the expiration if it is sooner
                ExpiresOn = expireOn;
                return true;
            }
            else
            {
                return false;
            }
        }

        public DetailsResult()
        {
            GatheredOn = DateTime.Now;
            ExpiresOn = DateTime.Now.AddDays(7);
            Details = new Dictionary<string, string>();
        }

        [JsonConstructor]
        public DetailsResult(DateTime gatheredOn, DateTime expiresOn, Dictionary<string, string> details)
        {
            GatheredOn = gatheredOn;
            ExpiresOn = expiresOn;
            Details = details ?? new Dictionary<string, string>();
        }
    }
}
