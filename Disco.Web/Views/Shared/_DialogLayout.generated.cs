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

namespace Disco.Web.Views.Shared
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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Shared/_DialogLayout.cshtml")]
    public partial class _DialogLayout : Disco.Services.Web.WebViewPage<dynamic>
    {
        public _DialogLayout()
        {
        }
        public override void Execute()
        {
            
            #line 1 "..\..\Views\Shared\_DialogLayout.cshtml"
  
    Html.BundleDeferred("~/Style/Dialog");
    Html.BundleDeferred("~/Style/Site");
    Html.BundleDeferred("~/ClientScripts/Core");

            
            #line default
            #line hidden
WriteLiteral("\r\n<!doctype html>\r\n<html>\r\n<head>\r\n    <title>Disco ICT - ");

            
            #line 9 "..\..\Views\Shared\_DialogLayout.cshtml"
                  Write(CommonHelpers.BreadcrumbsTitle(ViewBag.Title));

            
            #line default
            #line hidden
WriteLiteral("</title>\r\n    <link");

WriteLiteral(" rel=\"shortcut icon\"");

WriteLiteral(" type=\"image/x-icon\"");

WriteLiteral(" href=\"/favicon.ico?v=20131224c\"");

WriteLiteral(" />\r\n    <meta");

WriteLiteral(" name=\"application-name\"");

WriteLiteral(" content=\"Disco ICT\"");

WriteLiteral(" />\r\n    <meta");

WriteLiteral(" name=\"msapplication-starturl\"");

WriteLiteral(" content=\"/\"");

WriteLiteral(" />\r\n    <meta");

WriteLiteral(" name=\"msapplication-tooltip\"");

WriteLiteral(" content=\"Open Disco ICT\"");

WriteLiteral(" />\r\n");

WriteLiteral("    ");

            
            #line 14 "..\..\Views\Shared\_DialogLayout.cshtml"
Write(Html.BundleRenderDeferred());

            
            #line default
            #line hidden
WriteLiteral("\r\n");

WriteLiteral("    ");

            
            #line 15 "..\..\Views\Shared\_DialogLayout.cshtml"
Write(RenderSection("head", false));

            
            #line default
            #line hidden
WriteLiteral("\r\n</head>\r\n<body");

WriteLiteral(" class=\"layoutDialog\"");

WriteLiteral(">\r\n");

WriteLiteral("    ");

            
            #line 18 "..\..\Views\Shared\_DialogLayout.cshtml"
Write(RenderBody());

            
            #line default
            #line hidden
WriteLiteral("\r\n");

            
            #line 19 "..\..\Views\Shared\_DialogLayout.cshtml"
    
            
            #line default
            #line hidden
            
            #line 19 "..\..\Views\Shared\_DialogLayout.cshtml"
       Disco.Services.Plugins.Features.UIExtension.UIExtensions.ExecuteExtensionResult(this); 
            
            #line default
            #line hidden
WriteLiteral("\r\n</body>\r\n</html>\r\n");

        }
    }
}
#pragma warning restore 1591
