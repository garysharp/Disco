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

namespace Disco.Web.Areas.Config.Views.DocumentTemplate
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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Areas/Config/Views/DocumentTemplate/CreatePackage.cshtml")]
    public partial class CreatePackage : Disco.Services.Web.WebViewPage<Disco.Web.Areas.Config.Models.DocumentTemplate.CreatePackageModel>
    {
        public CreatePackage()
        {
        }
        public override void Execute()
        {
            
            #line 2 "..\..\Areas\Config\Views\DocumentTemplate\CreatePackage.cshtml"
  
    Authorization.RequireAll(Claims.Config.DocumentTemplate.Create, Claims.Config.DocumentTemplate.Configure);
    
    ViewBag.Title = Html.ToBreadcrumb("Configuration", MVC.Config.Config.Index(), "Document Templates", MVC.Config.DocumentTemplate.Index(null), "Create Package");

            
            #line default
            #line hidden
WriteLiteral("\r\n");

            
            #line 7 "..\..\Areas\Config\Views\DocumentTemplate\CreatePackage.cshtml"
 using (Html.BeginForm(MVC.Config.DocumentTemplate.CreatePackage()))
{ 

            
            #line default
            #line hidden
WriteLiteral("    <div");

WriteLiteral(" class=\"form\"");

WriteLiteral(" style=\"width: 650px\"");

WriteLiteral(">\r\n        <table>\r\n            <tr>\r\n                <th>\r\n                    I" +
"d:\r\n                </th>\r\n                <td>");

            
            #line 15 "..\..\Areas\Config\Views\DocumentTemplate\CreatePackage.cshtml"
               Write(Html.TextBoxFor(model => model.Package.Id));

            
            #line default
            #line hidden
WriteLiteral("<br />");

            
            #line 15 "..\..\Areas\Config\Views\DocumentTemplate\CreatePackage.cshtml"
                                                                Write(Html.ValidationMessageFor(model => model.Package.Id));

            
            #line default
            #line hidden
WriteLiteral("\r\n                </td>\r\n            </tr>\r\n            <tr>\r\n                <th" +
">\r\n                    Description:\r\n                </th>\r\n                <td>" +
"");

            
            #line 22 "..\..\Areas\Config\Views\DocumentTemplate\CreatePackage.cshtml"
               Write(Html.TextBoxFor(model => model.Package.Description));

            
            #line default
            #line hidden
WriteLiteral("<br />");

            
            #line 22 "..\..\Areas\Config\Views\DocumentTemplate\CreatePackage.cshtml"
                                                                         Write(Html.ValidationMessageFor(model => model.Package.Description));

            
            #line default
            #line hidden
WriteLiteral("\r\n                </td>\r\n            </tr>\r\n            <tr>\r\n                <th" +
">\r\n                    Scope:\r\n                </th>\r\n                <td>\r\n");

WriteLiteral("                    ");

            
            #line 30 "..\..\Areas\Config\Views\DocumentTemplate\CreatePackage.cshtml"
               Write(Html.DropDownListFor(model => model.Package.Scope, Model.Scopes.ToSelectListItems(null)));

            
            #line default
            #line hidden
WriteLiteral("\r\n                </td>\r\n            </tr>\r\n        </table>\r\n        <p");

WriteLiteral(" class=\"actions\"");

WriteLiteral(">\r\n            <input");

WriteLiteral(" type=\"submit\"");

WriteLiteral(" class=\"button\"");

WriteLiteral(" value=\"Create\"");

WriteLiteral(" />\r\n        </p>\r\n    </div>\r\n");

            
            #line 38 "..\..\Areas\Config\Views\DocumentTemplate\CreatePackage.cshtml"
}
            
            #line default
            #line hidden
        }
    }
}
#pragma warning restore 1591