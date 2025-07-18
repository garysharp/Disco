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

namespace Disco.Web.Areas.Config.Views.Shared
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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Areas/Config/Views/Shared/LogEvents.cshtml")]
    public partial class LogEvents : Disco.Services.Web.WebViewPage<Disco.Web.Areas.Config.Models.Shared.LogEventsModel>
    {
        public LogEvents()
        {
        }
        public override void Execute()
        {
            
            #line 2 "..\..\Areas\Config\Views\Shared\LogEvents.cshtml"
  
    Authorization.Require(Claims.Config.Logging.Show);

    Html.BundleDeferred("~/ClientScripts/Modules/Knockout");
    Html.BundleDeferred("~/ClientScripts/Modules/jQuery-SignalR");
    var uniqueId = Guid.NewGuid().ToString("N");

            
            #line default
            #line hidden
WriteLiteral("\r\n<div");

WriteAttribute("id", Tuple.Create(" id=\"", 309), Tuple.Create("\"", 335)
, Tuple.Create(Tuple.Create("", 314), Tuple.Create("LogEvents_", 314), true)
            
            #line 9 "..\..\Areas\Config\Views\Shared\LogEvents.cshtml"
, Tuple.Create(Tuple.Create("", 324), Tuple.Create<System.Object, System.Int32>(uniqueId
            
            #line default
            #line hidden
, 324), false)
);

WriteLiteral(" class=\"logEventsViewport\"");

WriteLiteral(">\r\n");

WriteLiteral("    ");

            
            #line 10 "..\..\Areas\Config\Views\Shared\LogEvents.cshtml"
Write(Html.AntiForgeryToken());

            
            #line default
            #line hidden
WriteLiteral("\r\n    <table");

WriteLiteral(" class=\"logEventsViewport\"");

WriteLiteral(">\r\n        <thead>\r\n            <tr>\r\n                <th");

WriteLiteral(" class=\"icon\"");

WriteLiteral(">&nbsp;\r\n                </th>\r\n                <th");

WriteLiteral(" class=\"timestamp\"");

WriteLiteral(">Date/Time\r\n                </th>\r\n                <th");

WriteLiteral(" class=\"eventType\"");

WriteLiteral(">Event Type\r\n                </th>\r\n                <th");

WriteLiteral(" class=\"message\"");

WriteLiteral(">Message\r\n                </th>\r\n            </tr>\r\n        </thead>\r\n    </table" +
">\r\n    <div");

WriteLiteral(" class=\"logEventsViewportContainer\"");

WriteAttribute("style", Tuple.Create(" style=\"", 840), Tuple.Create("\"", 1050)
            
            #line 25 "..\..\Areas\Config\Views\Shared\LogEvents.cshtml"
, Tuple.Create(Tuple.Create("", 848), Tuple.Create<System.Object, System.Int32>(Model.ViewPortWidth.HasValue ? string.Format("width:{0}px;", Model.ViewPortWidth.Value) : null
            
            #line default
            #line hidden
, 848), false)
            
            #line 25 "..\..\Areas\Config\Views\Shared\LogEvents.cshtml"
                                                                      , Tuple.Create(Tuple.Create("", 945), Tuple.Create<System.Object, System.Int32>(Model.ViewPortHeight.HasValue ? string.Format("height:{0}px;", Model.ViewPortHeight.Value - 18) : null
            
            #line default
            #line hidden
, 945), false)
);

WriteLiteral(">\r\n        <div");

WriteLiteral(" class=\"logEventsViewportNoLogs\"");

WriteLiteral(" data-bind=\"visible: EventLogs().length == 0\"");

WriteLiteral("\r\n            style=\"display: none\"");

WriteLiteral(">\r\n            No logs\r\n        </div>\r\n        <table");

WriteLiteral(" class=\"logEventsViewport\"");

WriteLiteral(" data-bind=\"visible: EventLogs().length > 0\"");

WriteLiteral(" style=\"display: none\"");

WriteLiteral(">\r\n            <tbody");

WriteLiteral(" data-bind=\"foreach: EventLogs\"");

WriteLiteral(">\r\n                <tr>\r\n                    <td");

WriteLiteral(" class=\"icon\"");

WriteLiteral("><i");

WriteLiteral(" class=\"fa\"");

WriteLiteral(" data-bind=\"css: {\'fa-info-circle\': EventTypeSeverity == 0, \'fa-exclamation-trian" +
"gle\': EventTypeSeverity == 1, \'fa-exclamation-circle\': EventTypeSeverity == 2}\"");

WriteLiteral("></i></td>\r\n                    <td");

WriteLiteral(" class=\"timestamp\"");

WriteLiteral(" data-bind=\"text: FormattedTimestamp\"");

WriteLiteral("></td>\r\n                    <td");

WriteLiteral(" class=\"eventType\"");

WriteLiteral(" data-bind=\"text: EventTypeName, attr: {title: ModuleDescription}\"");

WriteLiteral("></td>\r\n                    <td");

WriteLiteral(" class=\"message\"");

WriteLiteral(" data-bind=\"text: FormattedMessage, attr: {title: $parent.LogArguments($data)}\"");

WriteLiteral("></td>\r\n                </tr>\r\n            </tbody>\r\n        </table>\r\n    </div>" +
"\r\n");

            
            #line 41 "..\..\Areas\Config\Views\Shared\LogEvents.cshtml"
    
            
            #line default
            #line hidden
            
            #line 41 "..\..\Areas\Config\Views\Shared\LogEvents.cshtml"
      
        var eventTypesFilterJson = (Model.EventTypesFilter != null) ? Newtonsoft.Json.JsonConvert.SerializeObject(Model.EventTypesFilter.Select(et => et.Id).ToArray()) : "null";
    
            
            #line default
            #line hidden
WriteLiteral("\r\n    <script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(">\r\n        $(function () {\r\n            var logEventsHost = $(\'#LogEvents_");

            
            #line 46 "..\..\Areas\Config\Views\Shared\LogEvents.cshtml"
                                          Write(uniqueId);

            
            #line default
            #line hidden
WriteLiteral("\');\r\n            var logModuleId = \'");

            
            #line 47 "..\..\Areas\Config\Views\Shared\LogEvents.cshtml"
                           Write(Model.ModuleFilter != null ? Model.ModuleFilter.ModuleId.ToString() : null);

            
            #line default
            #line hidden
WriteLiteral("\';\r\n            var logModuleLiveGroupName = \'");

            
            #line 48 "..\..\Areas\Config\Views\Shared\LogEvents.cshtml"
                                      Write(Model.ModuleFilter != null ? Model.ModuleFilter.LiveLogGroupName : Disco.Services.Logging.LogNotificationsHub.AllLoggingNotification);

            
            #line default
            #line hidden
WriteLiteral("\';\r\n            var logEventTypeFiltered = ");

            
            #line 49 "..\..\Areas\Config\Views\Shared\LogEvents.cshtml"
                                   Write(eventTypesFilterJson);

            
            #line default
            #line hidden
WriteLiteral("; \r\n            var logStartFiler = ");

            
            #line 50 "..\..\Areas\Config\Views\Shared\LogEvents.cshtml"
                            Write(AjaxHelpers.JsonDate(Model.StartFilter));

            
            #line default
            #line hidden
WriteLiteral(";\r\n            var logEndFiler = ");

            
            #line 51 "..\..\Areas\Config\Views\Shared\LogEvents.cshtml"
                          Write(AjaxHelpers.JsonDate(Model.EndFilter));

            
            #line default
            #line hidden
WriteLiteral(";\r\n            var logTakeFiler = \'");

            
            #line 52 "..\..\Areas\Config\Views\Shared\LogEvents.cshtml"
                            Write(Model.TakeFilter);

            
            #line default
            #line hidden
WriteLiteral("\';\r\n            var logHub = null;\r\n            var liveEventReceivedFunction = \'" +
"");

            
            #line 54 "..\..\Areas\Config\Views\Shared\LogEvents.cshtml"
                                         Write(Model.JavascriptLiveEventFunctionName);

            
            #line default
            #line hidden
WriteLiteral("\';\r\n            var useLive = (\'True\'===\'");

            
            #line 55 "..\..\Areas\Config\Views\Shared\LogEvents.cshtml"
                                 Write(Model.IsLive);

            
            #line default
            #line hidden
WriteLiteral(@"');

            // View Model
            var logsViewModel;
            function LogsViewModel(initialLogs){
                var self = this;

                self.EventLogs = ko.observableArray(initialLogs);
                self.LogArguments = function(log){
                    if (log.Arguments)
                        return log.Arguments.join('\n');
                    else
                        return null;
                };
            }
            function formatDate(d){
                if (d){
                    return d.getFullYear() + '-' + (d.getMonth() + 1) + '-' + d.getDate() + ' ' + d.getHours() + ':'+d.getMinutes()+':'+d.getSeconds();
                }else{
                    return null;
                }
            }
            function loadInitialData(){
                // Load Data
                var loadData = {
                    Format: ""json"",
                    Start: formatDate(logStartFiler),
                    End: logEndFiler,
                    ModuleId: logModuleId,
                    Take: logTakeFiler,
                    '__RequestVerificationToken': logEventsHost.find('input[name=""__RequestVerificationToken""]').val()
                };
                if (logEventTypeFiltered)
                    loadData[""EventTypeIds""] = logEventTypeFiltered;
                $.ajax({
                    url: '");

            
            #line 90 "..\..\Areas\Config\Views\Shared\LogEvents.cshtml"
                      Write(Url.Action(MVC.API.Logging.RetrieveEvents()));

            
            #line default
            #line hidden
WriteLiteral("\',\r\n                    dataType: \'json\',\r\n                    type: \'POST\',\r\n   " +
"                 data: loadData,\r\n                    success: function (d) { \r\n" +
"                        initLogs(d);\r\n                    },\r\n                  " +
"  error: function (jqXHR, textStatus, errorThrown) {\r\n                        al" +
"ert(\'Unable to retrieve logs: \' + textStatus);\r\n                    }\r\n         " +
"       });\r\n            }\r\n\r\n            function initLogs(loadedLogs){\r\n       " +
"         logsViewModel = new LogsViewModel(loadedLogs);\r\n                ko.appl" +
"yBindings(logsViewModel, logEventsHost.get(0));\r\n\r\n                if (useLive){" +
"\r\n                    if (liveEventReceivedFunction){\r\n                        i" +
"f (!document.DiscoFunctions) document.DiscoFunctions = {};\r\n                    " +
"    if (!document.DiscoFunctions.LogEventsFunctions) document.DiscoFunctions.Log" +
"EventsFunctions = {};\r\n                        if (document.DiscoFunctions.LogEv" +
"entsFunctions[liveEventReceivedFunction]){\r\n                            liveEven" +
"tReceivedFunction = document.DiscoFunctions.LogEventsFunctions[liveEventReceived" +
"Function];\r\n                        }else{\r\n                            liveEven" +
"tReceivedFunction = null;\r\n                        }\r\n                    }\r\n\r\n " +
"                   logHub = $.connection.logNotifications;\r\n                    " +
"logHub.client.receiveLog = function(message){\r\n                        if (messa" +
"ge.UseDisplay) logsViewModel.EventLogs.unshift(message);\r\n                      " +
"  if (liveEventReceivedFunction) liveEventReceivedFunction(message);\r\n          " +
"          };\r\n\r\n                    $.connection.hub.qs = {LogModules: logModule" +
"LiveGroupName};\r\n                    $.connection.hub.error(function (error) {\r\n" +
"                        console.log(\'Server connection error: \' + error);\r\n     " +
"               });\r\n                    $.connection.hub.disconnected(function (" +
") {\r\n                        // Show Dialog Message\r\n                        if " +
"($(\'.disconnected-dialog\').length == 0) {\r\n                            $(\'<div>\'" +
")\r\n                                .addClass(\'dialog disconnected-dialog\')\r\n    " +
"                            .html(\'<h3><span class=\"fa-stack fa-lg\"><i class=\"fa" +
" fa-wifi fa-stack-1x\"></i><i class=\"fa fa-ban fa-stack-2x error\"></i></span>Disc" +
"onnected from the Disco ICT Server</h3><div>This page is not receiving live upda" +
"tes. Please ensure you are connected to the server, then refresh this page to en" +
"able features.</div>\')\r\n                                .dialog({\r\n             " +
"                       resizable: false,\r\n                                    ti" +
"tle: \'Disconnected\',\r\n                                    width: 400,\r\n         " +
"                           modal: true,\r\n                                    but" +
"tons: {\r\n                                        \'Refresh Now\': function () {\r\n " +
"                                           $(this).dialog(\'option\', \'buttons\', n" +
"ull);\r\n                                            window.location.reload(true);" +
"\r\n                                        },\r\n                                  " +
"      \'Close\': function () {\r\n                                            $(this" +
").dialog(\'destroy\');\r\n                                        }\r\n               " +
"                     }\r\n                                });\r\n                   " +
"     }\r\n                    })\r\n\r\n                    $.connection.hub.start();\r" +
"\n                }\r\n            }\r\n\r\n            loadInitialData();\r\n        });" +
"\r\n    </script>\r\n</div>\r\n");

        }
    }
}
#pragma warning restore 1591
