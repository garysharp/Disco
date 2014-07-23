using System;

namespace Disco.Models.Services.Interop.DiscoServices
{
    public class UpdateResponseV2
    {
        public string LatestVersion { get; set; }

        public DateTime ReleasedDate { get; set; }
        public string Description { get; set; }
        public bool IsBetaRelease { get; set; }
        public string UrlLink { get; set; }

        public DateTime UpdateResponseDate { get; set; }
    }
}
