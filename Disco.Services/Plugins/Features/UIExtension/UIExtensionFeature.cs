using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Disco.Models.UI;
using Disco.Services.Plugins.Features.UIExtension.Results;

namespace Disco.Services.Plugins.Features.UIExtension
{
    [PluginFeatureCategory(DisplayName = "User Interface Extensions")]
    public abstract class UIExtensionFeature<UIModel> : PluginFeature where UIModel : BaseUIModel
    {
        public ControllerContext Context { get; set; }
        private Lazy<WebHelper> plugin;
        protected WebHelper Plugin
        {
            get
            {
                return plugin.Value;
            }
        }
        public UIExtensionFeature()
        {
            this.plugin = new Lazy<WebHelper>(new Func<WebHelper>(() => {
                if (this.Context == null)
                    throw new InvalidOperationException("The Context property is not initialized");

                return new WebHelper(this.Context.HttpContext, this.Manifest.PluginManifest);
            }));
        }

        public abstract UIExtensionResult ExecuteAction(ControllerContext context, UIModel model);

        #region ActionResults

        protected LiteralResult Literal(string Content)
        {
            return new LiteralResult(this.Manifest, Content);
        }
        protected LiteralResult Nothing()
        {
            return new LiteralResult(this.Manifest, null);
        }
        protected LiteralResult ScriptInline(string JavaScriptContent)
        {
            return new LiteralResult(this.Manifest, string.Concat("<script type=\"text/javascript\">\n//<!--\n", JavaScriptContent, "\n//-->\n</script>"));
        }
        protected PluginResourceScriptResult ScriptResource(string Resource, bool PlaceInPageHead)
        {
            return new PluginResourceScriptResult(this.Manifest, Resource, PlaceInPageHead);
        }
        protected PluginResourceCssResult CssResource(string Resource)
        {
            return new PluginResourceCssResult(this.Manifest, Resource);
        }
        protected MultipleResult Multiple(params UIExtensionResult[] Results)
        {
            return new MultipleResult(this.Manifest, Results);
        }
        [Obsolete("Use: PartialCompiled<ViewType>(dynamic Model)")]
        protected PrecompiledPartialViewResult Partial(Type PartialViewType, object Model = null)
        {
            return new PrecompiledPartialViewResult(this.Manifest, PartialViewType, Model);
        }
        protected PrecompiledPartialViewResult PartialCompiled<ViewType>(dynamic Model = null) where ViewType : WebViewPage
        {
            return new PrecompiledPartialViewResult(this.Manifest, typeof(ViewType), Model);
        }

        #endregion

        #region Registration
        public bool Register()
        {
            return UIExtensions.RegisterExtension(this);
        }
        public bool Unregister()
        {
            return UIExtensions.UnregisterExtension(this);
        }
        public bool IsRegistered
        {
            get
            {
                return UIExtensions.ExtensionRegistered(this);
            }
        }
        #endregion
    }
}
