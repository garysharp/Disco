using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Disco.Models.BI.Interop.Community
{
    public class UpdateRequestBase
    {
        public virtual int RequestVersion { get; set; }
    }
}
