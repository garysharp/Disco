using Disco.Services.Web.Bundles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Disco.Services.Web
{
    public static class BundleExtensions
    {
        public static void BundleDeferred(this HtmlHelper htmlHelper, string BundleUrl)
        {
            // Ensure 'App-Relative' Url:
            BundleUrl = BundleUrl.StartsWith("~/") ? BundleUrl : (BundleUrl.StartsWith("/") ? string.Concat("~", BundleUrl) : string.Concat("~/", BundleUrl));

            var deferredBundles = htmlHelper.ViewContext.HttpContext.Items[BundleTable.DeferredKey] as List<string>;
            if (deferredBundles == null)
            {
                deferredBundles = new List<string>();
                htmlHelper.ViewContext.HttpContext.Items[BundleTable.DeferredKey] = deferredBundles;
            }
            if (!deferredBundles.Contains(BundleUrl))
                deferredBundles.Add(BundleUrl);
        }
        public static HtmlString BundleRenderDeferred(this HtmlHelper htmlHelper)
        {
            var deferredBundles = htmlHelper.ViewContext.HttpContext.Items[BundleTable.DeferredKey] as List<string>;

            var uiExtensionScripts = htmlHelper.ViewContext.HttpContext.Items[BundleTable.UIExtensionScriptsKey] as List<string>;
            var uiExtensionCss = htmlHelper.ViewContext.HttpContext.Items[BundleTable.UIExtensionCssKey] as List<string>;

            if (deferredBundles != null || uiExtensionScripts != null || uiExtensionCss != null)
            {
                StringBuilder bundleUrls = new StringBuilder();

                if (deferredBundles != null)
                {
                    deferredBundles.Reverse();
                    foreach (string bundleUrl in deferredBundles)
                    {
                        bundleUrls.AppendLine(BundleTable.ResolveBundleHtmlElement(bundleUrl));
                    }
                }
                if (uiExtensionCss != null)
                {
                    foreach (string extensionUrl in uiExtensionCss)
                        bundleUrls.Append("<link href=\"").Append(extensionUrl).AppendLine("\" rel=\"stylesheet\" type=\"text/css\" />");
                }
                if (uiExtensionScripts != null)
                {
                    foreach (string extensionUrl in uiExtensionScripts)
                        bundleUrls.Append("<script src=\"").Append(extensionUrl).AppendLine("\" type=\"text/javascript\"></script>");
                }

                return new HtmlString(bundleUrls.ToString());
            }
            else
            {
                return new HtmlString(string.Empty);
            }
        }
    }
}
