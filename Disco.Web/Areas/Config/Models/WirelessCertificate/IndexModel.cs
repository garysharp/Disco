using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.Config.Models.WirelessCertificate
{
    public class IndexModel
    {
        public int Total { get; set; }
        public int Unallocated { get; set; }
        public int Allocated { get; set; }
        public string Provider { get; set; }
        public int AutoBufferMax { get; set; }
        public int AutoBufferLow { get; set; }
        public bool Processing { get; set; }

        public string eduSTAR_SchoolId { get; set; }
        public string eduSTAR_Username { get; set; }
    }
}