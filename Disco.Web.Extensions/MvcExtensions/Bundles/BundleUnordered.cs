using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Optimization;

namespace Disco.Web.Extensions.MvcExtensions.Bundles
{
    public class BundleUnordered : IBundleOrderer
    {
        public IEnumerable<System.IO.FileInfo> OrderFiles(BundleContext context, IEnumerable<System.IO.FileInfo> files)
        {
            return files;
        }
    }
}
