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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Areas/Config/Views/Shared/TaskStatus.cshtml")]
    public partial class TaskStatus : Disco.Services.Web.WebViewPage<string>
    {
        public TaskStatus()
        {
        }
        public override void Execute()
        {
            
            #line 2 "..\..\Areas\Config\Views\Shared\TaskStatus.cshtml"
  
    Html.BundleDeferred("~/ClientScripts/Modules/Knockout");
    Html.BundleDeferred("~/ClientScripts/Modules/jQuery-SignalR");

            
            #line default
            #line hidden
WriteLiteral("\r\n<div");

WriteLiteral(" style=\"min-height: 300px;\"");

WriteLiteral(">\r\n    <div");

WriteLiteral(" id=\"Logging_Task_Status\"");

WriteLiteral(" class=\"form\"");

WriteLiteral(" style=\"width: 520px;\"");

WriteLiteral(" data-bind=\"visible: Initialized\"");

WriteLiteral(">\r\n        <h2");

WriteLiteral(" data-bind=\"text: TaskName\"");

WriteLiteral(">&nbsp;</h2>\r\n        <table>\r\n            <tr");

WriteLiteral(" data-bind=\"visible: IsRunning\"");

WriteLiteral(">\r\n                <th");

WriteLiteral(" class=\"process\"");

WriteLiteral(" data-bind=\"text: CurrentProcess\"");

WriteLiteral(">&nbsp;\r\n                </th>\r\n            </tr>\r\n            <tr");

WriteLiteral(" data-bind=\"visible: IsRunning\"");

WriteLiteral(">\r\n                <td");

WriteLiteral(" class=\"description\"");

WriteLiteral(" data-bind=\"text: CurrentDescription\"");

WriteLiteral(">&nbsp;\r\n                </td>\r\n            </tr>\r\n            <tr");

WriteLiteral(" data-bind=\"visible: IsRunning\"");

WriteLiteral(">\r\n                <td");

WriteLiteral(" class=\"progress\"");

WriteLiteral(">\r\n                    <div");

WriteLiteral(" data-bind=\"progressValue: Progress\"");

WriteLiteral(">\r\n                    </div>\r\n                </td>\r\n            </tr>\r\n        " +
"    <tr");

WriteLiteral(" data-bind=\"visible: FinishedTimestamp\"");

WriteLiteral(">\r\n                <td");

WriteLiteral(" class=\"finishedTimestamp\"");

WriteLiteral(">\r\n                    <h3>Finished: <span");

WriteLiteral(" data-bind=\"text: FinishedTimestampFormatted\"");

WriteLiteral("></span>\r\n                    </h3>\r\n                </td>\r\n            </tr>\r\n  " +
"          <tr");

WriteLiteral(" data-bind=\"visible: FinishedTimestamp() && !TaskExceptionMessage()\"");

WriteLiteral(">\r\n                <td");

WriteLiteral(" class=\"finishedMessage\"");

WriteLiteral(" data-bind=\"css: { finishedRedirect: FinishedUrl }\"");

WriteLiteral(">\r\n                    <span");

WriteLiteral(" data-bind=\"text: FinishedMessage\"");

WriteLiteral("></span>\r\n                    <i");

WriteLiteral(" class=\"fa fa-lg fa-cog fa-spin\"");

WriteLiteral("></i>\r\n                </td>\r\n            </tr>\r\n            <tr");

WriteLiteral(" data-bind=\"visible: TaskExceptionMessage\"");

WriteLiteral(">\r\n                <td");

WriteLiteral(" class=\"exception\"");

WriteLiteral(">Last Error:\r\n                    <div");

WriteLiteral(" class=\"code\"");

WriteLiteral(" data-bind=\"text: TaskExceptionMessage\"");

WriteLiteral(">\r\n                    </div>\r\n                </td>\r\n            </tr>\r\n        " +
"    <tr");

WriteLiteral(" data-bind=\"visible: NextScheduledTimestamp\"");

WriteLiteral(">\r\n                <td");

WriteLiteral(" class=\"nextScheduledTimestamp\"");

WriteLiteral(">Next Scheduled: <span");

WriteLiteral(" data-bind=\"text: NextScheduledTimestampFormatted\"");

WriteLiteral("></span>\r\n                </td>\r\n            </tr>\r\n        </table>\r\n    </div>\r" +
"\n</div>\r\n<script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(">\r\n    ko.bindingHandlers.progressValue = {\r\n        init: function (element, val" +
"ueAccessor, allBindingsAccessor, viewModel) {\r\n            var $element = $(elem" +
"ent);\r\n            if (!$element.is(\'.ui-progressbar\'))\r\n                $elemen" +
"t.progressbar();\r\n        },\r\n        update: function (element, valueAccessor, " +
"allBindingsAccessor, viewModel) {\r\n            var v = ko.utils.unwrapObservable" +
"(valueAccessor());\r\n            var vInt = parseInt(v);\r\n            if (vInt >=" +
" 0) {\r\n                $(element).progressbar(\'option\', \'value\', vInt);\r\n       " +
"     }\r\n        }\r\n    };\r\n    //* http://webcloud.se/log/JavaScript-and-ISO-860" +
"1/\r\n    Date.prototype.setISO8601 = function (string) {\r\n        var regexp = \"(" +
"[0-9]{4})(-([0-9]{2})(-([0-9]{2})\" +\r\n        \"(T([0-9]{2}):([0-9]{2})(:([0-9]{2" +
"})(\\.([0-9]+))?)?\" +\r\n        \"(Z|(([-+])([0-9]{2}):([0-9]{2})))?)?)?)?\";\r\n     " +
"   var d = string.match(new RegExp(regexp));\r\n\r\n        var offset = 0;\r\n       " +
" var date = new Date(d[1], 0, 1);\r\n\r\n        if (d[3]) { date.setMonth(d[3] - 1)" +
"; }\r\n        if (d[5]) { date.setDate(d[5]); }\r\n        if (d[7]) { date.setHour" +
"s(d[7]); }\r\n        if (d[8]) { date.setMinutes(d[8]); }\r\n        if (d[10]) { d" +
"ate.setSeconds(d[10]); }\r\n        if (d[12]) { date.setMilliseconds(Number(\"0.\" " +
"+ d[12]) * 1000); }\r\n        if (d[14]) {\r\n            offset = (Number(d[16]) *" +
" 60) + Number(d[17]);\r\n            offset *= ((d[15] == \'-\') ? 1 : -1);\r\n       " +
" }\r\n\r\n        offset -= date.getTimezoneOffset();\r\n        time = (Number(date) " +
"+ (offset * 60 * 1000));\r\n        this.setTime(Number(time));\r\n        return th" +
"is;\r\n    }\r\n</script>\r\n<script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(">\r\n    $(function () {\r\n        var sessionId = \'");

            
            #line 93 "..\..\Areas\Config\Views\Shared\TaskStatus.cshtml"
                     Write(Model);

            
            #line default
            #line hidden
WriteLiteral("\';\r\n\r\n        var view = $(\'#Logging_Task_Status\');\r\n        var vm = null;\r\n\r\n  " +
"      var notificationsHub = null;\r\n\r\n        var statusViewModel = function (se" +
"ssionId) {\r\n            var self = this;\r\n\r\n            self.FullUpdateToken = n" +
"ull;\r\n            self.Initialized = ko.observable(false);\r\n\r\n            self.T" +
"imestampParse = function (timestamp) {\r\n                if (timestamp) {\r\n      " +
"              if (timestamp.indexOf(\"\\/Date(\") == 0)\r\n                        re" +
"turn new Date(parseInt(timestamp.substr(6)));\r\n                    else\r\n       " +
"                 return (new Date()).setISO8601(timestamp);\r\n                }\r\n" +
"                return new Date();\r\n            }\r\n            self.TimestampFor" +
"mat = function (timestamp) {\r\n                var addZero = function (v) { v = v" +
" + \'\'; if (v.length == 1) v = \'0\' + v; return v; }\r\n                return times" +
"tamp.getFullYear() + \'/\' + addZero((1 + timestamp.getMonth())) + \'/\' + addZero(t" +
"imestamp.getDate()) + \' \' + addZero(timestamp.getHours()) + \':\' + addZero(timest" +
"amp.getMinutes()) + \':\' + addZero(timestamp.getSeconds());\r\n            }\r\n\r\n   " +
"         self.SessionId = sessionId;\r\n            self.TaskName = ko.observable(" +
"null);\r\n\r\n            self.Progress = ko.observable(0);\r\n            self.Curren" +
"tProcess = ko.observable(null);\r\n            self.CurrentDescription = ko.observ" +
"able(null);\r\n\r\n            self.IsRunning = ko.observable(null);\r\n\r\n            " +
"self.TaskExceptionMessage = ko.observable(null);\r\n\r\n            self.FinishedTim" +
"estamp = ko.observable(null);\r\n            self.NextScheduledTimestamp = ko.obse" +
"rvable(null)\r\n\r\n            self.NextScheduledTimestampFormatted = ko.computed(f" +
"unction () {\r\n                return self.TimestampFormat(self.TimestampParse(se" +
"lf.NextScheduledTimestamp()));\r\n            });\r\n            self.FinishedTimest" +
"ampFormatted = ko.computed(function () {\r\n                return self.TimestampF" +
"ormat(self.TimestampParse(self.FinishedTimestamp()));\r\n            });\r\n\r\n      " +
"      self.FinishedUrl = ko.observable(null);\r\n            self.FinishedMessage " +
"= ko.observable(null);\r\n\r\n            self.Finished = function () {\r\n           " +
"     if (self.FinishedTimestamp()) {\r\n                    if (self.FinishedUrl()" +
" && !self.TaskExceptionMessage()) {\r\n\r\n                        if (self.FullUpda" +
"teToken)\r\n                            window.clearTimeout(self.FullUpdateToken);" +
"\r\n\r\n                        if (self.FinishedMessage())\r\n                       " +
"     window.setTimeout(function () { window.location.href = self.FinishedUrl(); " +
"}, 3000);\r\n                        else\r\n                            window.loca" +
"tion.href = self.FinishedUrl();\r\n                    }\r\n                }\r\n     " +
"       }\r\n\r\n            self.Initialize = function (taskStatus) {\r\n             " +
"   self.TaskName(taskStatus.TaskName);\r\n                self.FinishedUrl(taskSta" +
"tus.FinishedUrl);\r\n\r\n                self.Progress(taskStatus.Progress);\r\n      " +
"          self.CurrentProcess(taskStatus.CurrentProcess);\r\n                self." +
"CurrentDescription(taskStatus.CurrentDescription);\r\n\r\n                self.IsRun" +
"ning(taskStatus.IsRunning);\r\n\r\n                self.TaskExceptionMessage(taskSta" +
"tus.TaskExceptionMessage);\r\n\r\n                self.FinishedTimestamp(taskStatus." +
"FinishedTimestamp);\r\n                self.NextScheduledTimestamp(taskStatus.Next" +
"ScheduledTimestamp);\r\n\r\n                self.FinishedMessage(taskStatus.Finished" +
"Message);\r\n\r\n                self.Initialized(true);\r\n\r\n                self.Fin" +
"ished();\r\n            }\r\n            self.Update = function (taskStatus) {\r\n    " +
"            if (!self.Initialized())\r\n                    return;\r\n\r\n           " +
"     if (self.FullUpdateToken)\r\n                    window.clearTimeout(self.Ful" +
"lUpdateToken);\r\n\r\n                if (taskStatus) {\r\n                    $.each(" +
"taskStatus, function (key, value) {\r\n                        switch (key) {\r\n   " +
"                         case \'Progress\':\r\n                                self." +
"Progress(value);\r\n                                break;\r\n                      " +
"      case \'CurrentProcess\':\r\n                                self.CurrentProces" +
"s(value);\r\n                                break;\r\n                            c" +
"ase \'CurrentDescription\':\r\n                                self.CurrentDescripti" +
"on(value);\r\n                                break;\r\n                            " +
"case \'IsRunning\':\r\n                                self.IsRunning(value);\r\n     " +
"                           break;\r\n                            case \'TaskExcepti" +
"onMessage\':\r\n                                self.TaskExceptionMessage(value);\r\n" +
"                                break;\r\n                            case \'NextSc" +
"heduledTimestamp\':\r\n                                self.NextScheduledTimestamp(" +
"value);\r\n                                break;\r\n                            cas" +
"e \'FinishedUrl\':\r\n                                self.FinishedUrl(value);\r\n    " +
"                            break;\r\n                            case \'FinishedMe" +
"ssage\':\r\n                                self.FinishedMessage(value);\r\n         " +
"                       break;\r\n                            case \'FinishedTimesta" +
"mp\':\r\n                                self.FinishedTimestamp(value);\r\n          " +
"                      window.setTimeout(self.Finished, 1);\r\n                    " +
"            break;\r\n                            default:\r\n                      " +
"          // Ignore\r\n                        }\r\n                    });\r\n       " +
"         }\r\n\r\n                if (!self.FinishedTimestamp())\r\n                  " +
"  self.FullUpdateToken = window.setTimeout(self.FullUpdate, 2000);\r\n            " +
"}\r\n\r\n            self.FullUpdate = function () {\r\n                self.FullUpdat" +
"eToken = null;\r\n\r\n                if (!self.FinishedTimestamp())\r\n              " +
"      notificationsHub.server.getStatus()\r\n                        .done(self.In" +
"itialize);\r\n            }\r\n        }\r\n\r\n        vm = new statusViewModel(session" +
"Id);\r\n        ko.applyBindings(vm, view[0]);\r\n\r\n        // Start Live Connection" +
"\r\n        updateWithLive();\r\n\r\n        function updateWithLive() {\r\n            " +
"notificationsHub = $.connection.scheduledTaskNotifications;\r\n            notific" +
"ationsHub.client.initializeTaskStatus = vm.Initialize;\r\n            notification" +
"sHub.client.updateTaskStatus = vm.Update;\r\n\r\n            $.connection.hub.qs = {" +
" TaskSessionId: sessionId };\r\n            $.connection.hub.error(function (error" +
") {\r\n                console.log(\'Server connection error: \' + error);\r\n        " +
"    });\r\n            $.connection.hub.disconnected(function () {\r\n              " +
"  // Show Dialog Message\r\n                if ($(\'.disconnected-dialog\').length =" +
"= 0) {\r\n                    $(\'<div>\')\r\n                        .addClass(\'dialo" +
"g disconnected-dialog\')\r\n                        .html(\'<h3><span class=\"fa-stac" +
"k fa-lg\"><i class=\"fa fa-wifi fa-stack-1x\"></i><i class=\"fa fa-ban fa-stack-2x e" +
"rror\"></i></span>Disconnected from the Disco ICT Server</h3><div>This page is no" +
"t receiving live updates. Please ensure you are connected to the server, then re" +
"fresh this page to enable features.</div>\')\r\n                        .dialog({\r\n" +
"                            resizable: false,\r\n                            title" +
": \'Disconnected\',\r\n                            width: 400,\r\n                    " +
"        modal: true,\r\n                            buttons: {\r\n                  " +
"              \'Refresh Now\': function () {\r\n                                    " +
"$(this).dialog(\'option\', \'buttons\', null);\r\n                                    " +
"window.location.reload(true);\r\n                                },\r\n             " +
"                   \'Close\': function () {\r\n                                    $" +
"(this).dialog(\'destroy\');\r\n                                }\r\n                  " +
"          }\r\n                        });\r\n                }\r\n            })\r\n\r\n " +
"           $.connection.hub.start()\r\n                .fail(onHubFailed);\r\n      " +
"  }\r\n\r\n    });\r\n</script>\r\n");

        }
    }
}
#pragma warning restore 1591
