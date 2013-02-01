using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disco.Data.Repository;

namespace Disco.Services.Plugins
{
    public abstract class PluginFeature : IDisposable
    {
        public PluginFeatureManifest Manifest {get; internal set;}
        
        public abstract bool Initalize(DiscoDataContext dbContext);

        public virtual void Dispose()
        {
            // Nothing in Base Class
        }
    }
}
