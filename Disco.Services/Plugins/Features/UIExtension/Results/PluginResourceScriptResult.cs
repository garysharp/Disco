using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Plugins.Features.UIExtension.Results
{
    public class PluginResourceScriptResult : UIExtensionResult
    {
        private string _resource;

        public PluginResourceScriptResult(PluginFeatureManifest Source, string Resource) : base(Source)
        {
            this._resource = Resource;
        }

        public override void ExecuteResult<T>(System.Web.Mvc.WebViewPage<T> page)
        {
            page.WriteLiteral("<script src=\"");
            page.WriteLiteral(page.DiscoPluginResourceUrl(_resource, false, this.Source.PluginManifest));
            page.WriteLiteral("\" type=\"text/javascript\"></script>");
        }
    }
}
