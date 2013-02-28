using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disco.Models.ClientServices
{
    public class WhoAmI : ServiceBase<WhoAmIResponse>
    {

        public override string Feature
        {
            get { return "WhoAmI"; }
        }
    }
}
