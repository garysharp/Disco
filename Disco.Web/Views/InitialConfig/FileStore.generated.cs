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

namespace Disco.Web.Views.InitialConfig
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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/InitialConfig/FileStore.cshtml")]
    public partial class FileStore : Disco.Services.Web.WebViewPage<Disco.Web.Models.InitialConfig.FileStoreModel>
    {
        public FileStore()
        {
        }
        public override void Execute()
        {
            
            #line 2 "..\..\Views\InitialConfig\FileStore.cshtml"
  
    ViewBag.Title = null;
    Html.BundleDeferred("~/Style/Fancytree");
    Html.BundleDeferred("~/ClientScripts/Modules/jQuery-Fancytree");

            
            #line default
            #line hidden
WriteLiteral("\r\n<h1>");

            
            #line 7 "..\..\Views\InitialConfig\FileStore.cshtml"
Write(CommonHelpers.Breadcrumbs(Html.ToBreadcrumb("Initial Configuration", MVC.InitialConfig.Index(), "File Store")));

            
            #line default
            #line hidden
WriteLiteral("</h1>\r\n<div");

WriteLiteral(" id=\"initialConfig_FileStore\"");

WriteLiteral(">\r\n");

WriteLiteral("    ");

            
            #line 9 "..\..\Views\InitialConfig\FileStore.cshtml"
Write(Html.ValidationSummary(false));

            
            #line default
            #line hidden
WriteLiteral("\r\n\r\n    <div");

WriteLiteral(" class=\"form\"");

WriteLiteral(" style=\"width: 650px\"");

WriteLiteral(">\r\n        <h2>File Store Location</h2>\r\n        <table>\r\n            <tr>\r\n     " +
"           <td>\r\n                    <div");

WriteLiteral(" id=\"treeFilesystem\"");

WriteLiteral(">\r\n                    </div>\r\n                    <div");

WriteLiteral(" id=\"treeFilesystemActions\"");

WriteLiteral(">\r\n                        <a");

WriteLiteral(" id=\"createDirectory\"");

WriteLiteral(" href=\"#\"");

WriteLiteral(" class=\"button\"");

WriteLiteral(" disabled=\"disabled\"");

WriteLiteral(">Create Directory</a>\r\n                    </div>\r\n                </td>\r\n       " +
"     </tr>\r\n            <tr>\r\n                <td>\r\n                    <div>\r\n " +
"                       Selected Location: <span");

WriteLiteral(" id=\"locationPath\"");

WriteLiteral(" class=\"code\"");

WriteLiteral(">&lt;None&gt;</span> <span");

WriteLiteral(" id=\"locationPathInvalid\"");

WriteLiteral(" class=\"smallMessage\"");

WriteLiteral(">(Invalid DataStore Location)</span>\r\n                    </div>\r\n               " +
" </td>\r\n            </tr>\r\n        </table>\r\n    </div>\r\n");

            
            #line 32 "..\..\Views\InitialConfig\FileStore.cshtml"
    
            
            #line default
            #line hidden
            
            #line 32 "..\..\Views\InitialConfig\FileStore.cshtml"
     using (Html.BeginForm())
    {
        
            
            #line default
            #line hidden
            
            #line 34 "..\..\Views\InitialConfig\FileStore.cshtml"
   Write(Html.HiddenFor(m => m.FileStoreLocation));

            
            #line default
            #line hidden
            
            #line 34 "..\..\Views\InitialConfig\FileStore.cshtml"
                                                 

            
            #line default
            #line hidden
WriteLiteral("        <div");

WriteLiteral(" class=\"actionBar\"");

WriteLiteral(">\r\n            <input");

WriteLiteral(" id=\"submitForm\"");

WriteLiteral(" type=\"submit\"");

WriteLiteral(" class=\"button\"");

WriteLiteral(" value=\"Continue\"");

WriteLiteral(" disabled=\"disabled\"");

WriteLiteral(" />\r\n        </div>\r\n");

            
            #line 38 "..\..\Views\InitialConfig\FileStore.cshtml"
    }

            
            #line default
            #line hidden
WriteLiteral("</div>\r\n<div");

WriteLiteral(" id=\"dialogWait\"");

WriteLiteral(" title=\"Please Wait\"");

WriteLiteral(" class=\"dialog\"");

WriteLiteral(">\r\n    <h2>\r\n        <i");

WriteLiteral(" class=\"fa fa-lg fa-cog fa-spin\"");

WriteLiteral("></i>\r\n        Building and Validating File Store\r\n    </h2>\r\n    <div>Please wai" +
"t while the Disco ICT File Store is created and/or validated</div>\r\n</div>\r\n<div" +
"");

WriteLiteral(" id=\"dialogCreateDirectory\"");

WriteLiteral(" title=\"Create Directory\"");

WriteLiteral(" class=\"dialog\"");

WriteLiteral(">\r\n    <h2>Create Directory</h2>\r\n    <input");

WriteLiteral(" type=\"text\"");

WriteLiteral(" id=\"createDirectoryName\"");

WriteLiteral(" />\r\n    <div>Parent: <span");

WriteLiteral(" id=\"createDirectoryParent\"");

WriteLiteral(" class=\"code\"");

WriteLiteral("></span></div>\r\n</div>\r\n<script>\r\n    (function () {\r\n        var tree = null;\r\n " +
"       var $tree = $(\'#treeFilesystem\');\r\n        var $dialogCreateDirectory;\r\n " +
"       var fileSystemBranchUrl = \'");

            
            #line 57 "..\..\Views\InitialConfig\FileStore.cshtml"
                               Write(Url.Action(MVC.InitialConfig.FileStoreBranch()));

            
            #line default
            #line hidden
WriteLiteral("\';\r\n        var rootNodes = processNode(");

            
            #line 58 "..\..\Views\InitialConfig\FileStore.cshtml"
                                Write(new HtmlString(Json.Encode(Model.DirectoryModel)));

            
            #line default
            #line hidden
WriteLiteral(").children;\r\n\r\n        function processNodes(nodes) {\r\n            return $.map(n" +
"odes, processNode);\r\n        }\r\n        function processNode(node) {\r\n          " +
"  var children = null;\r\n            if (node.SubDirectories) {\r\n                " +
"children = $.map(node.SubDirectories, processNode);\r\n            }\r\n            " +
"return {\r\n                title: node.IsNew ? node.Name + \' [New]\' : node.Name,\r" +
"\n                key: node.Path,\r\n                folder: true,\r\n               " +
" expanded: !!children,\r\n                unselectable: !node.Selectable,\r\n       " +
"         tooltip: node.Path,\r\n                children: children,\r\n             " +
"   lazy: !children\r\n            };\r\n        }\r\n\r\n        tree = $tree.fancytree(" +
"{\r\n            source: rootNodes,\r\n            checkbox: false,\r\n            sel" +
"ectMode: 1,\r\n            keyboard: false,\r\n            lazyload: function (e, da" +
"ta) {\r\n                var node = data.node;\r\n                data.result = {\r\n " +
"                   url: fileSystemBranchUrl,\r\n                    data: { Path: " +
"node.key },\r\n                    cache: false\r\n                }\r\n            }," +
"\r\n            postProcess: function (e, data) {\r\n                data.result = p" +
"rocessNode(data.response).children;\r\n            },\r\n            activate: funct" +
"ion (e, data) {\r\n                var node = data.node;\r\n\r\n                if (no" +
"de.unselectable) {\r\n                    $(\'#submitForm\').prop(\'disabled\', true);" +
"\r\n                    $(\'#locationPathInvalid\').show();\r\n                } else " +
"{\r\n                    $(\'#submitForm\').prop(\'disabled\', false);\r\n              " +
"      $(\'#locationPathInvalid\').hide();\r\n                }\r\n\r\n                $(" +
"\'#createDirectory\').prop(\'disabled\', false);\r\n                $(\'#locationPath\')" +
".text(node.key);\r\n\r\n            }\r\n        }).fancytree(\'getTree\');\r\n\r\n        v" +
"ar initalValue = $(\'#FileStoreLocation\').val();\r\n        if (initalValue) {\r\n   " +
"         var initialNode = tree.getNodeByKey(initalValue);\r\n            if (init" +
"ialNode) {\r\n                initialNode.setActive(true);\r\n            }\r\n       " +
" }\r\n\r\n        $(\'#createDirectory\').click(function () {\r\n            if (!$(this" +
").prop(\'disabled\')) {\r\n\r\n                // Create Dialog\r\n                if (!" +
"$dialogCreateDirectory) {\r\n                    $(\'#dialogCreateDirectory\').dialo" +
"g({\r\n                        autoOpen: false,\r\n                        draggable" +
": false,\r\n                        modal: true,\r\n                        resizabl" +
"e: false,\r\n                        width: 400,\r\n                        height: " +
"200,\r\n                        buttons: {\r\n                            \'Cancel\': " +
"function () {\r\n                                $(\'#dialogCreateDirectory\').dialo" +
"g(\'close\');\r\n                            },\r\n                            \'Create" +
" Directory\': function () {\r\n                                var dirName = $(\'#cr" +
"eateDirectoryName\').val();\r\n                                if (!!dirName) {\r\n  " +
"                                  var activeNode = tree.getActiveNode();\r\n      " +
"                              if (activeNode) {\r\n                               " +
"         var parentPath = activeNode.key;\r\n                                     " +
"   var path = parentPath.charAt(parentPath.length - 1) === \'\\\\\' ? parentPath + d" +
"irName : parentPath + \'\\\\\' + dirName;\r\n                                        n" +
"ode = {\r\n                                            title: dirName + \' [New]\',\r" +
"\n                                            key: path,\r\n                       " +
"                     folder: true,\r\n                                            " +
"expanded: false,\r\n                                            unselectable: fals" +
"e,\r\n                                            tooltip: path,\r\n                " +
"                            lazy: false\r\n                                       " +
" }\r\n                                        activeNode.addNode(node).setActive(t" +
"rue);\r\n                                    }\r\n                                }\r" +
"\n                                $(\'#dialogCreateDirectory\').dialog(\'close\');\r\n " +
"                           }\r\n                        }\r\n                    })\r" +
"\n                }\r\n\r\n                var activeNode = tree.getActiveNode();\r\n  " +
"              if (activeNode) {\r\n                    $(\'#dialogCreateDirectory\')" +
".dialog(\'open\');\r\n                    $(\'#createDirectoryName\').val(\'\').focus();" +
"\r\n                    $(\'#createDirectoryParent\').text(activeNode.key);\r\n       " +
"         }\r\n\r\n                return false;\r\n            }\r\n        });\r\n\r\n     " +
"   $(\'#submitForm\').closest(\'form\').submit(function () {\r\n            var active" +
"Node = tree.getActiveNode();\r\n            if (activeNode && !activeNode.unselect" +
"able) {\r\n                $(\'#FileStoreLocation\').val(activeNode.key);\r\n         " +
"       if ($(this).valid()) {\r\n                    $(\'#dialogWait\').dialog({\r\n  " +
"                      autoOpen: true,\r\n                        draggable: false," +
"\r\n                        modal: true,\r\n                        resizable: false" +
",\r\n                        width: 400,\r\n                        height: 150,\r\n  " +
"                      closeOnEscape: false\r\n                    }).closest(\'.ui-" +
"dialog\').find(\'.ui-dialog-titlebar-close\').hide();\r\n                }\r\n         " +
"       return true;\r\n            } else {\r\n                alert(\'Invalid FileSt" +
"ore Location\');\r\n                return false;\r\n            }\r\n        });\r\n    " +
"})();\r\n</script>\r\n");

        }
    }
}
#pragma warning restore 1591
