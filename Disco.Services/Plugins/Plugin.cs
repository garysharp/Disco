using System;
using Disco.Data.Repository;
using Disco.Services.Tasks;

namespace Disco.Services.Plugins
{
    public abstract class Plugin : IDisposable
    {
        public PluginManifest Manifest {get; internal set;}

        #region Lifecycle
        // Events/Triggers for Custom Plugin Initialization (Optional)
        public virtual void Install(DiscoDataContext Database, ScheduledTaskStatus Status) { return; }
        public virtual void Initialize(DiscoDataContext Database) { return; }
        public virtual void Uninstall(DiscoDataContext Database, bool UninstallData, ScheduledTaskStatus Status) { return; }
        public virtual void AfterUpdate(DiscoDataContext Database, PluginManifest PreviousManifest) { return; }
        #endregion

        public virtual void Dispose()
        {
            // Nothing in Base Class
        }

        public override sealed string ToString()
        {
            return string.Format("{0} ({1}) - v{2}", Manifest.Name, Manifest.Id, Manifest.Version.ToString(4));
        }
    }
}
