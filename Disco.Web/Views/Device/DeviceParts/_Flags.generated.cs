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

namespace Disco.Web.Views.Device.DeviceParts
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
    
    #line 2 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
    using Disco.Services.Devices.DeviceFlags;
    
    #line default
    #line hidden
    using Disco.Services.Web;
    using Disco.Web;
    using Disco.Web.Extensions;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Device/DeviceParts/_Flags.cshtml")]
    public partial class _Flags : Disco.Services.Web.WebViewPage<Disco.Web.Models.Device.ShowModel>
    {
        public _Flags()
        {
        }
        public override void Execute()
        {
            
            #line 3 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
  
    Authorization.Require(Claims.Device.ShowFlagAssignments);

    var hasRemove = Authorization.Has(Claims.Device.Actions.RemoveFlags);
    var hasEdit = Authorization.Has(Claims.Device.Actions.EditFlags);

    var hasDeviceFlagShow = Authorization.Has(Claims.Config.DeviceFlag.Show);
    var activeAssignmentCount = Model.Device.DeviceFlagAssignments == null ? 0 : Model.Device.DeviceFlagAssignments.Count(a => !a.RemovedDate.HasValue);

    var flagAssignments = Model.Device.DeviceFlagAssignments.Select(a => Tuple.Create(a, DeviceFlagService.GetDeviceFlag(a.DeviceFlagId))).ToList();

            
            #line default
            #line hidden
WriteLiteral("\r\n<div");

WriteLiteral(" id=\"DeviceDetailTab-Flags\"");

WriteLiteral(" class=\"DevicePart\"");

WriteLiteral(">\r\n");

            
            #line 15 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
    
            
            #line default
            #line hidden
            
            #line 15 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
     if (flagAssignments.Count > 0)
    {

            
            #line default
            #line hidden
WriteLiteral("        <table");

WriteLiteral(" id=\"deviceFlags\"");

WriteLiteral(">\r\n            <tr>\r\n                <th");

WriteLiteral(" class=\"name\"");

WriteLiteral(">Name</th>\r\n                <th");

WriteLiteral(" class=\"added\"");

WriteLiteral(">Added</th>\r\n                <th");

WriteLiteral(" class=\"sla\"");

WriteLiteral(">Comments</th>\r\n                <th");

WriteLiteral(" class=\"removed\"");

WriteLiteral(">Removed</th>\r\n            </tr>\r\n");

            
            #line 24 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
            
            
            #line default
            #line hidden
            
            #line 24 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
             foreach (var fa in flagAssignments.OrderByDescending(a => a.Item1.AddedDate))
            {

            
            #line default
            #line hidden
WriteLiteral("                <tr");

WriteLiteral(" data-deviceflagassignmentid=\"");

            
            #line 26 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                                            Write(fa.Item1.Id);

            
            #line default
            #line hidden
WriteLiteral("\"");

WriteLiteral(" data-flagassignmentaddeddate=\"");

            
            #line 26 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                                                                                         Write(fa.Item1.AddedDate.ToString("s"));

            
            #line default
            #line hidden
WriteLiteral("\"");

WriteAttribute("class", Tuple.Create(" class=\"", 1282), Tuple.Create("\"", 1345)
            
            #line 26 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                                                          , Tuple.Create(Tuple.Create("", 1290), Tuple.Create<System.Object, System.Int32>(!fa.Item1.RemovedDate.HasValue ? "added" : "removed"
            
            #line default
            #line hidden
, 1290), false)
);

WriteLiteral(">\r\n                    <td");

WriteLiteral(" class=\"name\"");

WriteLiteral(">\r\n                        <i");

WriteAttribute("class", Tuple.Create(" class=\"", 1414), Tuple.Create("\"", 1481)
, Tuple.Create(Tuple.Create("", 1422), Tuple.Create("fa", 1422), true)
, Tuple.Create(Tuple.Create(" ", 1424), Tuple.Create("fa-", 1425), true)
            
            #line 28 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
, Tuple.Create(Tuple.Create("", 1428), Tuple.Create<System.Object, System.Int32>(fa.Item2.Icon
            
            #line default
            #line hidden
, 1428), false)
, Tuple.Create(Tuple.Create(" ", 1444), Tuple.Create("fa-fw", 1445), true)
, Tuple.Create(Tuple.Create(" ", 1450), Tuple.Create("fa-lg", 1451), true)
, Tuple.Create(Tuple.Create(" ", 1456), Tuple.Create("d-", 1457), true)
            
            #line 28 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
, Tuple.Create(Tuple.Create("", 1459), Tuple.Create<System.Object, System.Int32>(fa.Item2.IconColour
            
            #line default
            #line hidden
, 1459), false)
);

WriteLiteral("></i>\r\n");

            
            #line 29 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                        
            
            #line default
            #line hidden
            
            #line 29 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                         if (hasDeviceFlagShow)
                        {
                            
            
            #line default
            #line hidden
            
            #line 31 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                       Write(Html.ActionLink(fa.Item2.Name, MVC.Config.DeviceFlag.Index(fa.Item2.Id)));

            
            #line default
            #line hidden
            
            #line 31 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                                                                                                     
                        }
                        else
                        {
                            
            
            #line default
            #line hidden
            
            #line 35 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                       Write(fa.Item2.Name);

            
            #line default
            #line hidden
            
            #line 35 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                                          
                        }

            
            #line default
            #line hidden
WriteLiteral("                    </td>\r\n                    <td");

WriteLiteral(" class=\"added\"");

WriteLiteral(">\r\n");

WriteLiteral("                        ");

            
            #line 39 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                   Write(CommonHelpers.FriendlyDateAndUser(fa.Item1.AddedDate, fa.Item1.AddedUser));

            
            #line default
            #line hidden
WriteLiteral("\r\n");

            
            #line 40 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                        
            
            #line default
            #line hidden
            
            #line 40 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                         if (fa.Item1.OnAssignmentExpressionResult != null)
                        {

            
            #line default
            #line hidden
WriteLiteral("                            <div");

WriteLiteral(" class=\"expressionResult\"");

WriteLiteral(">");

            
            #line 42 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                                                     Write(fa.Item1.OnAssignmentExpressionResult);

            
            #line default
            #line hidden
WriteLiteral("</div>\r\n");

            
            #line 43 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                        }

            
            #line default
            #line hidden
WriteLiteral("                    </td>\r\n                    <td");

WriteLiteral(" class=\"comments\"");

WriteLiteral(">\r\n");

            
            #line 46 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                        
            
            #line default
            #line hidden
            
            #line 46 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                         if (hasEdit)
                        {

            
            #line default
            #line hidden
WriteLiteral("                            <div");

WriteLiteral(" class=\"editable\"");

WriteLiteral("><i");

WriteLiteral(" class=\"fa fa-fw fa-edit\"");

WriteLiteral(" title=\"Edit Comments\"");

WriteLiteral("></i></div>\r\n");

            
            #line 49 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                        }

            
            #line default
            #line hidden
WriteLiteral("                        ");

            
            #line 50 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                         if (fa.Item1.Comments == null)
                        {

            
            #line default
            #line hidden
WriteLiteral("                            <div");

WriteLiteral(" class=\"comments smallMessage\"");

WriteLiteral(">[no comments]</div>\r\n");

            
            #line 53 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                        }
                        else
                        {

            
            #line default
            #line hidden
WriteLiteral("                            <div");

WriteLiteral(" class=\"comments\"");

WriteLiteral(">");

            
            #line 56 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                                             Write(fa.Item1.Comments.ToHtmlComment());

            
            #line default
            #line hidden
WriteLiteral("</div>\r\n");

WriteLiteral("                            <div");

WriteLiteral(" class=\"commentsRaw\"");

WriteLiteral(">");

            
            #line 57 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                                                Write(fa.Item1.Comments);

            
            #line default
            #line hidden
WriteLiteral("</div>\r\n");

            
            #line 58 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                        }

            
            #line default
            #line hidden
WriteLiteral("                    </td>\r\n                    <td");

WriteAttribute("class", Tuple.Create(" class=\"", 3000), Tuple.Create("\"", 3063)
, Tuple.Create(Tuple.Create("", 3008), Tuple.Create("removed", 3008), true)
            
            #line 60 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
, Tuple.Create(Tuple.Create("", 3015), Tuple.Create<System.Object, System.Int32>(!fa.Item1.RemovedDate.HasValue ? " na" : null
            
            #line default
            #line hidden
, 3015), false)
);

WriteLiteral(">\r\n");

            
            #line 61 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                        
            
            #line default
            #line hidden
            
            #line 61 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                         if (fa.Item1.RemovedDate.HasValue)
                        {
                            
            
            #line default
            #line hidden
            
            #line 63 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                       Write(CommonHelpers.FriendlyDateAndUser(fa.Item1.RemovedDate.Value, fa.Item1.RemovedUser));

            
            #line default
            #line hidden
            
            #line 63 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                                                                                                                
                            if (fa.Item1.OnUnassignmentExpressionResult != null)
                            {

            
            #line default
            #line hidden
WriteLiteral("                                <div");

WriteLiteral(" class=\"expressionResult\"");

WriteLiteral(">");

            
            #line 66 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                                                         Write(fa.Item1.OnUnassignmentExpressionResult);

            
            #line default
            #line hidden
WriteLiteral("</div>\r\n");

            
            #line 67 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                            }
                        }
                        else if (fa.Item1.CanRemove())
                        {

            
            #line default
            #line hidden
WriteLiteral("                            <a");

WriteLiteral(" href=\"#\"");

WriteLiteral(" class=\"button small remove\"");

WriteLiteral(">Remove</a>\r\n");

            
            #line 72 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                        }

            
            #line default
            #line hidden
WriteLiteral("                    </td>\r\n                </tr>\r\n");

            
            #line 75 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
            }

            
            #line default
            #line hidden
WriteLiteral("        </table>\r\n");

WriteLiteral("        <div");

WriteLiteral(" id=\"Device_Show_Flags_Actions_Remove_Dialog\"");

WriteLiteral(" class=\"dialog\"");

WriteLiteral(" title=\"Remove this flag from the device?\"");

WriteLiteral(">\r\n");

            
            #line 78 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
            
            
            #line default
            #line hidden
            
            #line 78 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
             using (Html.BeginForm(MVC.API.DeviceFlagAssignment.RemoveDevice()))
            {

            
            #line default
            #line hidden
WriteLiteral("                <input");

WriteLiteral(" id=\"Device_Show_Flags_Actions_Remove_Dialog_Id\"");

WriteLiteral(" type=\"hidden\"");

WriteLiteral(" name=\"id\"");

WriteLiteral(" value=\"\"");

WriteLiteral(" />\r\n");

WriteLiteral("                <p>\r\n                    <i");

WriteLiteral(" class=\"fa fa-exclamation-triangle fa-lg\"");

WriteLiteral("></i>&nbsp;Are you sure?\r\n                </p>\r\n");

            
            #line 84 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
            }

            
            #line default
            #line hidden
WriteLiteral("        </div>\r\n");

WriteLiteral("        <div");

WriteLiteral(" id=\"Device_Show_Flags_Actions_EditComments_Dialog\"");

WriteLiteral(" class=\"dialog\"");

WriteLiteral(" title=\"Edit the Comments\"");

WriteLiteral(">\r\n");

            
            #line 87 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
            
            
            #line default
            #line hidden
            
            #line 87 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
             using (Html.BeginForm(MVC.API.DeviceFlagAssignment.UpdateComments()))
            {

            
            #line default
            #line hidden
WriteLiteral("                <input");

WriteLiteral(" id=\"Device_Show_Flags_Actions_EditComments_Dialog_Id\"");

WriteLiteral(" type=\"hidden\"");

WriteLiteral(" name=\"id\"");

WriteLiteral(" value=\"\"");

WriteLiteral(" />\r\n");

WriteLiteral("                <input");

WriteLiteral(" type=\"hidden\"");

WriteLiteral(" name=\"redirect\"");

WriteLiteral(" value=\"true\"");

WriteLiteral(" />\r\n");

WriteLiteral("                <h4>Comments:</h4>\r\n");

WriteLiteral("                <p>\r\n                    <textarea");

WriteLiteral(" id=\"Device_Show_Flags_Actions_EditComments_Dialog_Comments\"");

WriteLiteral(" name=\"Comments\"");

WriteLiteral(" class=\"block\"");

WriteLiteral("></textarea>\r\n                </p>\r\n");

            
            #line 95 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
            }

            
            #line default
            #line hidden
WriteLiteral("        </div>\r\n");

WriteLiteral("        <script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(">\r\n            $(function () {\r\n                var deviceFlags = $(\'#deviceFlags" +
"\');\r\n\r\n                var dialog = null;\r\n                var dialogEditComment" +
"s = null;\r\n\r\n                deviceFlags.on(\'click\', \'a.remove\', function (e) {\r" +
"\n                    var $this = $(this);\r\n                    var DeviceFlagAss" +
"ignmentId = $this.closest(\'tr\').attr(\'data-deviceflagassignmentid\');\r\n\r\n        " +
"            if (!dialog) {\r\n                        dialog = $(\'#Device_Show_Fla" +
"gs_Actions_Remove_Dialog\');\r\n                        dialog.dialog({\r\n          " +
"                  resizable: false,\r\n                            modal: true,\r\n " +
"                           autoOpen: false,\r\n                            buttons" +
": {\r\n                                \"Remove Flag\": function () {\r\n             " +
"                       var $this = $(this);\r\n                                   " +
" $this.dialog(\"disable\");\r\n                                    $this.dialog(\"opt" +
"ion\", \"buttons\", null);\r\n                                    $this.find(\'form\')." +
"submit();\r\n                                },\r\n                                C" +
"ancel: function () {\r\n                                    $(this).dialog(\"close\"" +
");\r\n                                }\r\n                            }\r\n          " +
"              });\r\n                    }\r\n\r\n                    $(\'#Device_Show_" +
"Flags_Actions_Remove_Dialog_Id\').val(DeviceFlagAssignmentId);\r\n                 " +
"   dialog.dialog(\'open\');\r\n\r\n                    e.preventDefault();\r\n          " +
"          return false;\r\n                });\r\n\r\n                deviceFlags.on(\'" +
"click\', \'td.comments i.fa-edit\', function (e) {\r\n                    var $this =" +
" $(this);\r\n                    var DeviceFlagAssignmentId = $this.closest(\'tr\')." +
"attr(\'data-deviceflagassignmentid\');\r\n\r\n                    if (!dialogEditComme" +
"nts) {\r\n                        dialogEditComments = $(\'#Device_Show_Flags_Actio" +
"ns_EditComments_Dialog\');\r\n                        dialogEditComments.dialog({\r\n" +
"                            resizable: false,\r\n                            modal" +
": true,\r\n                            width: 320,\r\n                            au" +
"toOpen: false,\r\n                            buttons: {\r\n                        " +
"        \"Save Changes\": function () {\r\n                                    var $" +
"this = $(this);\r\n                                    $this.dialog(\"disable\");\r\n " +
"                                   $this.dialog(\"option\", \"buttons\", null);\r\n   " +
"                                 $this.find(\'form\').submit();\r\n                 " +
"               },\r\n                                Cancel: function () {\r\n      " +
"                              $(this).dialog(\"close\");\r\n                        " +
"        }\r\n                            }\r\n                        });\r\n         " +
"           }\r\n\r\n                    var $comments = $this.closest(\'td\').find(\'.c" +
"ommentsRaw\');\r\n                    if ($comments.hasClass(\'smallMessage\')) {\r\n  " +
"                      $(\'#Device_Show_Flags_Actions_EditComments_Dialog_Comments" +
"\').val(\'\');\r\n                    } else {\r\n                        $(\'#Device_Sh" +
"ow_Flags_Actions_EditComments_Dialog_Comments\').val($comments.text());\r\n        " +
"            }\r\n\r\n                    $(\'#Device_Show_Flags_Actions_EditComments_" +
"Dialog_Id\').val(DeviceFlagAssignmentId);\r\n                    dialogEditComments" +
".dialog(\'open\');\r\n                    e.preventDefault();\r\n                    r" +
"eturn false;\r\n                });\r\n            });\r\n        </script>\r\n");

            
            #line 174 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
    }
    else
    {

            
            #line default
            #line hidden
WriteLiteral("        <div");

WriteLiteral(" class=\"none\"");

WriteLiteral(">This device has no associated flags</div>\r\n");

            
            #line 178 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
    }

            
            #line default
            #line hidden
WriteLiteral("    <script>\r\n        $(\'#DeviceDetailTabItems\').append(\'<li><a href=\"#DeviceDeta" +
"ilTab-Flags\">Flags [");

            
            #line 180 "..\..\Views\Device\DeviceParts\_Flags.cshtml"
                                                                                  Write(activeAssignmentCount);

            
            #line default
            #line hidden
WriteLiteral("]</a></li>\');\r\n    </script>\r\n</div>\r\n");

        }
    }
}
#pragma warning restore 1591
