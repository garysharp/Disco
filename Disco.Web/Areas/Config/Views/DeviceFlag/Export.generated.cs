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

namespace Disco.Web.Areas.Config.Views.DeviceFlag
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
    
    #line 1 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
    using Disco.Web.Areas.Config.Models.DeviceFlag;
    
    #line default
    #line hidden
    using Disco.Web.Extensions;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Areas/Config/Views/DeviceFlag/Export.cshtml")]
    public partial class Export : Disco.Services.Web.WebViewPage<ExportModel>
    {
        public Export()
        {
        }
        public override void Execute()
        {
            
            #line 3 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
  
    Authorization.RequireAny(Claims.Config.DeviceFlag.Export);

    ViewBag.Title = Html.ToBreadcrumb("Configuration", MVC.Config.Config.Index(), "Device Flags", MVC.Config.DeviceFlag.Index(null), "Export");

    var optionsMetadata = ModelMetadata.FromLambdaExpression(m => m.Options, ViewData);
    var optionGroups = optionsMetadata.Properties.Where(p => p.ShortDisplayName != null && p.ModelType == typeof(bool) && p.PropertyName != "CurrentOnly")
        .GroupBy(m => m.ShortDisplayName);

            
            #line default
            #line hidden
WriteLiteral("\r\n<div");

WriteLiteral(" id=\"DeviceFlag_Export\"");

WriteLiteral(">\r\n");

            
            #line 13 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
    
            
            #line default
            #line hidden
            
            #line 13 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
     using (Html.BeginForm(MVC.API.DeviceFlag.Export()))
    {

            
            #line default
            #line hidden
WriteLiteral("        <div");

WriteLiteral(" id=\"DeviceFlag_Export_Scope\"");

WriteLiteral(" class=\"form\"");

WriteLiteral(" style=\"width: 570px\"");

WriteLiteral(">\r\n            <h2>Export Scope</h2>\r\n            <table>\r\n                <tr>\r\n" +
"                    <th");

WriteLiteral(" style=\"width: 150px\"");

WriteLiteral(">\r\n                        Device Flags:\r\n                    </th>\r\n            " +
"        <td>\r\n");

            
            #line 23 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                        
            
            #line default
            #line hidden
            
            #line 23 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                         foreach (var flag in Model.DeviceFlags)
                        {

            
            #line default
            #line hidden
WriteLiteral("                            <div>\r\n                                <label>\r\n     " +
"                               <input");

WriteLiteral(" type=\"checkbox\"");

WriteLiteral(" id=\"Options_DeviceFlagIds\"");

WriteLiteral(" name=\"Options.DeviceFlagIds\"");

WriteAttribute("value", Tuple.Create(" value=\"", 1252), Tuple.Create("\"", 1268)
            
            #line 27 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                                          , Tuple.Create(Tuple.Create("", 1260), Tuple.Create<System.Object, System.Int32>(flag.Id
            
            #line default
            #line hidden
, 1260), false)
);

WriteLiteral(" ");

            
            #line 27 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                                                                                                                                Write(((bool)Model.Options.DeviceFlagIds.Contains(flag.Id)) ? "checked " : null);

            
            #line default
            #line hidden
WriteLiteral(" />\r\n                                    <i");

WriteAttribute("class", Tuple.Create(" class=\"", 1389), Tuple.Create("\"", 1442)
, Tuple.Create(Tuple.Create("", 1397), Tuple.Create("fa", 1397), true)
, Tuple.Create(Tuple.Create(" ", 1399), Tuple.Create("fa-", 1400), true)
            
            #line 28 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
, Tuple.Create(Tuple.Create("", 1403), Tuple.Create<System.Object, System.Int32>(flag.Icon
            
            #line default
            #line hidden
, 1403), false)
, Tuple.Create(Tuple.Create(" ", 1415), Tuple.Create("fa-lg", 1416), true)
, Tuple.Create(Tuple.Create(" ", 1421), Tuple.Create("d-", 1422), true)
            
            #line 28 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
, Tuple.Create(Tuple.Create("", 1424), Tuple.Create<System.Object, System.Int32>(flag.IconColour
            
            #line default
            #line hidden
, 1424), false)
);

WriteLiteral("></i>\r\n                                    <span>");

            
            #line 29 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                                     Write(flag.Name);

            
            #line default
            #line hidden
WriteLiteral("</span>\r\n                                </label>\r\n                            </" +
"div>\r\n");

            
            #line 32 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                        }

            
            #line default
            #line hidden
WriteLiteral("                    </td>\r\n                </tr>\r\n                <tr>\r\n         " +
"           <th>");

            
            #line 36 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                   Write(Html.LabelFor(m => m.Options.CurrentOnly));

            
            #line default
            #line hidden
WriteLiteral("</th>\r\n                    <td>\r\n");

WriteLiteral("                        ");

            
            #line 38 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                   Write(Html.CheckBoxFor(m => m.Options.CurrentOnly));

            
            #line default
            #line hidden
WriteLiteral("\r\n                        <p>Uncheck to include all historical device flag assign" +
"ments.</p>\r\n                    </td>\r\n                </tr>\r\n                <t" +
"r>\r\n                    <th>");

            
            #line 43 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                   Write(Html.LabelFor(m => m.Options.Format));

            
            #line default
            #line hidden
WriteLiteral("</th>\r\n                    <td>\r\n");

WriteLiteral("                        ");

            
            #line 45 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                   Write(Html.DropDownListFor(m => m.Options.Format, Enum.GetNames(typeof(Disco.Models.Exporting.ExportFormat)).Select(v => new SelectListItem() { Value = v, Text = v })));

            
            #line default
            #line hidden
WriteLiteral("\r\n                    </td>\r\n                </tr>\r\n            </table>\r\n       " +
" </div>\r\n");

WriteLiteral("        <div");

WriteLiteral(" id=\"DeviceFlag_Export_Fields\"");

WriteLiteral(" class=\"form\"");

WriteLiteral(" style=\"width: 570px; margin-top: 15px;\"");

WriteLiteral(">\r\n            <h2>Export Fields <a");

WriteLiteral(" id=\"DeviceFlag_Export_Fields_Defaults\"");

WriteLiteral(" href=\"#\"");

WriteLiteral(">(Defaults)</a></h2>\r\n            <table>\r\n");

            
            #line 53 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                
            
            #line default
            #line hidden
            
            #line 53 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
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

            
            #line 59 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                       Write(optionGroup.Key);

            
            #line default
            #line hidden
WriteLiteral("\r\n");

            
            #line 60 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                            
            
            #line default
            #line hidden
            
            #line 60 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
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

            
            #line 63 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                            }

            
            #line default
            #line hidden
WriteLiteral("                        </th>\r\n                        <td>\r\n                    " +
"        <div");

WriteLiteral(" class=\"DeviceFlag_Export_Fields_Group\"");

WriteLiteral(">\r\n                                <table");

WriteLiteral(" class=\"none\"");

WriteLiteral(">\r\n                                    <tr>\r\n                                    " +
"    <td");

WriteLiteral(" style=\"width: 50%\"");

WriteLiteral(">\r\n                                            <ul");

WriteLiteral(" class=\"none\"");

WriteLiteral(">\r\n");

            
            #line 71 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                                                
            
            #line default
            #line hidden
            
            #line 71 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                                                 foreach (var optionItem in optionFields.Take(itemsPerColumn))
                                                {

            
            #line default
            #line hidden
WriteLiteral("                                                    <li");

WriteAttribute("title", Tuple.Create(" title=\"", 3832), Tuple.Create("\"", 3863)
            
            #line 73 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
, Tuple.Create(Tuple.Create("", 3840), Tuple.Create<System.Object, System.Int32>(optionItem.Description
            
            #line default
            #line hidden
, 3840), false)
);

WriteLiteral(">\r\n                                                        <input");

WriteLiteral(" type=\"checkbox\"");

WriteAttribute("id", Tuple.Create(" id=\"", 3945), Tuple.Create("\"", 3982)
, Tuple.Create(Tuple.Create("", 3950), Tuple.Create("Options_", 3950), true)
            
            #line 74 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
           , Tuple.Create(Tuple.Create("", 3958), Tuple.Create<System.Object, System.Int32>(optionItem.PropertyName
            
            #line default
            #line hidden
, 3958), false)
);

WriteAttribute("name", Tuple.Create(" name=\"", 3983), Tuple.Create("\"", 4022)
, Tuple.Create(Tuple.Create("", 3990), Tuple.Create("Options.", 3990), true)
            
            #line 74 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                                                   , Tuple.Create(Tuple.Create("", 3998), Tuple.Create<System.Object, System.Int32>(optionItem.PropertyName
            
            #line default
            #line hidden
, 3998), false)
);

WriteLiteral(" value=\"true\"");

WriteLiteral(" ");

            
            #line 74 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                                                                                                                                                                      Write(((bool)optionItem.Model) ? "checked " : null);

            
            #line default
            #line hidden
WriteLiteral("/><label");

WriteAttribute("for", Tuple.Create(" for=\"", 4092), Tuple.Create("\"", 4130)
, Tuple.Create(Tuple.Create("", 4098), Tuple.Create("Options_", 4098), true)
            
            #line 74 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                                                                                                                                                               , Tuple.Create(Tuple.Create("", 4106), Tuple.Create<System.Object, System.Int32>(optionItem.PropertyName
            
            #line default
            #line hidden
, 4106), false)
);

WriteLiteral(">");

            
            #line 74 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                                                                                                                                                                                                                                                                    Write(optionItem.DisplayName);

            
            #line default
            #line hidden
WriteLiteral("</label></li>\r\n");

            
            #line 75 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                                                }

            
            #line default
            #line hidden
WriteLiteral("                                            </ul>\r\n                              " +
"          </td>\r\n                                        <td");

WriteLiteral(" style=\"width: 50%\"");

WriteLiteral(">\r\n                                            <ul");

WriteLiteral(" class=\"none\"");

WriteLiteral(">\r\n");

            
            #line 80 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                                                
            
            #line default
            #line hidden
            
            #line 80 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                                                 foreach (var optionItem in optionFields.Skip(itemsPerColumn))
                                                {

            
            #line default
            #line hidden
WriteLiteral("                                                    <li");

WriteAttribute("title", Tuple.Create(" title=\"", 4665), Tuple.Create("\"", 4696)
            
            #line 82 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
, Tuple.Create(Tuple.Create("", 4673), Tuple.Create<System.Object, System.Int32>(optionItem.Description
            
            #line default
            #line hidden
, 4673), false)
);

WriteLiteral(">\r\n                                                        <input");

WriteLiteral(" type=\"checkbox\"");

WriteAttribute("id", Tuple.Create(" id=\"", 4778), Tuple.Create("\"", 4815)
, Tuple.Create(Tuple.Create("", 4783), Tuple.Create("Options_", 4783), true)
            
            #line 83 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
           , Tuple.Create(Tuple.Create("", 4791), Tuple.Create<System.Object, System.Int32>(optionItem.PropertyName
            
            #line default
            #line hidden
, 4791), false)
);

WriteAttribute("name", Tuple.Create(" name=\"", 4816), Tuple.Create("\"", 4855)
, Tuple.Create(Tuple.Create("", 4823), Tuple.Create("Options.", 4823), true)
            
            #line 83 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                                                   , Tuple.Create(Tuple.Create("", 4831), Tuple.Create<System.Object, System.Int32>(optionItem.PropertyName
            
            #line default
            #line hidden
, 4831), false)
);

WriteLiteral(" value=\"true\"");

WriteLiteral(" ");

            
            #line 83 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                                                                                                                                                                      Write(((bool)optionItem.Model) ? "checked " : null);

            
            #line default
            #line hidden
WriteLiteral("/><label");

WriteAttribute("for", Tuple.Create(" for=\"", 4925), Tuple.Create("\"", 4963)
, Tuple.Create(Tuple.Create("", 4931), Tuple.Create("Options_", 4931), true)
            
            #line 83 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                                                                                                                                                               , Tuple.Create(Tuple.Create("", 4939), Tuple.Create<System.Object, System.Int32>(optionItem.PropertyName
            
            #line default
            #line hidden
, 4939), false)
);

WriteLiteral(">");

            
            #line 83 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                                                                                                                                                                                                                                                                    Write(optionItem.DisplayName);

            
            #line default
            #line hidden
WriteLiteral("</label></li>\r\n");

            
            #line 84 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
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

            
            #line 92 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                }

            
            #line default
            #line hidden
WriteLiteral("            </table>\r\n        </div>\r\n");

WriteLiteral("        <script>\r\n            $(function () {\r\n                var exportDefaultF" +
"ields = [\'Name\', \'AddedDate\', \'UserId\', \'UserDisplayName\', \'Comments\'];\r\n       " +
"         var $exportFields = $(\'#DeviceFlag_Export_Fields\');\r\n                va" +
"r $exportScope = $(\'#DeviceFlag_Export_Scope\');\r\n                var $form = $ex" +
"portScope.closest(\'form\');\r\n                var $exportingDialog = null;\r\n\r\n    " +
"            $exportFields.on(\'click\', \'a.selectAll,a.selectNone\', function () {\r" +
"\n                    var $this = $(this);\r\n\r\n                    $this.closest(\'" +
"tr\').find(\'input\').prop(\'checked\', $this.is(\'.selectAll\'));\r\n\r\n                 " +
"   return false;\r\n                });\r\n\r\n                $(\'#DeviceFlag_Export_F" +
"ields_Defaults\').click(function () {\r\n\r\n                    $exportFields.find(\'" +
"input\').prop(\'checked\', false);\r\n\r\n                    $.each(exportDefaultField" +
"s, function (index, value) {\r\n                        $(\'#Options_\' + value).pro" +
"p(\'checked\', true);\r\n                    });\r\n\r\n                    return false" +
";\r\n                });\r\n\r\n                // Submit Validation\r\n                " +
"function submitHandler() {\r\n                    var exportFieldCount = $exportFi" +
"elds.find(\'input:checked\').length;\r\n\r\n                    if (exportFieldCount >" +
" 0) {\r\n\r\n                        if ($exportingDialog == null) {\r\n              " +
"              $exportingDialog = $(\'#DeviceFlag_Export_Exporting\').dialog({\r\n   " +
"                             width: 400,\r\n                                height" +
": 164,\r\n                                resizable: false,\r\n                     " +
"           modal: true,\r\n                                autoOpen: false\r\n      " +
"                      });\r\n                        }\r\n                        $e" +
"xportingDialog.dialog(\'open\');\r\n\r\n                        $form[0].submit();\r\n  " +
"                  }\r\n                    else\r\n                        alert(\'Se" +
"lect at least one field to export.\');\r\n                }\r\n                $.vali" +
"dator.unobtrusive.parse($form);\r\n                $form.data(\"validator\").setting" +
"s.submitHandler = submitHandler;\r\n\r\n                $(\'#DeviceFlag_Export_Downlo" +
"ad_Dialog\').dialog({\r\n                    width: 400,\r\n                    heigh" +
"t: 164,\r\n                    resizable: false,\r\n                    modal: true," +
"\r\n                    autoOpen: true\r\n                });\r\n                $(\'#D" +
"eviceFlag_Export_Button\').click(function () {\r\n                    $form.submit(" +
");\r\n                });\r\n            });\r\n        </script>\r\n");

            
            #line 159 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
    }

            
            #line default
            #line hidden
WriteLiteral("</div>\r\n");

            
            #line 161 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
 if (Model.ExportSessionId != null)
{

            
            #line default
            #line hidden
WriteLiteral("    <div");

WriteLiteral(" id=\"DeviceFlag_Export_Download_Dialog\"");

WriteLiteral(" class=\"dialog\"");

WriteLiteral(" title=\"Export Device Flags\"");

WriteLiteral(">\r\n        <h4>");

            
            #line 164 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
       Write(Model.ExportSessionResult.RecordCount);

            
            #line default
            #line hidden
WriteLiteral(" record");

            
            #line 164 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
                                                     Write(Model.ExportSessionResult.RecordCount != 1 ? "s" : null);

            
            #line default
            #line hidden
WriteLiteral(" were successfully exported.</h4>\r\n        <a");

WriteAttribute("href", Tuple.Create(" href=\"", 8250), Tuple.Create("\"", 8326)
            
            #line 165 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
, Tuple.Create(Tuple.Create("", 8257), Tuple.Create<System.Object, System.Int32>(Url.Action(MVC.API.DeviceFlag.ExportRetrieve(Model.ExportSessionId))
            
            #line default
            #line hidden
, 8257), false)
);

WriteLiteral(" class=\"button\"");

WriteLiteral("><i");

WriteLiteral(" class=\"fa fa-download fa-lg\"");

WriteLiteral("></i>Download Device Flag Export</a>\r\n    </div>\r\n");

WriteLiteral(@"    <script>
        $(function () {
            $('#DeviceFlag_Export_Download_Dialog')
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

            
            #line 179 "..\..\Areas\Config\Views\DeviceFlag\Export.cshtml"
}

            
            #line default
            #line hidden
WriteLiteral("<div");

WriteLiteral(" id=\"DeviceFlag_Export_Exporting\"");

WriteLiteral(" class=\"dialog\"");

WriteLiteral(" title=\"Exporting Device Flags...\"");

WriteLiteral(">\r\n    <h4><i");

WriteLiteral(" class=\"fa fa-lg fa-cog fa-spin\"");

WriteLiteral(" title=\"Please Wait\"");

WriteLiteral("></i>Exporting device flags...</h4>\r\n</div>\r\n<div");

WriteLiteral(" class=\"actionBar\"");

WriteLiteral(">\r\n    <a");

WriteLiteral(" id=\"DeviceFlag_Export_Button\"");

WriteLiteral(" href=\"#\"");

WriteLiteral(" class=\"button\"");

WriteLiteral(">Export Device Flags</a>\r\n</div>\r\n");

        }
    }
}
#pragma warning restore 1591