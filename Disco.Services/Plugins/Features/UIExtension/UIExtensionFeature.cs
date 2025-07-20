using Disco.Models.UI;
using Disco.Services.Plugins.Features.UIExtension.Results;
using System;
using System.Web.Mvc;

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
            plugin = new Lazy<WebHelper>(new Func<WebHelper>(() =>
            {
                if (Context == null)
                    throw new InvalidOperationException("The Context property is not initialized");

                return new WebHelper(Context.HttpContext, Manifest.PluginManifest);
            }));
        }

        public abstract UIExtensionResult ExecuteAction(ControllerContext context, UIModel model);

        #region ActionResults

        protected LiteralResult Literal(string Content)
        {
            return new LiteralResult(Manifest, Content);
        }
        protected LiteralResult Nothing()
        {
            return new LiteralResult(Manifest, null);
        }
        protected LiteralResult ScriptInline(string JavaScriptContent)
        {
            return new LiteralResult(Manifest, string.Concat("<script type=\"text/javascript\">\n//<!--\n", JavaScriptContent, "\n//-->\n</script>"));
        }
        protected PluginResourceScriptResult ScriptResource(string Resource, bool PlaceInPageHead)
        {
            return new PluginResourceScriptResult(Manifest, Resource, PlaceInPageHead);
        }
        protected PluginResourceCssResult CssResource(string Resource)
        {
            return new PluginResourceCssResult(Manifest, Resource);
        }
        protected MultipleResult Multiple(params UIExtensionResult[] Results)
        {
            return new MultipleResult(Manifest, Results);
        }
        [Obsolete("Use: PartialCompiled<ViewType>(dynamic Model)")]
        protected PrecompiledPartialViewResult Partial(Type PartialViewType, object Model = null)
        {
            return new PrecompiledPartialViewResult(Manifest, PartialViewType, Model);
        }
        protected PrecompiledPartialViewResult PartialCompiled<ViewType>(dynamic Model = null) where ViewType : WebViewPage
        {
            return new PrecompiledPartialViewResult(Manifest, typeof(ViewType), Model);
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
