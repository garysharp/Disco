using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disco.Data.Repository;

namespace Disco.Services.Plugins
{
    public abstract class Plugin : IDisposable
    {
        public PluginManifest Manifest {get; internal set;}

        #region Lifecycle
        public abstract bool Install(DiscoDataContext dbContext);
        public abstract bool Initalize(DiscoDataContext dbContext);
        public abstract bool Uninstall(DiscoDataContext dbContext);
        public abstract bool BeforeUpdate(DiscoDataContext dbContext, PluginManifest updateManifest);
        #endregion

        public virtual void Dispose()
        {
            // Nothing in Base Class
        }

        public override sealed string ToString()
        {
            return string.Format("{0} ({1}) - v{2}", this.Manifest.Name, this.Manifest.Id, this.Manifest.Version.ToString(4));
        }
    }
}
