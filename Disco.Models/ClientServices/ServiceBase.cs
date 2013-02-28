using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disco.Models.ClientServices
{
    public abstract class ServiceBase<ResponseType>
    {
        internal ServiceBase()
        {
        }

        public abstract string Feature { get; }
    }
}
