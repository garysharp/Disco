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

namespace Disco.Web.Areas.Config.Views.DeviceBatch
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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Areas/Config/Views/DeviceBatch/Index.cshtml")]
    public partial class Index : Disco.Services.Web.WebViewPage<Disco.Web.Areas.Config.Models.DeviceBatch.IndexModel>
    {
        public Index()
        {
        }
        public override void Execute()
        {
            
            #line 2 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
  
    Authorization.Require(Claims.Config.DeviceBatch.Show);
    ViewBag.Title = Html.ToBreadcrumb("Configuration", MVC.Config.Config.Index(), "Device Batches");
    var hasDecommissionedBatches = Model.DeviceBatches.Any(db => db.DeviceCount > 0 && db.DeviceDecommissionedCount >= db.DeviceCount);
    var showTags = hasDecommissionedBatches || Model.DeviceBatches.Any(i => i.IsLinked);

            
            #line default
            #line hidden
WriteLiteral("\r\n<div");

WriteLiteral(" id=\"Config_DeviceBatches\"");

WriteLiteral(">\r\n");

            
            #line 9 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
    
            
            #line default
            #line hidden
            
            #line 9 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
     if (Model.DeviceBatches.Count == 0)
    {

            
            #line default
            #line hidden
WriteLiteral("        <div");

WriteLiteral(" class=\"form\"");

WriteLiteral(" style=\"width: 450px; padding: 100px 0;\"");

WriteLiteral(">\r\n            <h2>No device batches are configured</h2>\r\n        </div>\r\n");

            
            #line 14 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
    }
    else
    {
        if (hasDecommissionedBatches)
        {

            
            #line default
            #line hidden
WriteLiteral("            <a");

WriteLiteral(" id=\"Config_DeviceBatches_ShowDecommissioned\"");

WriteLiteral(" href=\"#\"");

WriteLiteral(" class=\"button small\"");

WriteLiteral(">Show Decommissioned (");

            
            #line 19 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                                                                                                           Write(Model.DeviceBatches.Count(db => db.DeviceCount > 0 && db.DeviceDecommissionedCount >= db.DeviceCount));

            
            #line default
            #line hidden
WriteLiteral(")</a>\r\n");

WriteLiteral(@"            <script>
                $(function () {
                    $('#Config_DeviceBatches_ShowDecommissioned').click(function () {
                        $(this).remove();
                        $('#Config_DeviceBatches_List')
                            .find('tr.hidden').removeClass('hidden')
                            .filter('.decommissioned-padding').remove();
                        return false;
                    }).detach().appendTo('#layout_PageHeading');
                })
            </script>
");

            
            #line 31 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
        }

            
            #line default
            #line hidden
WriteLiteral("        <table");

WriteLiteral(" id=\"Config_DeviceBatches_List\"");

WriteLiteral(" class=\"tableData\"");

WriteLiteral(@">
            <tr>
                <th>Name</th>
                <th>Default Model</th>
                <th>Purchase Date</th>
                <th>Warranty Expires</th>
                <th>Insurance Expires</th>
                <th>Device Count</th>
");

            
            #line 40 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                
            
            #line default
            #line hidden
            
            #line 40 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                 if (showTags)
                {

            
            #line default
            #line hidden
WriteLiteral("                    <th>&nbsp;</th>\r\n");

            
            #line 43 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                }

            
            #line default
            #line hidden
WriteLiteral("            </tr>\r\n");

            
            #line 45 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
            
            
            #line default
            #line hidden
            
            #line 45 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
             foreach (var item in Model.DeviceBatches)
            {
                var isDecommissioned = item.DeviceCount > 0 && item.DeviceDecommissionedCount >= item.DeviceCount;

            
            #line default
            #line hidden
WriteLiteral("                <tr");

WriteAttribute("class", Tuple.Create(" class=\"", 2172), Tuple.Create("\"", 2217)
            
            #line 48 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
, Tuple.Create(Tuple.Create("", 2180), Tuple.Create<System.Object, System.Int32>(isDecommissioned ? "hidden" : null
            
            #line default
            #line hidden
, 2180), false)
);

WriteLiteral(">\r\n                    <td>\r\n");

WriteLiteral("                        ");

            
            #line 50 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                   Write(Html.ActionLink(item.Name, MVC.Config.DeviceBatch.Index(item.Id)));

            
            #line default
            #line hidden
WriteLiteral("\r\n                    </td>\r\n                    <td>\r\n");

WriteLiteral("                        ");

            
            #line 53 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                   Write(item.DefaultDeviceModel);

            
            #line default
            #line hidden
WriteLiteral("\r\n                    </td>\r\n                    <td>\r\n");

WriteLiteral("                        ");

            
            #line 56 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                   Write(CommonHelpers.FriendlyDate(item.PurchaseDate));

            
            #line default
            #line hidden
WriteLiteral("\r\n                    </td>\r\n                    <td>\r\n");

WriteLiteral("                        ");

            
            #line 59 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                   Write(CommonHelpers.FriendlyDate(item.WarrantyExpires, "Unknown"));

            
            #line default
            #line hidden
WriteLiteral("\r\n                    </td>\r\n                    <td>\r\n");

WriteLiteral("                        ");

            
            #line 62 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                   Write(CommonHelpers.FriendlyDate(item.InsuredUntil, item.InsuranceSupplier == null ? "N/A" : "Unknown"));

            
            #line default
            #line hidden
WriteLiteral("\r\n");

WriteLiteral("                        ");

            
            #line 63 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                    Write(item.InsuranceSupplier == null ? string.Empty : string.Format("[{0}]", item.InsuranceSupplier));

            
            #line default
            #line hidden
WriteLiteral("\r\n                    </td>\r\n                    <td>\r\n");

            
            #line 66 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                        
            
            #line default
            #line hidden
            
            #line 66 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                         if (item.DeviceCount > 0 && Authorization.Has(Claims.Device.Search))
                        {

            
            #line default
            #line hidden
WriteLiteral("                            <span>");

            
            #line 68 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                             Write(Html.ActionLink(string.Format("View {0}", item.DeviceCount), MVC.Search.Query(item.Id.ToString(), "DeviceBatch")));

            
            #line default
            #line hidden
WriteLiteral("</span>\r\n");

            
            #line 69 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                        }
                        else
                        {

            
            #line default
            #line hidden
WriteLiteral("                            <span>");

            
            #line 72 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                             Write(item.DeviceCount.ToString("n0"));

            
            #line default
            #line hidden
WriteLiteral("</span>\r\n");

            
            #line 73 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                        }

            
            #line default
            #line hidden
WriteLiteral("                        ");

            
            #line 74 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                         if (item.PurchaseUnitQuantity.HasValue)
                        {

            
            #line default
            #line hidden
WriteLiteral("                            <span>/ ");

            
            #line 76 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                                Write(item.PurchaseUnitQuantity.Value.ToString("n0"));

            
            #line default
            #line hidden
WriteLiteral("</span>\r\n");

            
            #line 77 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                        }

            
            #line default
            #line hidden
WriteLiteral("                        ");

            
            #line 78 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                         if (item.DeviceDecommissionedCount > 0)
                        {

            
            #line default
            #line hidden
WriteLiteral("                            <span");

WriteLiteral(" class=\"smallMessage\"");

WriteAttribute("title", Tuple.Create(" title=\"", 3885), Tuple.Create("\"", 3956)
            
            #line 80 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
, Tuple.Create(Tuple.Create("", 3893), Tuple.Create<System.Object, System.Int32>(item.DeviceDecommissionedCount.ToString("n0")
            
            #line default
            #line hidden
, 3893), false)
, Tuple.Create(Tuple.Create(" ", 3941), Tuple.Create("Decommissioned", 3942), true)
);

WriteLiteral(">(");

            
            #line 80 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                                                                                                                            Write(item.DeviceDecommissionedCount.ToString("n0"));

            
            #line default
            #line hidden
WriteLiteral(")</span>\r\n");

            
            #line 81 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                        }

            
            #line default
            #line hidden
WriteLiteral("                    </td>\r\n");

            
            #line 83 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                    
            
            #line default
            #line hidden
            
            #line 83 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                     if (showTags)
                    {

            
            #line default
            #line hidden
WriteLiteral("                        <td>\r\n");

            
            #line 86 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                            
            
            #line default
            #line hidden
            
            #line 86 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                             if (item.IsLinked)
                            {

            
            #line default
            #line hidden
WriteLiteral("                                <i");

WriteLiteral(" class=\"fa fa-link fa-lg success\"");

WriteLiteral(" title=\"Is Linked\"");

WriteLiteral("></i>\r\n");

            
            #line 89 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                            }

            
            #line default
            #line hidden
WriteLiteral("                            ");

            
            #line 90 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                             if (isDecommissioned)
                            {

            
            #line default
            #line hidden
WriteLiteral("                                <i");

WriteLiteral(" class=\"fa fa-minus-square fa-lg alert\"");

WriteLiteral(" title=\"Decommissioned\"");

WriteLiteral("></i>\r\n");

            
            #line 93 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                            }

            
            #line default
            #line hidden
WriteLiteral("                        </td>\r\n");

            
            #line 95 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                    }

            
            #line default
            #line hidden
WriteLiteral("                </tr>\r\n");

            
            #line 97 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                if (isDecommissioned)
                {

            
            #line default
            #line hidden
WriteLiteral("                    <tr");

WriteLiteral(" class=\"hidden decommissioned-padding\"");

WriteLiteral("></tr>\r\n");

            
            #line 100 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                }
            }

            
            #line default
            #line hidden
WriteLiteral("        </table>\r\n");

            
            #line 103 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
    }

            
            #line default
            #line hidden
WriteLiteral("</div>\r\n");

            
            #line 105 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
 if (Authorization.HasAny(Claims.Config.DeviceBatch.Create, Claims.Config.DeviceBatch.ShowTimeline))
{

            
            #line default
            #line hidden
WriteLiteral("    <div");

WriteLiteral(" class=\"actionBar\"");

WriteLiteral(">\r\n");

            
            #line 108 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
        
            
            #line default
            #line hidden
            
            #line 108 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
         if (Authorization.Has(Claims.Config.DeviceBatch.ShowTimeline) && Model.DeviceBatches.Count > 0)
        {
            
            
            #line default
            #line hidden
            
            #line 110 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
       Write(Html.ActionLinkButton("Timeline", MVC.Config.DeviceBatch.Timeline()));

            
            #line default
            #line hidden
            
            #line 110 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                                                                                 
        }

            
            #line default
            #line hidden
WriteLiteral("        ");

            
            #line 112 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
         if (Authorization.HasAll(Claims.Config.DeviceBatch.Create, Claims.Config.DeviceBatch.Configure))
        {
            
            
            #line default
            #line hidden
            
            #line 114 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
       Write(Html.ActionLinkButton("Create Device Batch", MVC.Config.DeviceBatch.Create()));

            
            #line default
            #line hidden
            
            #line 114 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
                                                                                          
        }

            
            #line default
            #line hidden
WriteLiteral("    </div>\r\n");

            
            #line 117 "..\..\Areas\Config\Views\DeviceBatch\Index.cshtml"
}
            
            #line default
            #line hidden
        }
    }
}
#pragma warning restore 1591
