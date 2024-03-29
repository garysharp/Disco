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

namespace Disco.Web.Areas.Public.Views.UserHeldDevices
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
    using Disco.Models.Repository;
    using Disco.Services;
    using Disco.Services.Authorization;
    using Disco.Services.Web;
    using Disco.Web;
    using Disco.Web.Extensions;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Areas/Public/Views/UserHeldDevices/Index.cshtml")]
    public partial class Index : Disco.Services.Web.WebViewPage<IEnumerable<Disco.Models.Services.Jobs.Noticeboards.IHeldDeviceItem>>
    {
        public Index()
        {
        }
        public override void Execute()
        {
            
            #line 2 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
  
    ViewBag.Title = Html.ToBreadcrumb("Public Reports", MVC.Public.Public.Index(), "Held Devices for Users", null);
    Html.BundleDeferred("~/Style/Public/HeldDevices");

            
            #line default
            #line hidden
WriteLiteral("\r\n<div");

WriteLiteral(" class=\"clearfix page\"");

WriteLiteral(">\r\n    <div");

WriteLiteral(" class=\"column1\"");

WriteLiteral(">\r\n");

            
            #line 8 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
        
            
            #line default
            #line hidden
            
            #line 8 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
          
            var DevicesInProcess = Model.Where(i => !i.ReadyForReturn && !i.WaitingForUserAction).ToArray();
        
            
            #line default
            #line hidden
WriteLiteral("\r\n        <h2>In Process (");

            
            #line 11 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
                   Write(DevicesInProcess.Length);

            
            #line default
            #line hidden
WriteLiteral(")</h2>\r\n        <table");

WriteLiteral(" class=\"dataTable\"");

WriteLiteral(">\r\n");

            
            #line 13 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
            
            
            #line default
            #line hidden
            
            #line 13 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
             foreach (var item in DevicesInProcess.OrderBy(i => i.UserIdFriendly))
            {

            
            #line default
            #line hidden
WriteLiteral("                <tr>\r\n                    <td");

WriteLiteral(" class=\"id\"");

WriteLiteral(">\r\n");

WriteLiteral("                        ");

            
            #line 17 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
                   Write(item.UserIdFriendly);

            
            #line default
            #line hidden
WriteLiteral("\r\n                    </td>\r\n                    <td");

WriteLiteral(" class=\"description\"");

WriteLiteral(">\r\n");

WriteLiteral("                        ");

            
            #line 20 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
                   Write(item.UserDisplayName);

            
            #line default
            #line hidden
WriteLiteral("\r\n");

            
            #line 21 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
                        
            
            #line default
            #line hidden
            
            #line 21 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
                         if (item.EstimatedReturnTime.HasValue)
                        { 

            
            #line default
            #line hidden
WriteLiteral("                            <span");

WriteLiteral(" class=\"smallMessage\"");

WriteLiteral(">(Expected: ");

            
            #line 23 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
                                                             Write(CommonHelpers.FriendlyDate(item.EstimatedReturnTime));

            
            #line default
            #line hidden
WriteLiteral(")</span>\r\n");

            
            #line 24 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
                        }

            
            #line default
            #line hidden
WriteLiteral("                    </td>\r\n                </tr>\r\n");

            
            #line 27 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
            }

            
            #line default
            #line hidden
WriteLiteral("        </table>\r\n    </div>\r\n    <div");

WriteLiteral(" class=\"column2\"");

WriteLiteral(">\r\n");

            
            #line 31 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
        
            
            #line default
            #line hidden
            
            #line 31 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
          
            var WaitingForUserActionJobs = Model.Where(i => i.WaitingForUserAction).ToArray();
        
            
            #line default
            #line hidden
WriteLiteral("\r\n        <h2>Waiting for User Action (");

            
            #line 34 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
                                Write(WaitingForUserActionJobs.Length);

            
            #line default
            #line hidden
WriteLiteral(")</h2>\r\n        <table");

WriteLiteral(" class=\"dataTable\"");

WriteLiteral(">\r\n");

            
            #line 36 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
            
            
            #line default
            #line hidden
            
            #line 36 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
             foreach (var item in WaitingForUserActionJobs.OrderBy(i => i.UserIdFriendly))
            {

            
            #line default
            #line hidden
WriteLiteral("                <tr>\r\n                    <td");

WriteLiteral(" class=\"id\"");

WriteLiteral(">\r\n");

WriteLiteral("                        ");

            
            #line 40 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
                   Write(item.UserIdFriendly);

            
            #line default
            #line hidden
WriteLiteral("\r\n                    </td>\r\n                    <td");

WriteLiteral(" class=\"description\"");

WriteLiteral(">\r\n");

WriteLiteral("                        ");

            
            #line 43 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
                   Write(item.UserDisplayName);

            
            #line default
            #line hidden
WriteLiteral("\r\n                    </td>\r\n                    <td");

WriteAttribute("class", Tuple.Create(" class=\"", 1845), Tuple.Create("\"", 1903)
, Tuple.Create(Tuple.Create("", 1853), Tuple.Create("timestamp", 1853), true)
            
            #line 45 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
, Tuple.Create(Tuple.Create("", 1862), Tuple.Create<System.Object, System.Int32>(item.IsAlert ? " Alert" : string.Empty
            
            #line default
            #line hidden
, 1862), false)
);

WriteLiteral(">Since ");

            
            #line 45 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
                                                                                    Write(CommonHelpers.FriendlyDate(item.WaitingForUserActionSince));

            
            #line default
            #line hidden
WriteLiteral("\r\n                    </td>\r\n                </tr>\r\n");

            
            #line 48 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
            }

            
            #line default
            #line hidden
WriteLiteral("        </table>\r\n        <hr />\r\n");

            
            #line 51 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
        
            
            #line default
            #line hidden
            
            #line 51 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
          
            var DevicesReadyForReturn = Model.Where(i => i.ReadyForReturn && !i.WaitingForUserAction).ToArray();
        
            
            #line default
            #line hidden
WriteLiteral("\r\n        <h2>Ready for Return (");

            
            #line 54 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
                         Write(DevicesReadyForReturn.Length);

            
            #line default
            #line hidden
WriteLiteral(")</h2>\r\n        <table");

WriteLiteral(" class=\"dataTable\"");

WriteLiteral(">\r\n");

            
            #line 56 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
            
            
            #line default
            #line hidden
            
            #line 56 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
             foreach (var item in DevicesReadyForReturn.OrderBy(i => i.UserIdFriendly))
            {

            
            #line default
            #line hidden
WriteLiteral("                <tr>\r\n                    <td");

WriteLiteral(" class=\"id\"");

WriteLiteral(">\r\n");

WriteLiteral("                        ");

            
            #line 60 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
                   Write(item.UserIdFriendly);

            
            #line default
            #line hidden
WriteLiteral("\r\n                    </td>\r\n                    <td");

WriteLiteral(" class=\"description\"");

WriteLiteral(">\r\n");

WriteLiteral("                        ");

            
            #line 63 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
                   Write(item.UserDisplayName);

            
            #line default
            #line hidden
WriteLiteral("\r\n                    </td>\r\n                    <td");

WriteAttribute("class", Tuple.Create(" class=\"", 2689), Tuple.Create("\"", 2747)
, Tuple.Create(Tuple.Create("", 2697), Tuple.Create("timestamp", 2697), true)
            
            #line 65 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
, Tuple.Create(Tuple.Create("", 2706), Tuple.Create<System.Object, System.Int32>(item.IsAlert ? " Alert" : string.Empty
            
            #line default
            #line hidden
, 2706), false)
);

WriteLiteral(">Ready ");

            
            #line 65 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
                                                                                    Write(CommonHelpers.FriendlyDate(item.ReadyForReturnSince));

            
            #line default
            #line hidden
WriteLiteral("\r\n                    </td>\r\n                </tr>\r\n");

            
            #line 68 "..\..\Areas\Public\Views\UserHeldDevices\Index.cshtml"
            }

            
            #line default
            #line hidden
WriteLiteral("        </table>\r\n    </div>\r\n</div>\r\n");

        }
    }
}
#pragma warning restore 1591
