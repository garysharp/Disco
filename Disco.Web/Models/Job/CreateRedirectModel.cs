using System;

namespace Disco.Web.Models.Job
{
    public class CreateRedirectModel
    {
        public string RedirectLink { get; set; }
        public TimeSpan? RedirectDelay { get; set; }
        public int JobId { get; set; }
    }
}