using Disco.Services.Web.Bundles;
using System.IO;
using System.Web;

namespace Disco.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles()
        {
            // Styles - Site Core
            BundleTable.Add(new Bundle("~/Style/Site", Links.ClientSource.Style.BundleSite_min_css));

            // Styles - Targeted
            BundleTable.Add(new Bundle("~/Style/Config", Links.ClientSource.Style.Config_min_css));
            BundleTable.Add(new Bundle("~/Style/Device", Links.ClientSource.Style.Device_min_css));
            BundleTable.Add(new Bundle("~/Style/Dialog", Links.ClientSource.Style.Dialog_min_css));
            BundleTable.Add(new Bundle("~/Style/Job", Links.ClientSource.Style.Job_min_css));
            BundleTable.Add(new Bundle("~/Style/User", Links.ClientSource.Style.User_min_css));
            BundleTable.Add(new Bundle("~/Style/Credits", Links.ClientSource.Style.Credits_min_css));
            BundleTable.Add(new Bundle("~/Style/AppMaintenance", Links.ClientSource.Style.AppMaintenance_min_css));
            BundleTable.Add(new Bundle("~/Style/jQueryUI/dynatree", Links.ClientSource.Style.jQueryUI.dynatree.ui_dynatree_min_css));
            BundleTable.Add(new Bundle("~/Style/Fancytree", Links.ClientSource.Style.Fancytree.disco_fancytree_min_css));
            BundleTable.Add(new Bundle("~/Style/Shadowbox", Links.ClientSource.Style.Shadowbox_min_css));
            BundleTable.Add(new Bundle("~/Style/Timeline", Links.ClientSource.Style.Timeline_min_css));

            // Styles - Public Targeted
            BundleTable.Add(new Bundle("~/Style/Public/HeldDevices", Links.ClientSource.Style.Public.HeldDevices_min_css));
            BundleTable.Add(new Bundle("~/Style/Public/HeldDevicesNoticeboard", Links.ClientSource.Style.Public.HeldDevicesNoticeboard_min_css));


            // Scripts - Core
#if DEBUG
            BundleTable.Add(new Bundle("~/ClientScripts/Core", Links.ClientSource.Scripts.Core_js));
#else
            BundleTable.Add(new Bundle("~/ClientScripts/Core", Links.ClientSource.Scripts.Core_min_js));
#endif

            // Scripts - Modules
#if DEBUG
            foreach (FileInfo f in new DirectoryInfo(HttpContext.Current.Server.MapPath("~/ClientSource/Scripts/Modules")).EnumerateFiles("*.js", SearchOption.TopDirectoryOnly))
                BundleTable.Add(new Bundle(string.Format("~/ClientScripts/Modules/{0}", f.Name.Substring(0, f.Name.Length - 3)), f.FullName));
#else
            foreach (FileInfo f in new DirectoryInfo(HttpContext.Current.Server.MapPath("~/ClientSource/Scripts/Modules")).EnumerateFiles("*.min.js", SearchOption.TopDirectoryOnly))
                            BundleTable.Add(new Bundle(string.Format("~/ClientScripts/Modules/{0}", f.Name.Substring(0, f.Name.Length - 7)), f.FullName));
#endif

        }
    }
}