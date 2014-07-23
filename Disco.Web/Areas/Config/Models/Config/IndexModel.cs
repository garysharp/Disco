using Disco.Models.Services.Interop.DiscoServices;
using System;

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
                    var updateVersion = Version.Parse(UpdateResponse.LatestVersion);
                    return (updateVersion > typeof(DiscoApplication).Assembly.GetName().Version);
                }

                return false;
            }
        }

        public UpdateResponseV2 UpdateResponse { get; set; }
    }
}