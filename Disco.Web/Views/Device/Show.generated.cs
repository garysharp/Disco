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

namespace Disco.Web.Views.Device
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
    
    #line 2 "..\..\Views\Device\Show.cshtml"
    using Disco.Services.Devices.DeviceFlags;
    
    #line default
    #line hidden
    using Disco.Services.Web;
    using Disco.Web;
    using Disco.Web.Extensions;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Device/Show.cshtml")]
    public partial class Show : Disco.Services.Web.WebViewPage<Disco.Web.Models.Device.ShowModel>
    {
        public Show()
        {
        }
        public override void Execute()
        {
            
            #line 3 "..\..\Views\Device\Show.cshtml"
  
    ViewBag.Title = Html.ToBreadcrumb("Devices", MVC.Device.Index(), string.Format("Device: {0}", Model.Device.SerialNumber));

            
            #line default
            #line hidden
WriteLiteral("\r\n<div");

WriteLiteral(" id=\"Device_Show\"");

WriteLiteral(">\r\n    <div");

WriteLiteral(" id=\"Device_Show_Status\"");

WriteLiteral(">\r\n        <i");

WriteAttribute("class", Tuple.Create(" class=\"", 290), Tuple.Create("\"", 352)
, Tuple.Create(Tuple.Create("", 298), Tuple.Create("fa", 298), true)
, Tuple.Create(Tuple.Create(" ", 300), Tuple.Create("fa-square", 301), true)
, Tuple.Create(Tuple.Create(" ", 310), Tuple.Create("deviceStatus", 311), true)
            
            #line 8 "..\..\Views\Device\Show.cshtml"
, Tuple.Create(Tuple.Create(" ", 323), Tuple.Create<System.Object, System.Int32>(Model.Device.StatusCode()
            
            #line default
            #line hidden
, 324), false)
);

WriteLiteral("></i>&nbsp;");

            
            #line 8 "..\..\Views\Device\Show.cshtml"
                                                                               Write(Model.Device.Status());

            
            #line default
            #line hidden
WriteLiteral("\r\n        <script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(">\r\n            $(function () {\r\n                $(\'#Device_Show_Status\').appendTo" +
"(\'#layout_PageHeading\')\r\n            });\r\n        </script>\r\n    </div>\r\n");

            
            #line 15 "..\..\Views\Device\Show.cshtml"
    
            
            #line default
            #line hidden
            
            #line 15 "..\..\Views\Device\Show.cshtml"
     if (Authorization.Has(Claims.Device.ShowFlagAssignments))
    {

            
            #line default
            #line hidden
WriteLiteral("        <div");

WriteLiteral(" id=\"Device_Show_Flags\"");

WriteLiteral(">\r\n");

            
            #line 18 "..\..\Views\Device\Show.cshtml"
            
            
            #line default
            #line hidden
            
            #line 18 "..\..\Views\Device\Show.cshtml"
             foreach (var flag in Model.Device.DeviceFlagAssignments.Where(f => !f.RemovedDate.HasValue).Select(f => Tuple.Create(f, DeviceFlagService.GetDeviceFlag(f.DeviceFlagId))))
            {

            
            #line default
            #line hidden
WriteLiteral("                <i");

WriteAttribute("class", Tuple.Create(" class=\"", 907), Tuple.Create("\"", 983)
, Tuple.Create(Tuple.Create("", 915), Tuple.Create("flag", 915), true)
, Tuple.Create(Tuple.Create(" ", 919), Tuple.Create("fa", 920), true)
, Tuple.Create(Tuple.Create(" ", 922), Tuple.Create("fa-", 923), true)
            
            #line 20 "..\..\Views\Device\Show.cshtml"
, Tuple.Create(Tuple.Create("", 926), Tuple.Create<System.Object, System.Int32>(flag.Item2.Icon
            
            #line default
            #line hidden
, 926), false)
, Tuple.Create(Tuple.Create(" ", 944), Tuple.Create("fa-fw", 945), true)
, Tuple.Create(Tuple.Create(" ", 950), Tuple.Create("fa-lg", 951), true)
, Tuple.Create(Tuple.Create(" ", 956), Tuple.Create("d-", 957), true)
            
            #line 20 "..\..\Views\Device\Show.cshtml"
, Tuple.Create(Tuple.Create("", 959), Tuple.Create<System.Object, System.Int32>(flag.Item2.IconColour
            
            #line default
            #line hidden
, 959), false)
);

WriteLiteral(">\r\n                    <span");

WriteLiteral(" class=\"details\"");

WriteLiteral(">\r\n                        <span");

WriteLiteral(" class=\"name\"");

WriteLiteral(">");

            
            #line 22 "..\..\Views\Device\Show.cshtml"
                                      Write(flag.Item2.Name);

            
            #line default
            #line hidden
WriteLiteral("</span>");

            
            #line 22 "..\..\Views\Device\Show.cshtml"
                                                                   if (flag.Item1.Comments != null)
                        {
            
            #line default
            #line hidden
WriteLiteral("<span");

WriteLiteral(" class=\"comments\"");

WriteLiteral(">");

            
            #line 23 "..\..\Views\Device\Show.cshtml"
                                           Write(flag.Item1.Comments.ToHtmlComment());

            
            #line default
            #line hidden
WriteLiteral("</span>");

            
            #line 23 "..\..\Views\Device\Show.cshtml"
                                                                                           }
            
            #line default
            #line hidden
WriteLiteral("<span");

WriteLiteral(" class=\"added\"");

WriteLiteral(">");

            
            #line 23 "..\..\Views\Device\Show.cshtml"
                                                                                                           Write(CommonHelpers.FriendlyDateAndUser(flag.Item1.AddedDate, flag.Item1.AddedUser));

            
            #line default
            #line hidden
WriteLiteral("</span>\r\n                    </span>\r\n                </i>\r\n");

            
            #line 26 "..\..\Views\Device\Show.cshtml"
            }

            
            #line default
            #line hidden
WriteLiteral("        </div>\r\n");

WriteLiteral("        <script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(@">
            $(function () {
                $('#Device_Show_Flags')
                    .appendTo('#layout_PageHeading')
                    .tooltip({
                        items: 'i.flag',
                        content: function () {
                            var $this = $(this);
                            return $this.children('.details').html();
                        },
                        tooltipClass: 'FlagAssignment_Tooltip',
                        position: {
                            my: ""right top"",
                            at: ""right bottom"",
                            collision: ""flipfit flip""
                        },
                        hade: {
                            effect: ''
                        },
                        close: function (e, ui) {
                            ui.tooltip.hover(
                                function () {
                                    $(this).stop(true).fadeTo(100, 1);
                                },
                                function () {
                                    $(this).fadeOut(100, function () { $(this).remove(); });
                                });
                        }
                    });
            });
        </script>
");

            
            #line 59 "..\..\Views\Device\Show.cshtml"
    }

            
            #line default
            #line hidden
WriteLiteral("    ");

            
            #line 60 "..\..\Views\Device\Show.cshtml"
Write(Html.Partial(MVC.Device.Views.DeviceParts._Subject, Model));

            
            #line default
            #line hidden
WriteLiteral("\r\n    <script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(">\r\n        $(function () {\r\n            var $tabs = $(\'#DeviceDetailTabs\');\r\n    " +
"        if ($tabs.children().length > 1) {\r\n                $tabs.tabs({\r\n      " +
"              activate: function (event, ui) {\r\n                        window.s" +
"etTimeout(function () {\r\n                            var $window = $(window);\r\n " +
"                           var tabHeight = $tabs.height();\r\n                    " +
"        var tabOffset = $tabs.offset();\r\n                            var windowS" +
"crollTop = $window.scrollTop();\r\n                            var windowHeight = " +
"$window.height();\r\n\r\n                            var tabTopNotShown = windowScro" +
"llTop - tabOffset.top;\r\n                            if (tabTopNotShown > 0) {\r\n " +
"                               $(\'html\').animate({ scrollTop: tabOffset.top }, 1" +
"25);\r\n                            } else {\r\n                                var " +
"tabBottomNotShown = ((windowScrollTop + windowHeight) - (tabHeight + tabOffset.t" +
"op)) * -1;\r\n                                if (tabBottomNotShown > 0) {\r\n      " +
"                              if (tabHeight > windowHeight)\r\n                   " +
"                     $(\'html\').animate({ scrollTop: tabOffset.top }, 125);\r\n    " +
"                                else\r\n                                        $(" +
"\'html\').animate({ scrollTop: windowScrollTop + tabBottomNotShown }, 125);\r\n     " +
"                           }\r\n                            }\r\n                   " +
"     }, 1);\r\n                    }\r\n                });\r\n            } else {\r\n " +
"               $tabs.hide();\r\n            }\r\n        });\r\n    </script>\r\n    <di" +
"v");

WriteLiteral(" id=\"DeviceDetailTabs\"");

WriteLiteral(">\r\n        <ul");

WriteLiteral(" id=\"DeviceDetailTabItems\"");

WriteLiteral("></ul>\r\n");

            
            #line 96 "..\..\Views\Device\Show.cshtml"
        
            
            #line default
            #line hidden
            
            #line 96 "..\..\Views\Device\Show.cshtml"
         if (Authorization.Has(Claims.Device.ShowJobs))
        {
            
            
            #line default
            #line hidden
            
            #line 98 "..\..\Views\Device\Show.cshtml"
       Write(Html.Partial(MVC.Device.Views.DeviceParts._Jobs, Model));

            
            #line default
            #line hidden
            
            #line 98 "..\..\Views\Device\Show.cshtml"
                                                                    
        }

            
            #line default
            #line hidden
WriteLiteral("        ");

            
            #line 100 "..\..\Views\Device\Show.cshtml"
         if (Authorization.Has(Claims.Device.ShowDetails))
        {
            
            
            #line default
            #line hidden
            
            #line 102 "..\..\Views\Device\Show.cshtml"
       Write(Html.Partial(MVC.Device.Views.DeviceParts._Details, Model));

            
            #line default
            #line hidden
            
            #line 102 "..\..\Views\Device\Show.cshtml"
                                                                       
        }

            
            #line default
            #line hidden
WriteLiteral("        ");

            
            #line 104 "..\..\Views\Device\Show.cshtml"
         if (Authorization.Has(Claims.Device.ShowAssignmentHistory))
        {
            
            
            #line default
            #line hidden
            
            #line 106 "..\..\Views\Device\Show.cshtml"
       Write(Html.Partial(MVC.Device.Views.DeviceParts._AssignmentHistory, Model));

            
            #line default
            #line hidden
            
            #line 106 "..\..\Views\Device\Show.cshtml"
                                                                                 
        }

            
            #line default
            #line hidden
WriteLiteral("        ");

            
            #line 108 "..\..\Views\Device\Show.cshtml"
         if (Authorization.Has(Claims.Device.ShowAttachments))
        {
            
            
            #line default
            #line hidden
            
            #line 110 "..\..\Views\Device\Show.cshtml"
       Write(Html.Partial(MVC.Device.Views.DeviceParts._Resources, Model));

            
            #line default
            #line hidden
            
            #line 110 "..\..\Views\Device\Show.cshtml"
                                                                         
        }

            
            #line default
            #line hidden
WriteLiteral("        ");

            
            #line 112 "..\..\Views\Device\Show.cshtml"
         if (Authorization.Has(Claims.Device.ShowFlagAssignments))
        {
            
            
            #line default
            #line hidden
            
            #line 114 "..\..\Views\Device\Show.cshtml"
       Write(Html.Partial(MVC.Device.Views.DeviceParts._Flags, Model));

            
            #line default
            #line hidden
            
            #line 114 "..\..\Views\Device\Show.cshtml"
                                                                     
        }

            
            #line default
            #line hidden
WriteLiteral("        ");

            
            #line 116 "..\..\Views\Device\Show.cshtml"
         if (Authorization.Has(Claims.Device.ShowCertificates))
        {
            
            
            #line default
            #line hidden
            
            #line 118 "..\..\Views\Device\Show.cshtml"
       Write(Html.Partial(MVC.Device.Views.DeviceParts._Certificates, Model));

            
            #line default
            #line hidden
            
            #line 118 "..\..\Views\Device\Show.cshtml"
                                                                            
        }

            
            #line default
            #line hidden
WriteLiteral("    </div>\r\n</div>\r\n");

        }
    }
}
#pragma warning restore 1591
