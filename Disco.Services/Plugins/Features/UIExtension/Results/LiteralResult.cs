using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Disco.Services.Plugins.Features.UIExtension.Results
{
    public class LiteralResult : UIExtensionResult
    {
        private string _content;

        public LiteralResult(PluginFeatureManifest Source, string Content)
            : base(Source)
        {
            this._content = Content;
        }

        public override void ExecuteResult<T>(WebViewPage<T> page)
        {
            if (!string.IsNullOrEmpty(_content))
                page.Write(new HtmlString(_content));
        }
    }
}
