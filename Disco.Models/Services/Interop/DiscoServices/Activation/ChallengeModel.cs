using System;

namespace Disco.Models.Services.Interop.DiscoServices.Activation
{
    public class ChallengeModel
    {
        public byte[] Key { get; set; }
        public Guid ActivationId { get; set; }
        public string UserId { get; set; }
        public long TimeStamp { get; set; }
        public byte[] ChallengeResponse { get; set; }
        public byte[] ChallengeResponseIv { get; set; }
        public string RedirectUrl { get; set; }
    }
}
