using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Data.Repository;
using System.Web.Mvc;

namespace Disco.Services.Plugins
{
    public abstract class Plugin : IDisposable
    {
        public string Id { get { return this.GetType().Name; } }
        public abstract string Name { get; }
        public abstract string Author { get; }
        public abstract Version Version { get; }

        public abstract Type PluginCategoryType { get; }

        public abstract bool Initalize(DiscoDataContext dbContext);

        public abstract void Dispose();

        public override sealed string ToString()
        {
            return string.Format("{0} ({1}) - v{2}", this.Name, this.Id, this.Version.ToString(3));
        }
    }
}
