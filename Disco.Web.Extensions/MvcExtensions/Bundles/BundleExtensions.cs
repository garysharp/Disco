using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Disco.Web.Extensions.MvcExtensions.Bundles;

namespace Disco.Web.Extensions
{
    public static class BundleExtensions
    {
        public static void BundleDeferred(this HtmlHelper htmlHelper, string BundleUrl)
        {
            // Ensure 'App-Relative' Url:
            BundleUrl = BundleUrl.StartsWith("~/") ? BundleUrl : (BundleUrl.StartsWith("/") ? string.Concat("~", BundleUrl) : string.Concat("~/", BundleUrl));

            var deferredBundles = default(List<string>);
            deferredBundles = htmlHelper.ViewContext.HttpContext.Items["Bundles.Deferred"] as List<string>;
            if (deferredBundles == null)
            {
                deferredBundles = new List<string>();
                htmlHelper.ViewContext.HttpContext.Items["Bundles.Deferred"] = deferredBundles;
            }
            if (!deferredBundles.Contains(BundleUrl))
                deferredBundles.Add(BundleUrl);
        }
        public static HtmlString BundleRenderDeferred(this HtmlHelper htmlHelper)
        {
            var deferredBundles = default(List<string>);
            deferredBundles = htmlHelper.ViewContext.HttpContext.Items["Bundles.Deferred"] as List<string>;

            if (deferredBundles != null)
            {
                StringBuilder bundleUrls = new StringBuilder();
                deferredBundles.Reverse();
                foreach (string bundleUrl in deferredBundles)
                {
                    bundleUrls.AppendLine(BundleTable.ResolveBundleHtmlElement(bundleUrl));
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
