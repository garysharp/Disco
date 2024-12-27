using System;

namespace Disco.Models.Services.Interop.DiscoServices.Activation
{
    public class CallbackModel
    {
        public Guid DeploymentId { get; set; }
        public Guid CorrelationId { get; set; }
        public string User { get; set; }
    }
}
