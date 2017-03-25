using System;
using System.Collections.Generic;
using System.Web;

namespace Disco.Services.Web.Bundles
{
    public static class BundleTable
    {
        public const string DeferredKey = "Bundles.Deferred";
        public const string UIExtensionScriptsKey = "Bundles.UIExtensionScripts";
        public const string UIExtensionCssKey = "Bundles.UIExtensionCss";

        private static Dictionary<string, IBundle> _bundles;

        static BundleTable()
        {
            _bundles = new Dictionary<string, IBundle>();
        }

        public static void Add(IBundle Bundle)
        {
            _bundles[Bundle.Url] = Bundle;
        }

        public static int Count
        {
            get
            {
                return _bundles.Count;
            }
        }

        internal static IBundle GetBundleFor(string Url)
        {
            if (_bundles.ContainsKey(Url))
            {
                return _bundles[Url];
            }
            return null;
        }

        public static string ResolveBundleUrl(string BundleUrl)
        {
            var bundle = GetBundleFor(BundleUrl);

            if (bundle == null)
                throw new ArgumentException(string.Format("Unknown Bundle Url: {0}", BundleUrl), "BundleUrl");

            return VirtualPathUtility.ToAbsolute(bundle.VersionUrl);
        }
        public static string ResolveBundleHtmlElement(string BundleUrl)
        {
            var bundle = GetBundleFor(BundleUrl);

            if (bundle == null)
                throw new ArgumentException(string.Format("Unknown Bundle Url: {0}", BundleUrl), "BundleUrl");

            var bundleUrl = VirtualPathUtility.ToAbsolute(bundle.VersionUrl);

            switch (bundle.ContentType)
            {
                case "text/css":
                    return string.Format("<link href=\"{0}\" rel=\"stylesheet\" type=\"text/css\" />", bundleUrl);
                case "text/javascript":
                    return string.Format("<script src=\"{0}\" type=\"text/javascript\"></script>", bundleUrl);
                default:
                    throw new ArgumentException("Unsupported Bundle Content Type", "BundleUrl");
            }
        }
    }
}
