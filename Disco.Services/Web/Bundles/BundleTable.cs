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
                throw new ArgumentException($"Unknown Bundle Url: {BundleUrl}", "BundleUrl");

            return VirtualPathUtility.ToAbsolute(bundle.VersionUrl);
        }
        public static string ResolveBundleHtmlElement(string BundleUrl)
        {
            var bundle = GetBundleFor(BundleUrl);

            if (bundle == null)
                throw new ArgumentException($"Unknown Bundle Url: {BundleUrl}", "BundleUrl");

            var bundleUrl = VirtualPathUtility.ToAbsolute(bundle.VersionUrl);

            switch (bundle.ContentType)
            {
                case "text/css":
                    return $"<link href=\"{bundleUrl}\" rel=\"stylesheet\" type=\"text/css\" />";
                case "text/javascript":
                    return $"<script src=\"{bundleUrl}\" type=\"text/javascript\"></script>";
                default:
                    throw new ArgumentException("Unsupported Bundle Content Type", "BundleUrl");
            }
        }
    }
}
