using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disco.Data.Repository;
using Disco.Services.Tasks;

namespace Disco.Services.Plugins
{
    public abstract class Plugin : IDisposable
    {
        public PluginManifest Manifest {get; internal set;}

        #region Lifecycle
        // Events/Triggers for Custom Plugin Initialization (Optional)
        public virtual void Install(DiscoDataContext dbContext, ScheduledTaskStatus Status) { return; }
        public virtual void Initialize(DiscoDataContext dbContext) { return; }
        public virtual void Uninstall(DiscoDataContext dbContext, bool UninstallData, ScheduledTaskStatus Status) { return; }
        public virtual void AfterUpdate(DiscoDataContext dbContext, PluginManifest PreviousManifest) { return; }
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
