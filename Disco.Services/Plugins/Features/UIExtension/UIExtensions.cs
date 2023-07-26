using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web;
using System.Web.Mvc;
using Disco.Models.UI;

namespace Disco.Services.Plugins.Features.UIExtension
{
    public static class UIExtensions
    {
        private const string ViewDataKey = "___DiscoUIExtensionResults";

        // Warning: No type-safety, validate types before updating
        private static Dictionary<Type, List<PluginFeatureManifest>> _registrations = new Dictionary<Type, List<PluginFeatureManifest>>();

        private static List<PluginFeatureManifest> GetUIModelRegistrations<UIModel>() where UIModel : BaseUIModel
        {
            Type uiModelType = typeof(UIModel);
            List<PluginFeatureManifest> modelRegistrations;
            if (!_registrations.TryGetValue(uiModelType, out modelRegistrations))
            {
                lock (_registrations)
                {
                    if (!_registrations.TryGetValue(uiModelType, out modelRegistrations))
                    {
                        modelRegistrations = new List<PluginFeatureManifest>();
                        _registrations.Add(uiModelType, modelRegistrations);
                    }
                }
            }
            return modelRegistrations;
        }

        public static void ExecuteExtensions<UIModel>(ControllerContext context, UIModel model) where UIModel : BaseUIModel
        {
            var uiExts = UIExtensions.GetRegisteredExtensions<UIModel>();
            Queue<UIExtensionResult> uiExtResults = new Queue<UIExtensionResult>();
            foreach (var uiExt in uiExts)
            {
                using (var uiExtInstance = uiExt.CreateInstance<UIExtensionFeature<UIModel>>())
                {
                    uiExtInstance.Context = context;
                    uiExtResults.Enqueue(uiExtInstance.ExecuteAction(context, model));
                }
            }
            context.Controller.ViewData[ViewDataKey] = uiExtResults;
        }
        public static void ExecuteExtensionResult<UIModel>(WebViewPage<UIModel> page)
        {
            Queue<UIExtensionResult> uiExtResults = page.ViewData[ViewDataKey] as Queue<UIExtensionResult>;

            if (uiExtResults != null && uiExtResults.Count > 0)
            {
                page.WriteLiteral("<!-- BEGIN: Disco ICT UI Extensions -->");
                page.WriteLiteral("\n<div id=\"layout_uiExtensions\">");
                foreach (var uiExtResult in uiExtResults)
                {
                    string extensionDescription = HttpUtility.HtmlEncode(string.Format("{0} @ {1} v{2}", uiExtResult.Source.Id, uiExtResult.Source.PluginManifest.Id, uiExtResult.Source.PluginManifest.Version.ToString(4)));
                    page.WriteLiteral(string.Format("\n<!-- BEGIN UI EXTENSION: {0} -->\n", extensionDescription));
                    uiExtResult.ExecuteResult(page);
                    page.WriteLiteral(string.Format("\n<!-- END UI EXTENSION: {0} -->", extensionDescription));
                }
                page.WriteLiteral("\n</div>");
                page.WriteLiteral("\n<!-- END: Disco ICT UI Extensions -->");
            }
        }

        public static ReadOnlyCollection<PluginFeatureManifest> GetRegisteredExtensions<UIModel>() where UIModel : BaseUIModel
        {
            List<PluginFeatureManifest> modelRegistrations = GetUIModelRegistrations<UIModel>();
            return new ReadOnlyCollection<PluginFeatureManifest>(modelRegistrations);
        }

        internal static bool ExtensionRegistered<UIModel>(UIExtensionFeature<UIModel> Extension) where UIModel : BaseUIModel
        {
            List<PluginFeatureManifest> modelRegistrations = GetUIModelRegistrations<UIModel>();
            return modelRegistrations.Contains(Extension.Manifest);
        }

        internal static bool RegisterExtension<UIModel>(UIExtensionFeature<UIModel> Extension) where UIModel : BaseUIModel
        {
            List<PluginFeatureManifest> modelRegistrations = GetUIModelRegistrations<UIModel>();

            lock (modelRegistrations)
            {
                if (!modelRegistrations.Contains(Extension.Manifest))
                {
                    modelRegistrations.Add(Extension.Manifest);
                    return true;
                }
            }
            return false;
        }
        internal static bool UnregisterExtension<UIModel>(UIExtensionFeature<UIModel> Extension) where UIModel : BaseUIModel
        {
            List<PluginFeatureManifest> modelRegistrations = GetUIModelRegistrations<UIModel>();

            lock (modelRegistrations)
            {
                if (modelRegistrations.Contains(Extension.Manifest))
                {
                    modelRegistrations.Remove(Extension.Manifest);
                    return true;
                }
            }
            return false;
        }
    }
}
