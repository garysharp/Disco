﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Disco.Web.Areas.Config.Views.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Mvc;
    using System.Web.Mvc.Ajax;
    using System.Web.Mvc.Html;
    using System.Web.Routing;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.WebPages;
    using Disco;
    using Disco.Models.Repository;
    using Disco.Services;
    using Disco.Services.Authorization;
    using Disco.Services.Web;
    using Disco.Web;
    using Disco.Web.Extensions;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Areas/Config/Views/Plugins/Configure.cshtml")]
    public partial class Configure : Disco.Services.Web.WebViewPage<Disco.Web.Areas.Config.Models.Plugins.PluginConfigurationViewModel>
    {
        public Configure()
        {
        }
        public override void Execute()
        {
            
            #line 2 "..\..\Areas\Config\Views\Plugins\Configure.cshtml"
  
    Authorization.Require(Claims.Config.Plugin.Configure);
    
    ViewBag.Title = Html.ToBreadcrumb("Configuration", MVC.Config.Config.Index(), "Plugins", MVC.Config.Plugins.Index(), Model.Manifest.Name);

            
            #line default
            #line hidden
WriteLiteral("\r\n");

            
            #line 7 "..\..\Areas\Config\Views\Plugins\Configure.cshtml"
 using (Html.BeginForm())
{ 
    
            
            #line default
            #line hidden
            
            #line 9 "..\..\Areas\Config\Views\Plugins\Configure.cshtml"
Write(Html.ValidationSummary(false));

            
            #line default
            #line hidden
            
            #line 9 "..\..\Areas\Config\Views\Plugins\Configure.cshtml"
                                  

            
            #line default
            #line hidden
WriteLiteral("    <div");

WriteLiteral(" class=\"clearfix\"");

WriteLiteral(">\r\n");

WriteLiteral("        ");

            
            #line 11 "..\..\Areas\Config\Views\Plugins\Configure.cshtml"
   Write(Html.PartialCompiled(Model.PluginViewType, Model.PluginViewModel));

            
            #line default
            #line hidden
WriteLiteral("\r\n    </div>\r\n");

WriteLiteral("    <div");

WriteLiteral(" class=\"actionBar\"");

WriteLiteral(">\r\n        <input");

WriteLiteral(" type=\"submit\"");

WriteLiteral(" class=\"button\"");

WriteLiteral(" value=\"Save Configuration\"");

WriteLiteral(" />\r\n    </div>\r\n");

            
            #line 16 "..\..\Areas\Config\Views\Plugins\Configure.cshtml"
}
            
            #line default
            #line hidden
        }
    }
}
#pragma warning restore 1591
