using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Disco.Data.Repository;

namespace Disco.Services.Plugins
{
    public abstract class PluginConfigurationHandler : IDisposable
    {
        public PluginManifest Manifest { get; set; }

        public abstract PluginConfigurationHandlerGetResponse Get(DiscoDataContext Database, Controller controller);
        public abstract bool Post(DiscoDataContext Database, FormCollection form, Controller controller);

        public virtual void Dispose()
        {
            // Nothing in Base Class
        }

        [Obsolete("Use: Response<ViewType>(dynamic ViewModel)")]
        protected PluginConfigurationHandlerGetResponse GetResponse(Type ViewType, dynamic ViewModel = null)
        {
            return new PluginConfigurationHandlerGetResponse(this.Manifest, ViewType, ViewModel);
        }

        protected PluginConfigurationHandlerGetResponse Response<ViewType>(dynamic Model = null) where ViewType : WebViewPage
        {
            return new PluginConfigurationHandlerGetResponse(this.Manifest, typeof(ViewType), Model);
        }

        public class PluginConfigurationHandlerGetResponse
        {
            public PluginManifest Manifest { get; set; }
            public Type ViewType { get; set; }
            public dynamic Model { get; set; }

            public PluginConfigurationHandlerGetResponse(PluginManifest Manifest, Type ViewType, dynamic Model = null)
            {
                if (ViewType == null)
                    throw new ArgumentNullException("ViewType");
                if (!typeof(WebViewPage).IsAssignableFrom(ViewType))
                    throw new ArgumentException("The PluginConfigurationHandler ViewType must inherit System.Web.Mvc.WebViewPage", "ViewType");

                this.Manifest = Manifest;

                this.ViewType = ViewType;
                this.Model = Model;
            }
        }
    }
}
