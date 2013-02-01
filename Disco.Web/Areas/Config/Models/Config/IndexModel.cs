using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.Models.BI.Interop.Community;

namespace Disco.Web.Areas.Config.Models.Config
{
    public class IndexModel
    {
        public bool UpdateAvailable
        {
            get
            {
                if (UpdateResponse != null)
                {
                    var updateVersion = Version.Parse(UpdateResponse.Version);
                    return (updateVersion > typeof(DiscoApplication).Assembly.GetName().Version);
                }

                return false;
            }
        }
        public UpdateResponse UpdateResponse { get; set; }
    }
}