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
            _content = Content;
        }

        public override void ExecuteResult<T>(WebViewPage<T> page)
        {
            if (!string.IsNullOrEmpty(_content))
                page.Write(new HtmlString(_content));
        }
    }
}
