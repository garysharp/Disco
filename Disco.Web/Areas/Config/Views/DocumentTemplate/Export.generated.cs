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
    
    #line 2 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
    using Disco.Services.Exporting;
    
    #line default
    #line hidden
    using Disco.Services.Web;
    using Disco.Web;
    
    #line 1 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
    using Disco.Web.Areas.Config.Models.DocumentTemplate;
    
    #line default
    #line hidden
    using Disco.Web.Extensions;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Areas/Config/Views/DocumentTemplate/Export.cshtml")]
    public partial class Export : Disco.Services.Web.WebViewPage<ExportModel>
    {
        public Export()
        {
        }
        public override void Execute()
        {
            
            #line 4 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
  
    Authorization.Require(Claims.Config.DocumentTemplate.Export);

    ViewBag.Title = Html.ToBreadcrumb("Configuration", MVC.Config.Config.Index(), "Document Templates", MVC.Config.DocumentTemplate.Index(null), "Export");

            
            #line default
            #line hidden
WriteLiteral("\r\n<div");

WriteLiteral(" id=\"DocumentTemplate_Export\"");

WriteLiteral(">\r\n");

            
            #line 10 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
    
            
            #line default
            #line hidden
            
            #line 10 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
     using (Html.BeginForm(MVC.API.DocumentTemplate.Export(), FormMethod.Post, new { @data_saveaction = Url.Action(MVC.API.DocumentTemplate.SaveExport()) }))
    {
        
            
            #line default
            #line hidden
            
            #line 12 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
   Write(Html.AntiForgeryToken());

            
            #line default
            #line hidden
            
            #line 12 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                                

            
            #line default
            #line hidden
WriteLiteral("        <div");

WriteLiteral(" id=\"DocumentTemplate_Export_Scope\"");

WriteLiteral(" class=\"form\"");

WriteLiteral(" style=\"width: 570px\"");

WriteLiteral(">\r\n            <h2>Export Scope</h2>\r\n            <table>\r\n                <tr>\r\n" +
"                    <th");

WriteLiteral(" style=\"width: 100px\"");

WriteLiteral(">\r\n                        Documents:\r\n                    </th>\r\n               " +
"     <td");

WriteLiteral(" class=\"details\"");

WriteLiteral(">\r\n                        <table");

WriteLiteral(" class=\"tableData\"");

WriteLiteral(">\r\n");

            
            #line 22 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                            
            
            #line default
            #line hidden
            
            #line 22 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                              
                                var index = 0;
                                foreach (var document in Model.DocumentTemplates)
                                {

            
            #line default
            #line hidden
WriteLiteral("                                    <tr>\r\n                                       " +
" <td>\r\n                                            <label>\r\n                    " +
"                            <input");

WriteLiteral(" type=\"checkbox\"");

WriteAttribute("id", Tuple.Create(" id=\"", 1353), Tuple.Create("\"", 1392)
, Tuple.Create(Tuple.Create("", 1358), Tuple.Create("Options_DocumentTemplateIds_", 1358), true)
            
            #line 29 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                       , Tuple.Create(Tuple.Create("", 1386), Tuple.Create<System.Object, System.Int32>(index
            
            #line default
            #line hidden
, 1386), false)
);

WriteLiteral(" name=\"Options.DocumentTemplateIds\"");

WriteAttribute("value", Tuple.Create(" value=\"", 1428), Tuple.Create("\"", 1448)
            
            #line 29 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                                                                         , Tuple.Create(Tuple.Create("", 1436), Tuple.Create<System.Object, System.Int32>(document.Id
            
            #line default
            #line hidden
, 1436), false)
);

WriteLiteral(" ");

            
            #line 29 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                                                                                                                                                                   Write(((bool)Model.Options.DocumentTemplateIds.Contains(document.Id)) ? "checked " : null);

            
            #line default
            #line hidden
WriteLiteral(" />\r\n                                                <strong>");

            
            #line 30 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                                                   Write(document.Id);

            
            #line default
            #line hidden
WriteLiteral("</strong>\r\n                                            </label>\r\n                " +
"                        </td>\r\n                                        <td>\r\n   " +
"                                         <label");

WriteAttribute("for", Tuple.Create(" for=\"", 1817), Tuple.Create("\"", 1857)
, Tuple.Create(Tuple.Create("", 1823), Tuple.Create("Options_DocumentTemplateIds_", 1823), true)
            
            #line 34 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
    , Tuple.Create(Tuple.Create("", 1851), Tuple.Create<System.Object, System.Int32>(index
            
            #line default
            #line hidden
, 1851), false)
);

WriteLiteral(">");

            
            #line 34 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                                                                                       Write(document.Description);

            
            #line default
            #line hidden
WriteLiteral("</label>\r\n                                        </td>\r\n                        " +
"                <td>\r\n");

WriteLiteral("                                            ");

            
            #line 37 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                                       Write(document.Scope);

            
            #line default
            #line hidden
WriteLiteral("\r\n                                        </td>\r\n                                " +
"    </tr>\r\n");

            
            #line 40 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                                    index++;
                                }
                            
            
            #line default
            #line hidden
WriteLiteral("\r\n                        </table>\r\n                    </td>\r\n                </" +
"tr>\r\n                <tr>\r\n                    <th>");

            
            #line 47 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                   Write(Html.LabelFor(m => m.Options.LatestOnly));

            
            #line default
            #line hidden
WriteLiteral("</th>\r\n                    <td>\r\n");

WriteLiteral("                        ");

            
            #line 49 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                   Write(Html.CheckBoxFor(m => m.Options.LatestOnly));

            
            #line default
            #line hidden
WriteLiteral("\r\n                        <p>Uncheck to include all document instances.</p>\r\n    " +
"                </td>\r\n                </tr>\r\n                <tr>\r\n            " +
"        <th>");

            
            #line 54 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                   Write(Html.LabelFor(m => m.Options.Format));

            
            #line default
            #line hidden
WriteLiteral("</th>\r\n                    <td>\r\n");

WriteLiteral("                        ");

            
            #line 56 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                   Write(Html.DropDownListFor(m => m.Options.Format, Enum.GetNames(typeof(Disco.Models.Exporting.ExportFormat)).Select(v => new SelectListItem() { Value = v, Text = v })));

            
            #line default
            #line hidden
WriteLiteral("\r\n                    </td>\r\n                </tr>\r\n            </table>\r\n       " +
" </div>\r\n");

WriteLiteral("        <div");

WriteLiteral(" id=\"DocumentTemplate_Export_Fields\"");

WriteLiteral(" class=\"form\"");

WriteLiteral(" style=\"width: 570px; margin-top: 15px;\"");

WriteLiteral(">\r\n            <h2>Export Fields <a");

WriteLiteral(" id=\"DocumentTemplate_Export_Fields_Defaults\"");

WriteLiteral(" href=\"#\"");

WriteLiteral(">(Defaults)</a></h2>\r\n            <table>\r\n");

            
            #line 64 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                
            
            #line default
            #line hidden
            
            #line 64 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                 foreach (var optionGroup in Model.Fields.FieldGroups)
                {
                    var optionFields = optionGroup.ToList();
                    var itemsPerColumn = (int)Math.Ceiling((double)optionFields.Count / 2);

            
            #line default
            #line hidden
WriteLiteral("                    <tr>\r\n                        <th");

WriteLiteral(" style=\"width: 120px;\"");

WriteLiteral(">\r\n");

WriteLiteral("                            ");

            
            #line 70 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                       Write(optionGroup.Name);

            
            #line default
            #line hidden
WriteLiteral("\r\n");

            
            #line 71 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                            
            
            #line default
            #line hidden
            
            #line 71 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
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

            
            #line 74 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                            }

            
            #line default
            #line hidden
WriteLiteral("                        </th>\r\n                        <td>\r\n                    " +
"        <div");

WriteLiteral(" class=\"DocumentTemplate_Export_Fields_Group\"");

WriteLiteral(">\r\n                                <table");

WriteLiteral(" class=\"none\"");

WriteLiteral(">\r\n                                    <tr>\r\n                                    " +
"    <td");

WriteLiteral(" style=\"width: 50%\"");

WriteLiteral(">\r\n                                            <ul");

WriteLiteral(" class=\"none\"");

WriteLiteral(">\r\n");

            
            #line 82 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                                                
            
            #line default
            #line hidden
            
            #line 82 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                                                 foreach (var optionItem in optionFields.Take(itemsPerColumn))
                                                {

            
            #line default
            #line hidden
WriteLiteral("                                                    <li");

WriteAttribute("title", Tuple.Create(" title=\"", 4509), Tuple.Create("\"", 4540)
            
            #line 84 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
, Tuple.Create(Tuple.Create("", 4517), Tuple.Create<System.Object, System.Int32>(optionItem.Description
            
            #line default
            #line hidden
, 4517), false)
);

WriteLiteral(">\r\n                                                        <input");

WriteLiteral(" type=\"checkbox\"");

WriteAttribute("id", Tuple.Create(" id=\"", 4622), Tuple.Create("\"", 4651)
, Tuple.Create(Tuple.Create("", 4627), Tuple.Create("Options_", 4627), true)
            
            #line 85 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
           , Tuple.Create(Tuple.Create("", 4635), Tuple.Create<System.Object, System.Int32>(optionItem.Name
            
            #line default
            #line hidden
, 4635), false)
);

WriteAttribute("name", Tuple.Create(" name=\"", 4652), Tuple.Create("\"", 4703)
, Tuple.Create(Tuple.Create("", 4659), Tuple.Create("Options.", 4659), true)
            
            #line 85 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                                            , Tuple.Create(Tuple.Create("", 4667), Tuple.Create<System.Object, System.Int32>(optionItem.Key ?? optionItem.Name
            
            #line default
            #line hidden
, 4667), false)
);

WriteAttribute("value", Tuple.Create(" value=\"", 4704), Tuple.Create("\"", 4741)
            
            #line 85 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                                                                                         , Tuple.Create(Tuple.Create("", 4712), Tuple.Create<System.Object, System.Int32>(optionItem.Value ?? "true"
            
            #line default
            #line hidden
, 4712), false)
);

WriteLiteral(" ");

            
            #line 85 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                                                                                                                                                                                                   Write((optionItem.Checked) ? "checked " : null);

            
            #line default
            #line hidden
WriteLiteral(" /><label");

WriteAttribute("for", Tuple.Create(" for=\"", 4795), Tuple.Create("\"", 4825)
, Tuple.Create(Tuple.Create("", 4801), Tuple.Create("Options_", 4801), true)
            
            #line 85 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                                                                                                                                                                                         , Tuple.Create(Tuple.Create("", 4809), Tuple.Create<System.Object, System.Int32>(optionItem.Name
            
            #line default
            #line hidden
, 4809), false)
);

WriteLiteral(">");

            
            #line 85 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                                                                                                                                                                                                                                                                                      Write(optionItem.DisplayName);

            
            #line default
            #line hidden
WriteLiteral("</label>\r\n                                                    </li>\r\n");

            
            #line 87 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                                                }

            
            #line default
            #line hidden
WriteLiteral("                                            </ul>\r\n                              " +
"          </td>\r\n                                        <td");

WriteLiteral(" style=\"width: 50%\"");

WriteLiteral(">\r\n                                            <ul");

WriteLiteral(" class=\"none\"");

WriteLiteral(">\r\n");

            
            #line 92 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                                                
            
            #line default
            #line hidden
            
            #line 92 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                                                 foreach (var optionItem in optionFields.Skip(itemsPerColumn))
                                                {

            
            #line default
            #line hidden
WriteLiteral("                                                    <li");

WriteAttribute("title", Tuple.Create(" title=\"", 5414), Tuple.Create("\"", 5445)
            
            #line 94 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
, Tuple.Create(Tuple.Create("", 5422), Tuple.Create<System.Object, System.Int32>(optionItem.Description
            
            #line default
            #line hidden
, 5422), false)
);

WriteLiteral(">\r\n                                                        <input");

WriteLiteral(" type=\"checkbox\"");

WriteAttribute("id", Tuple.Create(" id=\"", 5527), Tuple.Create("\"", 5556)
, Tuple.Create(Tuple.Create("", 5532), Tuple.Create("Options_", 5532), true)
            
            #line 95 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
           , Tuple.Create(Tuple.Create("", 5540), Tuple.Create<System.Object, System.Int32>(optionItem.Name
            
            #line default
            #line hidden
, 5540), false)
);

WriteAttribute("name", Tuple.Create(" name=\"", 5557), Tuple.Create("\"", 5608)
, Tuple.Create(Tuple.Create("", 5564), Tuple.Create("Options.", 5564), true)
            
            #line 95 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                                            , Tuple.Create(Tuple.Create("", 5572), Tuple.Create<System.Object, System.Int32>(optionItem.Key ?? optionItem.Name
            
            #line default
            #line hidden
, 5572), false)
);

WriteAttribute("value", Tuple.Create(" value=\"", 5609), Tuple.Create("\"", 5646)
            
            #line 95 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                                                                                         , Tuple.Create(Tuple.Create("", 5617), Tuple.Create<System.Object, System.Int32>(optionItem.Value ?? "true"
            
            #line default
            #line hidden
, 5617), false)
);

WriteLiteral(" ");

            
            #line 95 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                                                                                                                                                                                                   Write((optionItem.Checked) ? "checked " : null);

            
            #line default
            #line hidden
WriteLiteral(" /><label");

WriteAttribute("for", Tuple.Create(" for=\"", 5700), Tuple.Create("\"", 5730)
, Tuple.Create(Tuple.Create("", 5706), Tuple.Create("Options_", 5706), true)
            
            #line 95 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                                                                                                                                                                                         , Tuple.Create(Tuple.Create("", 5714), Tuple.Create<System.Object, System.Int32>(optionItem.Name
            
            #line default
            #line hidden
, 5714), false)
);

WriteLiteral(">");

            
            #line 95 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                                                                                                                                                                                                                                                                                      Write(optionItem.DisplayName);

            
            #line default
            #line hidden
WriteLiteral("</label>\r\n                                                    </li>\r\n");

            
            #line 97 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
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

            
            #line 105 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"

                }

            
            #line default
            #line hidden
WriteLiteral("            </table>\r\n        </div>\r\n");

WriteLiteral("        <script>\r\n            $(function () {\r\n                var exportDefaultF" +
"ields = [\'Id\', \'Description\', \'Scope\', \'AttachmentId\', \'AttachmentCreatedUser\', " +
"\'AttachmentCreatedDate\', \'AttachmentComments\', \'DeviceSerialNumber\', \'JobId\', \'J" +
"obStatus\', \'JobType\', \'UserId\', \'UserDisplayName\'];\r\n                var $export" +
"Fields = $(\'#DocumentTemplate_Export_Fields\');\r\n                var $exportScope" +
" = $(\'#DocumentTemplate_Export_Scope\');\r\n                var $form = $exportScop" +
"e.closest(\'form\');\r\n                var $exportingDialog = null;\r\n\r\n            " +
"    $exportFields.on(\'click\', \'a.selectAll,a.selectNone\', function () {\r\n       " +
"             var $this = $(this);\r\n\r\n                    $this.closest(\'tr\').fin" +
"d(\'input\').prop(\'checked\', $this.is(\'.selectAll\'));\r\n\r\n                    retur" +
"n false;\r\n                });\r\n\r\n                $(\'#DocumentTemplate_Export_Fie" +
"lds_Defaults\').click(function () {\r\n\r\n                    $exportFields.find(\'in" +
"put\').prop(\'checked\', false);\r\n\r\n                    $.each(exportDefaultFields," +
" function (index, value) {\r\n                        $(\'#Options_\' + value).prop(" +
"\'checked\', true);\r\n                    });\r\n\r\n                    return false;\r" +
"\n                });\r\n\r\n                // Submit Validation\r\n                fu" +
"nction submitHandler() {\r\n                    var exportFieldCount = $exportFiel" +
"ds.find(\'input:checked\').length;\r\n\r\n                    if (exportFieldCount > 0" +
") {\r\n\r\n                        if ($exportingDialog == null) {\r\n                " +
"            $exportingDialog = $(\'#DocumentTemplate_Export_Exporting\').dialog({\r" +
"\n                                width: 400,\r\n                                he" +
"ight: 164,\r\n                                resizable: false,\r\n                 " +
"               modal: true,\r\n                                autoOpen: false\r\n  " +
"                          });\r\n                        }\r\n                      " +
"  $exportingDialog.dialog(\'open\');\r\n\r\n                        $form[0].submit();" +
"\r\n                    }\r\n                    else\r\n                        alert" +
"(\'Select at least one field to export.\');\r\n                }\r\n                $." +
"validator.unobtrusive.parse($form);\r\n                $form.data(\"validator\").set" +
"tings.submitHandler = submitHandler;\r\n\r\n                $(\'#DocumentTemplate_Exp" +
"ort_Download_Dialog\').dialog({\r\n                    width: 400,\r\n               " +
"     height: 164,\r\n                    resizable: false,\r\n                    mo" +
"dal: true,\r\n                    autoOpen: true\r\n                });\r\n           " +
"     $(\'#DocumentTemplate_Export_Button\').click(function () {\r\n                 " +
"   $form.submit();\r\n                });\r\n                $(\'#DocumentTemplate_Ex" +
"port_Save_Button\').click(function () {\r\n                    $form.attr(\'action\'," +
" $form[0].dataset.saveaction);\r\n                    $form.submit();\r\n           " +
"     });\r\n            });\r\n        </script>\r\n");

            
            #line 177 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
    }

            
            #line default
            #line hidden
WriteLiteral("</div>\r\n");

            
            #line 179 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
 if (Model.ExportId.HasValue)
{

            
            #line default
            #line hidden
WriteLiteral("    <div");

WriteLiteral(" id=\"DocumentTemplate_Export_Download_Dialog\"");

WriteLiteral(" class=\"dialog\"");

WriteLiteral(" title=\"Export Document Instances\"");

WriteLiteral(">\r\n");

            
            #line 182 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
        
            
            #line default
            #line hidden
            
            #line 182 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
         if (Model.ExportResult.RecordCount == 0)
        {

            
            #line default
            #line hidden
WriteLiteral("            <h4>No records matched the filter criteria</h4>\r\n");

            
            #line 185 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
        }
        else
        {

            
            #line default
            #line hidden
WriteLiteral("            <h4>");

            
            #line 188 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
           Write(Model.ExportResult.RecordCount);

            
            #line default
            #line hidden
WriteLiteral(" record");

            
            #line 188 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
                                                  Write(Model.ExportResult.RecordCount != 1 ? "s" : null);

            
            #line default
            #line hidden
WriteLiteral(" were successfully exported.</h4>\r\n");

WriteLiteral("            <a");

WriteAttribute("href", Tuple.Create(" href=\"", 9605), Tuple.Create("\"", 9686)
            
            #line 189 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
, Tuple.Create(Tuple.Create("", 9612), Tuple.Create<System.Object, System.Int32>(Url.Action(MVC.API.DocumentTemplate.ExportRetrieve(Model.ExportId.Value))
            
            #line default
            #line hidden
, 9612), false)
);

WriteLiteral(" class=\"button\"");

WriteLiteral("><i");

WriteLiteral(" class=\"fa fa-download fa-lg\"");

WriteLiteral("></i>Download Document Instance Export</a>\r\n");

            
            #line 190 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
        }

            
            #line default
            #line hidden
WriteLiteral("    </div>\r\n");

WriteLiteral(@"    <script>
        $(function () {
            $('#DocumentTemplate_Export_Download_Dialog')
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

            
            #line 204 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
}

            
            #line default
            #line hidden
WriteLiteral("<div");

WriteLiteral(" id=\"DocumentTemplate_Export_Exporting\"");

WriteLiteral(" class=\"dialog\"");

WriteLiteral(" title=\"Exporting Document Instances...\"");

WriteLiteral(">\r\n    <h4><i");

WriteLiteral(" class=\"fa fa-lg fa-cog fa-spin\"");

WriteLiteral(" title=\"Please Wait\"");

WriteLiteral("></i>Exporting document instances...</h4>\r\n</div>\r\n<div");

WriteLiteral(" class=\"actionBar\"");

WriteLiteral(">\r\n");

            
            #line 209 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
    
            
            #line default
            #line hidden
            
            #line 209 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
     if (Authorization.Has(Claims.Config.ManageSavedExports))
    {

            
            #line default
            #line hidden
WriteLiteral("        <button");

WriteLiteral(" type=\"button\"");

WriteLiteral(" id=\"DocumentTemplate_Export_Save_Button\"");

WriteLiteral(" class=\"button\"");

WriteLiteral(">Save Export</button>\r\n");

            
            #line 212 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
    }
    else
    {

            
            #line default
            #line hidden
WriteLiteral("        <button");

WriteLiteral(" type=\"button\"");

WriteLiteral(" class=\"button\"");

WriteLiteral(" disabled");

WriteLiteral(" title=\"Requires Manage Saved Exports Permission\"");

WriteLiteral(">Save Export</button>\r\n");

            
            #line 216 "..\..\Areas\Config\Views\DocumentTemplate\Export.cshtml"
    }

            
            #line default
            #line hidden
WriteLiteral("\r\n    <button");

WriteLiteral(" type=\"button\"");

WriteLiteral(" id=\"DocumentTemplate_Export_Button\"");

WriteLiteral(" class=\"button\"");

WriteLiteral(">Export Now</button>\r\n</div>\r\n");

        }
    }
}
#pragma warning restore 1591
