using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Disco.Web.Models.Job
{
    public class CreateRedirectModel
    {
        public string RedirectLink { get; set; }
        public TimeSpan? RedirectDelay { get; set; }
    }
}