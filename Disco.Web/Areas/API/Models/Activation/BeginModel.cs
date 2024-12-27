using System;

namespace Disco.Web.Areas.API.Models.Activation
{
    public class BeginModel
    {
        public Guid ActivationId { get; set; }
        public string ChallengeResponse { get; set; }
        public string ChallengeResponseIv { get; set; }
        public string RedirectUrl { get; set; }
    }
}
