﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34011
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Disco.Web.Areas.Config.Views.JobPreferences.Parts
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
    using Disco.BI.Extensions;
    using Disco.Models.Repository;
    using Disco.Services;
    using Disco.Services.Authorization;
    using Disco.Services.Web;
    using Disco.Web;
    using Disco.Web.Extensions;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Areas/Config/Views/JobPreferences/Parts/Locations.cshtml")]
    public partial class Locations : Disco.Services.Web.WebViewPage<Disco.Web.Areas.Config.Models.JobPreferences.IndexModel>
    {
        public Locations()
        {
        }
        public override void Execute()
        {
            
            #line 2 "..\..\Areas\Config\Views\JobPreferences\Parts\Locations.cshtml"
  
    Authorization.Require(Claims.Config.JobPreferences.Show);

    var canConfig = Authorization.Has(Claims.Config.JobPreferences.Configure);

            
            #line default
            #line hidden
WriteLiteral("\r\n<div");

WriteLiteral(" id=\"Config_Location\"");

WriteLiteral(" class=\"form\"");

WriteLiteral(" style=\"width: 530px;\"");

WriteLiteral(">\r\n    <h2>Job Locations</h2>\r\n    <table>\r\n        <tr>\r\n            <th");

WriteLiteral(" style=\"width: 200px\"");

WriteLiteral(">Mode:\r\n            </th>\r\n            <td>");

            
            #line 13 "..\..\Areas\Config\Views\JobPreferences\Parts\Locations.cshtml"
                 if (canConfig)
                {
                
            
            #line default
            #line hidden
            
            #line 15 "..\..\Areas\Config\Views\JobPreferences\Parts\Locations.cshtml"
           Write(Html.DropDownListFor(model => model.LocationMode, Model.LocationModeOptions().Select(o => new SelectListItem() { Value = o.Key.ToString(), Text = o.Value })));

            
            #line default
            #line hidden
            
            #line 15 "..\..\Areas\Config\Views\JobPreferences\Parts\Locations.cshtml"
                                                                                                                                                                              
                
            
            #line default
            #line hidden
            
            #line 16 "..\..\Areas\Config\Views\JobPreferences\Parts\Locations.cshtml"
           Write(AjaxHelpers.AjaxSave());

            
            #line default
            #line hidden
            
            #line 16 "..\..\Areas\Config\Views\JobPreferences\Parts\Locations.cshtml"
                                       
                
            
            #line default
            #line hidden
            
            #line 17 "..\..\Areas\Config\Views\JobPreferences\Parts\Locations.cshtml"
           Write(AjaxHelpers.AjaxLoader());

            
            #line default
            #line hidden
            
            #line 17 "..\..\Areas\Config\Views\JobPreferences\Parts\Locations.cshtml"
                                         

            
            #line default
            #line hidden
WriteLiteral("                <div");

WriteLiteral(" id=\"Config_Location_Unrestricted\"");

WriteLiteral(">\r\n                    <div");

WriteLiteral(" style=\"padding: 0.7em 0.7em;\"");

WriteLiteral(" class=\"ui-state-highlight ui-corner-all\"");

WriteLiteral(">\r\n                        <i");

WriteLiteral(" class=\"fa fa-info-circle information\"");

WriteLiteral("></i>&nbsp;Technicians will be able to specify <em>any</em> value when entering a" +
" location. A selection of locations used historically will be offered.\r\n        " +
"            </div>\r\n                </div>\r\n");

WriteLiteral("                <div");

WriteLiteral(" id=\"Config_Location_List\"");

WriteLiteral(">\r\n                    <a");

WriteLiteral(" id=\"Config_Location_List_Button\"");

WriteLiteral(" href=\"#\"");

WriteLiteral(" class=\"button small\"");

WriteLiteral(">Update List</a> <a");

WriteLiteral(" id=\"Config_Location_List_ImportButton\"");

WriteLiteral(" href=\"#\"");

WriteLiteral(" class=\"button small\"");

WriteLiteral(">Import List</a>\r\n                    <div");

WriteLiteral(" id=\"Config_Location_List_Dialog\"");

WriteLiteral(" class=\"dialog\"");

WriteLiteral(" title=\"Locations\"");

WriteLiteral(">\r\n                        <div");

WriteLiteral(" id=\"Config_Location_List_Dialog_ListContainer\"");

WriteLiteral(">\r\n                            <span");

WriteLiteral(" id=\"Config_Location_List_Dialog_None\"");

WriteLiteral(" class=\"smallMessage\"");

WriteLiteral(">The List is Empty</span>\r\n                            <ul");

WriteLiteral(" id=\"Config_Location_List_Dialog_List\"");

WriteLiteral(" class=\"none\"");

WriteLiteral(">\r\n");

            
            #line 29 "..\..\Areas\Config\Views\JobPreferences\Parts\Locations.cshtml"
                                
            
            #line default
            #line hidden
            
            #line 29 "..\..\Areas\Config\Views\JobPreferences\Parts\Locations.cshtml"
                                 foreach (var loc in Model.LocationList)
                                {

            
            #line default
            #line hidden
WriteLiteral("                                    <li");

WriteLiteral(" data-location=\"");

            
            #line 31 "..\..\Areas\Config\Views\JobPreferences\Parts\Locations.cshtml"
                                                  Write(loc);

            
            #line default
            #line hidden
WriteLiteral("\"");

WriteLiteral(">");

            
            #line 31 "..\..\Areas\Config\Views\JobPreferences\Parts\Locations.cshtml"
                                                        Write(loc);

            
            #line default
            #line hidden
WriteLiteral("<i");

WriteLiteral(" class=\"fa fa-times-circle remove\"");

WriteLiteral("></i></li>\r\n");

            
            #line 32 "..\..\Areas\Config\Views\JobPreferences\Parts\Locations.cshtml"
                                }

            
            #line default
            #line hidden
WriteLiteral("                            </ul>\r\n                        </div>\r\n              " +
"          <div");

WriteLiteral(" id=\"Config_Location_List_Dialog_AddContainer\"");

WriteLiteral(">\r\n                            <input");

WriteLiteral(" type=\"text\"");

WriteLiteral(" id=\"Config_Location_List_Dialog_TextAdd\"");

WriteLiteral(" />\r\n                            <a");

WriteLiteral(" id=\"Config_Location_List_Dialog_Add\"");

WriteLiteral(" href=\"#\"");

WriteLiteral(" class=\"button small\"");

WriteLiteral(">Add</a>\r\n                        </div>\r\n                        <form");

WriteLiteral(" id=\"Config_Location_List_Dialog_Form\"");

WriteAttribute("action", Tuple.Create(" action=\"", 2447), Tuple.Create("\"", 2534)
            
            #line 39 "..\..\Areas\Config\Views\JobPreferences\Parts\Locations.cshtml"
, Tuple.Create(Tuple.Create("", 2456), Tuple.Create<System.Object, System.Int32>(Url.Action(MVC.API.JobPreferences.UpdateLocationList(null, redirect: true))
            
            #line default
            #line hidden
, 2456), false)
);

WriteLiteral(" method=\"post\"");

WriteLiteral("></form>\r\n                    </div>\r\n                    <div");

WriteLiteral(" id=\"Config_Location_ListImport_Dialog\"");

WriteLiteral(" class=\"dialog\"");

WriteLiteral(" title=\"Import Locations\"");

WriteLiteral(">\r\n                        <form");

WriteLiteral(" id=\"Config_Location_ListImport_Dialog_Form\"");

WriteAttribute("action", Tuple.Create(" action=\"", 2766), Tuple.Create("\"", 2853)
            
            #line 42 "..\..\Areas\Config\Views\JobPreferences\Parts\Locations.cshtml"
   , Tuple.Create(Tuple.Create("", 2775), Tuple.Create<System.Object, System.Int32>(Url.Action(MVC.API.JobPreferences.ImportLocationList(null, redirect: true))
            
            #line default
            #line hidden
, 2775), false)
);

WriteLiteral(" method=\"post\"");

WriteLiteral(">\r\n                            <input");

WriteLiteral(" type=\"hidden\"");

WriteLiteral(" id=\"Config_Location_ListImport_Dialog_AutomaticList\"");

WriteLiteral(" name=\"AutomaticList\"");

WriteLiteral(" value=\"False\"");

WriteLiteral(" />\r\n                            <div");

WriteLiteral(" id=\"Config_Location_ListImport_Dialog_Overwrite_Container\"");

WriteLiteral(">\r\n                                <input");

WriteLiteral(" type=\"checkbox\"");

WriteLiteral(" id=\"Config_Location_ListImport_Dialog_Overwrite\"");

WriteLiteral(" name=\"Override\"");

WriteLiteral(" value=\"True\"");

WriteLiteral(" /><label");

WriteLiteral(" for=\"Config_Location_ListImport_Dialog_Overwrite\"");

WriteLiteral(">Override Existing List</label>\r\n                            </div>\r\n            " +
"                <textarea");

WriteLiteral(" id=\"Config_Location_ListImport_Dialog_LocationList\"");

WriteLiteral(" name=\"LocationList\"");

WriteLiteral("></textarea>\r\n                            <div");

WriteLiteral(" style=\"padding: 0.7em 0.7em; margin-top: 10px;\"");

WriteLiteral(" class=\"ui-state-highlight ui-corner-all\"");

WriteLiteral(">\r\n                                <i");

WriteLiteral(" class=\"fa fa-info-circle information\"");

WriteLiteral(@"></i>&nbsp;Enter multiple locations separated by <code>&lt;new line&gt;</code>, commas (<code>,</code>) or semicolons (<code>;</code>).
                            </div>
                        </form>
                    </div>
                </div>
");

WriteLiteral("                <div");

WriteLiteral(" id=\"Config_Location_Optional\"");

WriteLiteral(">\r\n                    <div");

WriteLiteral(" style=\"padding: 0.7em 0.7em;\"");

WriteLiteral(" class=\"ui-state-highlight ui-corner-all\"");

WriteLiteral(">\r\n                        <i");

WriteLiteral(" class=\"fa fa-info-circle information\"");

WriteLiteral("></i>&nbsp;Technicians will be able to specify <em>any</em> value when entering a" +
" location. A defined list of location options is suggested.\r\n                   " +
" </div>\r\n                </div>\r\n");

WriteLiteral("                <div");

WriteLiteral(" id=\"Config_Location_Restricted\"");

WriteLiteral(">\r\n                    <div");

WriteLiteral(" style=\"padding: 0.7em 0.7em;\"");

WriteLiteral(" class=\"ui-state-highlight ui-corner-all\"");

WriteLiteral(">\r\n                        <i");

WriteLiteral(" class=\"fa fa-info-circle information\"");

WriteLiteral("></i>&nbsp;Technicians are restricted to select a location from the defined list." +
"\r\n                    </div>\r\n                </div>\r\n");

WriteLiteral("                <script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(">\r\n                    $(function () {\r\n                        document.DiscoFun" +
"ctions.PropertyChangeHelper(\r\n                            $(\'#LocationMode\'),\r\n " +
"                           null,\r\n                            \'");

            
            #line 69 "..\..\Areas\Config\Views\JobPreferences\Parts\Locations.cshtml"
                         Write(Url.Action(MVC.API.JobPreferences.UpdateLocationMode()));

            
            #line default
            #line hidden
WriteLiteral("\',\r\n                            \'LocationMode\');\r\n\r\n                        var $" +
"locationMode = $(\'#LocationMode\');\r\n\r\n                        function update() " +
"{\r\n                            var $Config_Location_List = $(\'#Config_Location_L" +
"ist\');\r\n\r\n                            var $Config_Location_Unrestricted = $(\'#Co" +
"nfig_Location_Unrestricted\');\r\n                            var $Config_Location_" +
"Optional = $(\'#Config_Location_Optional\');\r\n                            var $Con" +
"fig_Location_Restricted = $(\'#Config_Location_Restricted\');\r\n\r\n\r\n               " +
"             switch ($locationMode.val()) {\r\n                                cas" +
"e \'Unrestricted\':\r\n                                    $Config_Location_List.hid" +
"e();\r\n                                    $Config_Location_Optional.hide();\r\n   " +
"                                 $Config_Location_Restricted.hide();\r\n\r\n        " +
"                            $Config_Location_Unrestricted.show();\r\n             " +
"                       break;\r\n                                case \'OptionalLis" +
"t\':\r\n                                    $Config_Location_Unrestricted.hide();\r\n" +
"                                    $Config_Location_Restricted.hide();\r\n\r\n     " +
"                               $Config_Location_List.show();\r\n                  " +
"                  $Config_Location_Optional.show();\r\n                           " +
"         break;\r\n                                case \'RestrictedList\':\r\n       " +
"                             $Config_Location_Unrestricted.hide();\r\n            " +
"                        $Config_Location_Optional.hide();\r\n\r\n                   " +
"                 $Config_Location_List.show();\r\n                                " +
"    $Config_Location_Restricted.show();\r\n                                    bre" +
"ak;\r\n                            }\r\n                        }\r\n                 " +
"       update();\r\n                        $locationMode.change(update);\r\n\r\n     " +
"                   var dialog, textAdd, list, noList, form;\r\n\r\n                 " +
"       $(\'#Config_Location_List_Button\').click(showDialog);\r\n\r\n                 " +
"       function showDialog() {\r\n                            if (!dialog) {\r\n    " +
"                            dialog = $(\'#Config_Location_List_Dialog\').dialog({\r" +
"\n                                    resizable: false,\r\n                        " +
"            modal: true,\r\n                                    autoOpen: false,\r\n" +
"                                    width: 350,\r\n                               " +
"     height: 420,\r\n                                    buttons: {\r\n             " +
"                           \"Save Changes\": saveChanges,\r\n                       " +
"                 Cancel: cancel\r\n                                    }\r\n        " +
"                        });\r\n\r\n                                dialog.on(\'click\'" +
", \'.remove\', remove);\r\n\r\n                                list = $(\'#Config_Locat" +
"ion_List_Dialog_List\');\r\n                                noList = $(\'#Config_Loc" +
"ation_List_Dialog_None\');\r\n\r\n                                textAdd = $(\'#Confi" +
"g_Location_List_Dialog_TextAdd\');\r\n\r\n                                textAdd.wat" +
"ermark(\'Location\');\r\n                                textAdd.keydown(function (e" +
") {\r\n                                    if (e.keyCode == 13)\r\n                 " +
"                       add();\r\n                                });\r\n\r\n          " +
"                      $(\'#Config_Location_List_Dialog_Add\').click(add);\r\n       " +
"                     }\r\n\r\n                            dialog.dialog(\'open\');\r\n\r\n" +
"                            updateNoList();\r\n                            return " +
"false;\r\n                        }\r\n\r\n                        function cancel() {" +
"\r\n                            $(this).dialog(\"close\");\r\n\r\n                      " +
"      list.find(\'li\').each(function () {\r\n                                $this " +
"= $(this);\r\n                                if ($this.is(\'[data-status=\"new\"]\'))" +
" {\r\n                                    $this.remove();\r\n                       " +
"         } else {\r\n                                    if ($this.is(\'[data-statu" +
"s=\"removed\"]\')) {\r\n                                        $this.show();\r\n      " +
"                                  $this.attr(\'data-status\', \'\')\r\n               " +
"                     }\r\n                                }\r\n                     " +
"       });\r\n                        }\r\n\r\n                        function remove" +
"() {\r\n                            $this = $(this).closest(\'li\');\r\n\r\n            " +
"                if ($this.is(\'[data-status=\"new\"]\')) {\r\n                        " +
"        $this.remove();\r\n                            } else {\r\n                 " +
"               $this.attr(\'data-status\', \'removed\').hide();\r\n                   " +
"         }\r\n\r\n                            updateNoList();\r\n                     " +
"   }\r\n\r\n                        function add() {\r\n\r\n                            " +
"var value = textAdd.val();\r\n\r\n                            // Trim\r\n             " +
"               value = jQuery.trim(value);\r\n\r\n                            if (!v" +
"alue) {\r\n                                alert(\'Enter a location to be added\');\r" +
"\n                                return;\r\n                            }\r\n\r\n     " +
"                       // Already Exists\r\n                            var existi" +
"ngValues = list.find(\'li[data-location]\').filter(\'[data-status!=\"removed\"]\').map" +
"(function () { return $(this).attr(\'data-location\') }).get();\r\n                 " +
"           if (jQuery.inArray(value, existingValues) >= 0) {\r\n                  " +
"              alert(\'That item already exists in the list\');\r\n                  " +
"              return;\r\n                            }\r\n\r\n                        " +
"    // Add Item\r\n                            var li = $(\'<li>\')\r\n               " +
"                 .append($(\'<span>\').text(value))\r\n                             " +
"   .append($(\'<i>\').addClass(\'fa fa-times-circle remove\'))\r\n                    " +
"            .attr(\'data-location\', value)\r\n                                .attr" +
"(\'data-status\', \'new\');\r\n\r\n                            list.append(li);\r\n\r\n     " +
"                       textAdd.focus();\r\n\r\n                            updateNoL" +
"ist();\r\n                        }\r\n\r\n                        function updateNoLi" +
"st() {\r\n                            if (list.find(\'li:visible\').length > 0)\r\n   " +
"                             noList.hide();\r\n                            else\r\n " +
"                               noList.show();\r\n                        }\r\n\r\n    " +
"                    function saveChanges() {\r\n                            var fo" +
"rm = $(\'#Config_Location_List_Dialog_Form\').empty();\r\n\r\n                        " +
"    list.find(\'li[data-status!=\"removed\"]\').each(function () {\r\n                " +
"                var location = $(this).attr(\'data-location\');\r\n\r\n               " +
"                 form.append($(\'<input>\').attr({\r\n                              " +
"      \'name\': \'LocationList\',\r\n                                    \'type\': \'hidd" +
"en\'\r\n                                }).val(location));\r\n\r\n                     " +
"       }).get();\r\n\r\n                            form.submit();\r\n\r\n              " +
"              dialog.dialog(\"disable\");\r\n                            dialog.dial" +
"og(\"option\", \"buttons\", null);\r\n                        }\r\n\r\n                   " +
"     // Import\r\n                        var dialogImport, formImport;\r\n\r\n       " +
"                 $(\'#Config_Location_List_ImportButton\').click(showDialogImport)" +
";\r\n\r\n                        function showDialogImport() {\r\n                    " +
"        if (!dialogImport) {\r\n                                dialogImport = $(\'" +
"#Config_Location_ListImport_Dialog\').dialog({\r\n                                 " +
"   resizable: false,\r\n                                    modal: true,\r\n        " +
"                            autoOpen: false,\r\n                                  " +
"  width: 350,\r\n                                    height: 420,\r\n               " +
"                     buttons: {\r\n                                        \"Build " +
"Automatic List\": function () {\r\n                                            $(\'#" +
"Config_Location_ListImport_Dialog_AutomaticList\').val(\'True\').closest(\'form\').su" +
"bmit();\r\n                                            dialogImport.dialog(\"disabl" +
"e\");\r\n                                            dialogImport.dialog(\"option\", " +
"\"buttons\", null);\r\n                                        },\r\n                 " +
"                       \"Import List\": function () {\r\n                           " +
"                 $(\'#Config_Location_ListImport_Dialog_LocationList\').closest(\'f" +
"orm\').submit();\r\n                                            dialogImport.dialog" +
"(\"disable\");\r\n                                            dialogImport.dialog(\"o" +
"ption\", \"buttons\", null);\r\n                                        },\r\n         " +
"                               Cancel: function () {\r\n                          " +
"                  dialogImport.dialog(\"close\");\r\n                               " +
"         }\r\n                                    }\r\n                             " +
"   });\r\n                            }\r\n\r\n                            dialogImpor" +
"t.dialog(\'open\');\r\n\r\n                            return false;\r\n                " +
"        }\r\n\r\n                    });\r\n                </script>\r\n");

            
            #line 274 "..\..\Areas\Config\Views\JobPreferences\Parts\Locations.cshtml"
                }
                else
                {
                
            
            #line default
            #line hidden
            
            #line 277 "..\..\Areas\Config\Views\JobPreferences\Parts\Locations.cshtml"
           Write(Model.LocationModeOptions().First(o => o.Key == Model.LocationMode.ToString()).Value);

            
            #line default
            #line hidden
            
            #line 277 "..\..\Areas\Config\Views\JobPreferences\Parts\Locations.cshtml"
                                                                                                     
                }

            
            #line default
            #line hidden
WriteLiteral("            </td>\r\n        </tr>\r\n    </table>\r\n</div>\r\n");

        }
    }
}
#pragma warning restore 1591