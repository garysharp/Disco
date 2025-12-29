using System;

namespace Disco.Web.Areas.Config.Models.SystemConfig
{
    public class ActivateModel
    {
        public Guid DeploymentId { get; set; }
        public Guid CorrelationId { get; set; }
        public string UserId { get; set; }
        public long Timestamp { get; set; }
        public string Proof { get; set; }
        public Uri CallbackUrl { get; set; }
    }
}
