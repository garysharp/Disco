﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18051
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Disco.Web.Areas.Config.Views.AuthorizationRole
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
    using Disco.BI.Extensions;
    using Disco.Models.Repository;
    using Disco.Services.Authorization;
    using Disco.Services.Web;
    using Disco.Web;
    using Disco.Web.Extensions;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Areas/Config/Views/AuthorizationRole/Index.cshtml")]
    public partial class Index : Disco.Services.Web.WebViewPage<Disco.Web.Areas.Config.Models.AuthorizationRole.IndexModel>
    {
        public Index()
        {
        }
        public override void Execute()
        {
            
            #line 2 "..\..\Areas\Config\Views\AuthorizationRole\Index.cshtml"
  
    Authorization.Require(Claims.DiscoAdminAccount);
    ViewBag.Title = Html.ToBreadcrumb("Configuration", MVC.Config.Config.Index(), "Authorization Roles");

            
            #line default
            #line hidden
WriteLiteral("\r\n");

            
            #line 6 "..\..\Areas\Config\Views\AuthorizationRole\Index.cshtml"
 if (Model.Tokens.Count == 0)
{

            
            #line default
            #line hidden
WriteLiteral("    <div");

WriteLiteral(" class=\"form\"");

WriteLiteral(" style=\"width: 450px; padding: 100px 0;\"");

WriteLiteral(">\r\n        <h2>No authorization roles are configured</h2>\r\n    </div>  \r\n");

            
            #line 11 "..\..\Areas\Config\Views\AuthorizationRole\Index.cshtml"
}
else
{

            
            #line default
            #line hidden
WriteLiteral("    <table");

WriteLiteral(" class=\"tableData\"");

WriteLiteral(">\r\n        <tr>\r\n            <th>Name\r\n            </th>\r\n            <th>Linked " +
"Groups/Users\r\n            </th>\r\n        </tr>\r\n");

            
            #line 21 "..\..\Areas\Config\Views\AuthorizationRole\Index.cshtml"
        
            
            #line default
            #line hidden
            
            #line 21 "..\..\Areas\Config\Views\AuthorizationRole\Index.cshtml"
         foreach (var item in Model.Tokens)
        {

            
            #line default
            #line hidden
WriteLiteral("            <tr>\r\n                <td>\r\n");

WriteLiteral("                    ");

            
            #line 25 "..\..\Areas\Config\Views\AuthorizationRole\Index.cshtml"
               Write(Html.ActionLink(item.Role.Name, MVC.Config.AuthorizationRole.Index(item.Role.Id)));

            
            #line default
            #line hidden
WriteLiteral("\r\n                </td>\r\n                <td>\r\n");

            
            #line 28 "..\..\Areas\Config\Views\AuthorizationRole\Index.cshtml"
                    
            
            #line default
            #line hidden
            
            #line 28 "..\..\Areas\Config\Views\AuthorizationRole\Index.cshtml"
                     if (item.SubjectIds.Count == 0)
                    {

            
            #line default
            #line hidden
WriteLiteral("                        <span");

WriteLiteral(" class=\"smallMessage\"");

WriteLiteral(">&lt;None&gt;</span>\r\n");

            
            #line 31 "..\..\Areas\Config\Views\AuthorizationRole\Index.cshtml"
                    }
                    else
                    {
                        
            
            #line default
            #line hidden
            
            #line 34 "..\..\Areas\Config\Views\AuthorizationRole\Index.cshtml"
                    Write(string.Join(", ", item.SubjectIds.OrderBy(i => i)));

            
            #line default
            #line hidden
            
            #line 34 "..\..\Areas\Config\Views\AuthorizationRole\Index.cshtml"
                                                                             
                    }

            
            #line default
            #line hidden
WriteLiteral("                </td>\r\n            </tr>\r\n");

            
            #line 38 "..\..\Areas\Config\Views\AuthorizationRole\Index.cshtml"
        }

            
            #line default
            #line hidden
WriteLiteral("    </table>\r\n");

            
            #line 40 "..\..\Areas\Config\Views\AuthorizationRole\Index.cshtml"
}

            
            #line default
            #line hidden
WriteLiteral("<div");

WriteLiteral(" class=\"actionBar\"");

WriteLiteral(">\r\n");

WriteLiteral("    ");

            
            #line 42 "..\..\Areas\Config\Views\AuthorizationRole\Index.cshtml"
Write(Html.ActionLinkButton("Create Authorization Role", MVC.Config.AuthorizationRole.Create()));

            
            #line default
            #line hidden
WriteLiteral("\r\n</div>\r\n");

        }
    }
}
#pragma warning restore 1591