using System;
using System.ComponentModel.DataAnnotations;

namespace Disco.Web.Areas.API.Models.Activation
{
    public class CallbackModel
    {
        public Guid DeploymentId { get; set; }
        public Guid CorrelationId { get; set; }
        [StringLength(50)]
        public string UserId { get; set; }
    }
}
