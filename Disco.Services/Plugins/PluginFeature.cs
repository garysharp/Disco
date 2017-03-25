using System;
using Disco.Data.Repository;

namespace Disco.Services.Plugins
{
    public abstract class PluginFeature : IDisposable
    {
        public PluginFeatureManifest Manifest {get; internal set;}

        // Allow Custom Initialization (Optional)
        public virtual void Initialize(DiscoDataContext Database) { return; }

        public virtual void Dispose()
        {
            // Nothing in Base Class
        }
    }
}
