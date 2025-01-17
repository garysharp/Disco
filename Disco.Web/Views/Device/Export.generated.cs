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
    using Disco.Services.Web;
    using Disco.Web;
    using Disco.Web.Extensions;
    
    #line 1 "..\..\Views\Device\Export.cshtml"
    using Disco.Web.Models.Device;
    
    #line default
    #line hidden
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Device/Export.cshtml")]
    public partial class Export : Disco.Services.Web.WebViewPage<ExportModel>
    {
        public Export()
        {
        }
        public override void Execute()
        {
            
            #line 3 "..\..\Views\Device\Export.cshtml"
  
    Authorization.RequireAny(Claims.Device.Actions.Export);

    ViewBag.Title = Html.ToBreadcrumb("Devices", MVC.Device.Index(), "Export Devices");

    var optionsMetadata = ModelMetadata.FromLambdaExpression(m => m.Options, ViewData);
    var optionGroups = optionsMetadata.Properties.Where(p => p.ShortDisplayName != null && p.ModelType == typeof(bool))
        .GroupBy(m => m.ShortDisplayName);

            
            #line default
            #line hidden
WriteLiteral("\r\n<div");

WriteLiteral(" id=\"Devices_Export\"");

WriteLiteral(">\r\n");

            
            #line 13 "..\..\Views\Device\Export.cshtml"
    
            
            #line default
            #line hidden
            
            #line 13 "..\..\Views\Device\Export.cshtml"
     using (Html.BeginForm(MVC.API.Device.Export()))
    {
        
            
            #line default
            #line hidden
            
            #line 15 "..\..\Views\Device\Export.cshtml"
   Write(Html.AntiForgeryToken());

            
            #line default
            #line hidden
            
            #line 15 "..\..\Views\Device\Export.cshtml"
                                

            
            #line default
            #line hidden
WriteLiteral("        <div");

WriteLiteral(" id=\"Devices_Export_Type\"");

WriteLiteral(" class=\"form\"");

WriteLiteral(" style=\"width: 570px\"");

WriteLiteral(">\r\n            <h2>Export Type</h2>\r\n            <table>\r\n                <tr>\r\n " +
"                   <th");

WriteLiteral(" style=\"width: 150px\"");

WriteLiteral(">\r\n                        Type:\r\n                    </th>\r\n                    " +
"<td>\r\n");

WriteLiteral("                        ");

            
            #line 24 "..\..\Views\Device\Export.cshtml"
                   Write(Html.DropDownListFor(m => m.Options.ExportType, Enum.GetNames(typeof(Disco.Models.Services.Devices.Exporting.DeviceExportTypes)).Select(t => new SelectListItem() { Text = t, Value = t })));

            
            #line default
            #line hidden
WriteLiteral("\r\n                        <div");

WriteLiteral(" id=\"Devices_Export_Type_Target_Batch\"");

WriteLiteral(" class=\"Devices_Export_Type_Target\"");

WriteLiteral(">\r\n");

WriteLiteral("                            ");

            
            #line 26 "..\..\Views\Device\Export.cshtml"
                       Write(Html.DropDownListFor(m => m.Options.ExportTypeTargetId, Model.DeviceBatches.Select(i => new SelectListItem() { Value = i.Key.ToString(), Text = i.Value })));

            
            #line default
            #line hidden
WriteLiteral("\r\n                        </div>\r\n                        <div");

WriteLiteral(" id=\"Devices_Export_Type_Target_Model\"");

WriteLiteral(" class=\"Devices_Export_Type_Target\"");

WriteLiteral(">\r\n");

WriteLiteral("                            ");

            
            #line 29 "..\..\Views\Device\Export.cshtml"
                       Write(Html.DropDownListFor(m => m.Options.ExportTypeTargetId, Model.DeviceModels.Select(i => new SelectListItem() { Value = i.Key.ToString(), Text = i.Value })));

            
            #line default
            #line hidden
WriteLiteral("\r\n                        </div>\r\n                        <div");

WriteLiteral(" id=\"Devices_Export_Type_Target_Profile\"");

WriteLiteral(" class=\"Devices_Export_Type_Target\"");

WriteLiteral(">\r\n");

WriteLiteral("                            ");

            
            #line 32 "..\..\Views\Device\Export.cshtml"
                       Write(Html.DropDownListFor(m => m.Options.ExportTypeTargetId, Model.DeviceProfiles.Select(i => new SelectListItem() { Value = i.Key.ToString(), Text = i.Value })));

            
            #line default
            #line hidden
WriteLiteral("\r\n                        </div>\r\n                    </td>\r\n                </tr" +
">\r\n                <tr>\r\n                    <th>");

            
            #line 37 "..\..\Views\Device\Export.cshtml"
                   Write(Html.LabelFor(m => m.Options.Format));

            
            #line default
            #line hidden
WriteLiteral("</th>\r\n                    <td>\r\n");

WriteLiteral("                        ");

            
            #line 39 "..\..\Views\Device\Export.cshtml"
                   Write(Html.DropDownListFor(m => m.Options.Format, Enum.GetNames(typeof(Disco.Models.Exporting.ExportFormat)).Select(v => new SelectListItem() { Value = v, Text = v })));

            
            #line default
            #line hidden
WriteLiteral("\r\n                    </td>\r\n                </tr>\r\n            </table>\r\n       " +
" </div>\r\n");

WriteLiteral("        <div");

WriteLiteral(" id=\"Devices_Export_Fields\"");

WriteLiteral(" class=\"form\"");

WriteLiteral(" style=\"width: 570px; margin-top: 15px;\"");

WriteLiteral(">\r\n            <h2>Export Fields <a");

WriteLiteral(" id=\"Devices_Export_Fields_Defaults\"");

WriteLiteral(" href=\"#\"");

WriteLiteral(">(Defaults)</a></h2>\r\n            <table>\r\n");

            
            #line 47 "..\..\Views\Device\Export.cshtml"
                
            
            #line default
            #line hidden
            
            #line 47 "..\..\Views\Device\Export.cshtml"
                 foreach (var optionGroup in optionGroups)
                {
                    var optionFields = optionGroup.ToList();
                    var itemsPerColumn = (int)Math.Ceiling((double)optionFields.Count / 2);

            
            #line default
            #line hidden
WriteLiteral("                    <tr>\r\n                        <th");

WriteLiteral(" style=\"width: 120px;\"");

WriteLiteral(">\r\n");

WriteLiteral("                            ");

            
            #line 53 "..\..\Views\Device\Export.cshtml"
                       Write(optionGroup.Key);

            
            #line default
            #line hidden
WriteLiteral("\r\n");

            
            #line 54 "..\..\Views\Device\Export.cshtml"
                            
            
            #line default
            #line hidden
            
            #line 54 "..\..\Views\Device\Export.cshtml"
                             if (optionFields.Count > 2)
                            {

            
            #line default
            #line hidden
WriteLiteral("                                <span");

WriteLiteral(" style=\"display: block;\"");

WriteLiteral(" class=\"select\"");

WriteLiteral("><a");

WriteLiteral(" class=\"selectAll\"");

WriteLiteral(" href=\"#\"");

WriteLiteral(">ALL</a> | <a");

WriteLiteral(" class=\"selectNone\"");

WriteLiteral(" href=\"#\"");

WriteLiteral(">NONE</a></span>\r\n");

            
            #line 57 "..\..\Views\Device\Export.cshtml"
                            }

            
            #line default
            #line hidden
WriteLiteral("                        </th>\r\n                        <td>\r\n                    " +
"        <div");

WriteLiteral(" class=\"Devices_Export_Fields_Group\"");

WriteLiteral(">\r\n                                <table");

WriteLiteral(" class=\"none\"");

WriteLiteral(">\r\n                                    <tr>\r\n                                    " +
"    <td");

WriteLiteral(" style=\"width: 50%\"");

WriteLiteral(">\r\n                                            <ul");

WriteLiteral(" class=\"none\"");

WriteLiteral(">\r\n");

            
            #line 65 "..\..\Views\Device\Export.cshtml"
                                                
            
            #line default
            #line hidden
            
            #line 65 "..\..\Views\Device\Export.cshtml"
                                                 foreach (var optionItem in optionFields.Take(itemsPerColumn))
                                                {

            
            #line default
            #line hidden
WriteLiteral("                                                    <li");

WriteAttribute("title", Tuple.Create(" title=\"", 3928), Tuple.Create("\"", 3959)
            
            #line 67 "..\..\Views\Device\Export.cshtml"
, Tuple.Create(Tuple.Create("", 3936), Tuple.Create<System.Object, System.Int32>(optionItem.Description
            
            #line default
            #line hidden
, 3936), false)
);

WriteLiteral(">\r\n                                                        <input");

WriteLiteral(" type=\"checkbox\"");

WriteAttribute("id", Tuple.Create(" id=\"", 4041), Tuple.Create("\"", 4078)
, Tuple.Create(Tuple.Create("", 4046), Tuple.Create("Options_", 4046), true)
            
            #line 68 "..\..\Views\Device\Export.cshtml"
           , Tuple.Create(Tuple.Create("", 4054), Tuple.Create<System.Object, System.Int32>(optionItem.PropertyName
            
            #line default
            #line hidden
, 4054), false)
);

WriteAttribute("name", Tuple.Create(" name=\"", 4079), Tuple.Create("\"", 4118)
, Tuple.Create(Tuple.Create("", 4086), Tuple.Create("Options.", 4086), true)
            
            #line 68 "..\..\Views\Device\Export.cshtml"
                                                   , Tuple.Create(Tuple.Create("", 4094), Tuple.Create<System.Object, System.Int32>(optionItem.PropertyName
            
            #line default
            #line hidden
, 4094), false)
);

WriteLiteral(" value=\"true\"");

WriteLiteral(" ");

            
            #line 68 "..\..\Views\Device\Export.cshtml"
                                                                                                                                                                      Write(((bool)optionItem.Model) ? "checked " : null);

            
            #line default
            #line hidden
WriteLiteral(" /><label");

WriteAttribute("for", Tuple.Create(" for=\"", 4189), Tuple.Create("\"", 4227)
, Tuple.Create(Tuple.Create("", 4195), Tuple.Create("Options_", 4195), true)
            
            #line 68 "..\..\Views\Device\Export.cshtml"
                                                                                                                                                                , Tuple.Create(Tuple.Create("", 4203), Tuple.Create<System.Object, System.Int32>(optionItem.PropertyName
            
            #line default
            #line hidden
, 4203), false)
);

WriteLiteral(">");

            
            #line 68 "..\..\Views\Device\Export.cshtml"
                                                                                                                                                                                                                                                                     Write(optionItem.DisplayName);

            
            #line default
            #line hidden
WriteLiteral("</label>\r\n                                                    </li>\r\n");

            
            #line 70 "..\..\Views\Device\Export.cshtml"
                                                }

            
            #line default
            #line hidden
WriteLiteral("                                            </ul>\r\n                              " +
"          </td>\r\n                                        <td");

WriteLiteral(" style=\"width: 50%\"");

WriteLiteral(">\r\n                                            <ul");

WriteLiteral(" class=\"none\"");

WriteLiteral(">\r\n");

            
            #line 75 "..\..\Views\Device\Export.cshtml"
                                                
            
            #line default
            #line hidden
            
            #line 75 "..\..\Views\Device\Export.cshtml"
                                                 foreach (var optionItem in optionFields.Skip(itemsPerColumn))
                                                {

            
            #line default
            #line hidden
WriteLiteral("                                                    <li");

WriteAttribute("title", Tuple.Create(" title=\"", 4816), Tuple.Create("\"", 4847)
            
            #line 77 "..\..\Views\Device\Export.cshtml"
, Tuple.Create(Tuple.Create("", 4824), Tuple.Create<System.Object, System.Int32>(optionItem.Description
            
            #line default
            #line hidden
, 4824), false)
);

WriteLiteral(">\r\n                                                        <input");

WriteLiteral(" type=\"checkbox\"");

WriteAttribute("id", Tuple.Create(" id=\"", 4929), Tuple.Create("\"", 4966)
, Tuple.Create(Tuple.Create("", 4934), Tuple.Create("Options_", 4934), true)
            
            #line 78 "..\..\Views\Device\Export.cshtml"
           , Tuple.Create(Tuple.Create("", 4942), Tuple.Create<System.Object, System.Int32>(optionItem.PropertyName
            
            #line default
            #line hidden
, 4942), false)
);

WriteAttribute("name", Tuple.Create(" name=\"", 4967), Tuple.Create("\"", 5006)
, Tuple.Create(Tuple.Create("", 4974), Tuple.Create("Options.", 4974), true)
            
            #line 78 "..\..\Views\Device\Export.cshtml"
                                                   , Tuple.Create(Tuple.Create("", 4982), Tuple.Create<System.Object, System.Int32>(optionItem.PropertyName
            
            #line default
            #line hidden
, 4982), false)
);

WriteLiteral(" value=\"true\"");

WriteLiteral(" ");

            
            #line 78 "..\..\Views\Device\Export.cshtml"
                                                                                                                                                                      Write(((bool)optionItem.Model) ? "checked " : null);

            
            #line default
            #line hidden
WriteLiteral(" /><label");

WriteAttribute("for", Tuple.Create(" for=\"", 5077), Tuple.Create("\"", 5115)
, Tuple.Create(Tuple.Create("", 5083), Tuple.Create("Options_", 5083), true)
            
            #line 78 "..\..\Views\Device\Export.cshtml"
                                                                                                                                                                , Tuple.Create(Tuple.Create("", 5091), Tuple.Create<System.Object, System.Int32>(optionItem.PropertyName
            
            #line default
            #line hidden
, 5091), false)
);

WriteLiteral(">");

            
            #line 78 "..\..\Views\Device\Export.cshtml"
                                                                                                                                                                                                                                                                     Write(optionItem.DisplayName);

            
            #line default
            #line hidden
WriteLiteral("</label>\r\n                                                    </li>\r\n");

            
            #line 80 "..\..\Views\Device\Export.cshtml"
                                                }

            
            #line default
            #line hidden
WriteLiteral(@"                                            </ul>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                        </td>
                    </tr>
");

            
            #line 88 "..\..\Views\Device\Export.cshtml"

                }

            
            #line default
            #line hidden
WriteLiteral("            </table>\r\n        </div>\r\n");

WriteLiteral("        <script>\r\n            $(function () {\r\n                var exportDefaultF" +
"ields = [\'DeviceSerialNumber\', \'ModelId\', \'ProfileId\', \'BatchId\', \'AssignedUserI" +
"d\', \'DeviceLocation\', \'DeviceAssetNumber\'];\r\n                var $exportFields =" +
" $(\'#Devices_Export_Fields\');\r\n                var $exportType = $(\'#Options_Exp" +
"ortType\');\r\n                var $exportTypeTargetContainers = $(\'#Devices_Export" +
"_Type\').find(\'.Devices_Export_Type_Target\');\r\n                var $form = $expor" +
"tType.closest(\'form\');\r\n                var $exportingDialog = null;\r\n\r\n        " +
"        function exportTypeChange() {\r\n                    $exportTypeTargetCont" +
"ainers.hide();\r\n                    $exportTypeTargetContainers.find(\'select\').p" +
"rop(\'disabled\', true);\r\n\r\n                    switch ($exportType.val()) {\r\n    " +
"                    case \'Batch\':\r\n                            $(\'#Devices_Expor" +
"t_Type_Target_Batch\').show().find(\'select\').prop(\'disabled\', false);\r\n          " +
"                  break;\r\n                        case \'Profile\':\r\n             " +
"               $(\'#Devices_Export_Type_Target_Profile\').show().find(\'select\').pr" +
"op(\'disabled\', false);\r\n                            break;\r\n                    " +
"    case \'Model\':\r\n                            $(\'#Devices_Export_Type_Target_Mo" +
"del\').show().find(\'select\').prop(\'disabled\', false);\r\n                          " +
"  break;\r\n                    }\r\n                }\r\n                $exportType." +
"change(exportTypeChange);\r\n                exportTypeChange();\r\n\r\n              " +
"  $exportFields.on(\'click\', \'a.selectAll,a.selectNone\', function () {\r\n         " +
"           var $this = $(this);\r\n\r\n                    $this.closest(\'tr\').find(" +
"\'input\').prop(\'checked\', $this.is(\'.selectAll\'));\r\n\r\n                    return " +
"false;\r\n                });\r\n\r\n                $(\'#Devices_Export_Fields_Default" +
"s\').click(function () {\r\n\r\n                    $exportFields.find(\'input\').prop(" +
"\'checked\', false);\r\n\r\n                    $.each(exportDefaultFields, function (" +
"index, value) {\r\n                        $(\'#Options_\' + value).prop(\'checked\', " +
"true);\r\n                    });\r\n\r\n                    return false;\r\n          " +
"      });\r\n\r\n                // Submit Validation\r\n                function subm" +
"itHandler() {\r\n                    var exportFieldCount = $exportFields.find(\'in" +
"put:checked\').length;\r\n\r\n                    if (exportFieldCount > 0) {\r\n\r\n    " +
"                    if ($exportingDialog == null) {\r\n                           " +
" $exportingDialog = $(\'#Devices_Export_Exporting\').dialog({\r\n                   " +
"             width: 400,\r\n                                height: 164,\r\n        " +
"                        resizable: false,\r\n                                modal" +
": true,\r\n                                autoOpen: false\r\n                      " +
"      });\r\n                        }\r\n                        $exportingDialog.d" +
"ialog(\'open\');\r\n\r\n                        $form[0].submit();\r\n                  " +
"  }\r\n                    else\r\n                        alert(\'Select at least on" +
"e field to export.\');\r\n                }\r\n                $.validator.unobtrusiv" +
"e.parse($form);\r\n                $form.data(\"validator\").settings.submitHandler " +
"= submitHandler;\r\n\r\n                $(\'#Devices_Export_Download_Dialog\').dialog(" +
"{\r\n                    width: 400,\r\n                    height: 164,\r\n          " +
"          resizable: false,\r\n                    modal: true,\r\n                 " +
"   autoOpen: true\r\n                });\r\n                $(\'#Devices_Export_Butto" +
"n\').click(function () {\r\n                    $form.submit();\r\n                })" +
";\r\n            });\r\n        </script>\r\n");

            
            #line 176 "..\..\Views\Device\Export.cshtml"
    }

            
            #line default
            #line hidden
WriteLiteral("</div>\r\n");

            
            #line 178 "..\..\Views\Device\Export.cshtml"
 if (Model.ExportSessionId != null)
{

            
            #line default
            #line hidden
WriteLiteral("    <div");

WriteLiteral(" id=\"Devices_Export_Download_Dialog\"");

WriteLiteral(" class=\"dialog\"");

WriteLiteral(" title=\"Export Devices\"");

WriteLiteral(">\r\n");

            
            #line 181 "..\..\Views\Device\Export.cshtml"
        
            
            #line default
            #line hidden
            
            #line 181 "..\..\Views\Device\Export.cshtml"
         if (Model.ExportSessionResult.RecordCount == 0)
        {

            
            #line default
            #line hidden
WriteLiteral("            <h4>No records matched the filter criteria</h4>\r\n");

            
            #line 184 "..\..\Views\Device\Export.cshtml"
        }
        else
        {

            
            #line default
            #line hidden
WriteLiteral("            <h4>");

            
            #line 187 "..\..\Views\Device\Export.cshtml"
           Write(Model.ExportSessionResult.RecordCount);

            
            #line default
            #line hidden
WriteLiteral(" record");

            
            #line 187 "..\..\Views\Device\Export.cshtml"
                                                         Write(Model.ExportSessionResult.RecordCount != 1 ? "s" : null);

            
            #line default
            #line hidden
WriteLiteral(" were successfully exported.</h4>\r\n");

WriteLiteral("            <a");

WriteAttribute("href", Tuple.Create(" href=\"", 9710), Tuple.Create("\"", 9782)
            
            #line 188 "..\..\Views\Device\Export.cshtml"
, Tuple.Create(Tuple.Create("", 9717), Tuple.Create<System.Object, System.Int32>(Url.Action(MVC.API.Device.ExportRetrieve(Model.ExportSessionId))
            
            #line default
            #line hidden
, 9717), false)
);

WriteLiteral(" class=\"button\"");

WriteLiteral("><i");

WriteLiteral(" class=\"fa fa-download fa-lg\"");

WriteLiteral("></i>Download Device Export</a>\r\n");

            
            #line 189 "..\..\Views\Device\Export.cshtml"
        }

            
            #line default
            #line hidden
WriteLiteral("    </div>\r\n");

WriteLiteral(@"    <script>
        $(function () {
            $('#Devices_Export_Download_Dialog')
                .dialog({
                    width: 400,
                    height: 164,
                    resizable: false,
                    modal: true,
                    autoOpen: true
                });
        });
    </script>
");

            
            #line 203 "..\..\Views\Device\Export.cshtml"
}

            
            #line default
            #line hidden
WriteLiteral("<div");

WriteLiteral(" id=\"Devices_Export_Exporting\"");

WriteLiteral(" class=\"dialog\"");

WriteLiteral(" title=\"Exporting Devices...\"");

WriteLiteral(">\r\n    <h4><i");

WriteLiteral(" class=\"fa fa-lg fa-cog fa-spin\"");

WriteLiteral(" title=\"Please Wait\"");

WriteLiteral("></i>Exporting devices...</h4>\r\n</div>\r\n<div");

WriteLiteral(" class=\"actionBar\"");

WriteLiteral(">\r\n    <a");

WriteLiteral(" id=\"Devices_Export_Button\"");

WriteLiteral(" href=\"#\"");

WriteLiteral(" class=\"button\"");

WriteLiteral(">Export Devices</a>\r\n</div>\r\n");

        }
    }
}
#pragma warning restore 1591
