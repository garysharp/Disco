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

namespace Disco.Web.Areas.Public.Views.HeldDevices
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
    using Disco.Models.Repository;
    using Disco.Services;
    using Disco.Services.Authorization;
    using Disco.Services.Web;
    using Disco.Web;
    using Disco.Web.Extensions;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Areas/Public/Views/HeldDevices/Noticeboard.cshtml")]
    public partial class Noticeboard : Disco.Services.Web.WebViewPage<Disco.Web.Areas.Public.Models.UserHeldDevices.NoticeboardModel>
    {
        public Noticeboard()
        {
        }
        public override void Execute()
        {
            
            #line 2 "..\..\Areas\Public\Views\HeldDevices\Noticeboard.cshtml"
  
    Layout = null;
    Html.BundleDeferred("~/ClientScripts/Modules/Knockout");
    Html.BundleDeferred("~/ClientScripts/Modules/jQuery-SignalR");
    Html.BundleDeferred("~/ClientScripts/Core");
    Html.BundleDeferred("~/Style/Public/HeldDevicesNoticeboard");

            
            #line default
            #line hidden
WriteLiteral("\r\n<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <meta");

WriteLiteral(" charset=\"utf-8\"");

WriteLiteral(" />\r\n    <meta");

WriteLiteral(" http-equiv=\"X-UA-Compatible\"");

WriteLiteral(" content=\"IE=edge\"");

WriteLiteral(" />\r\n    <title>Disco ICT - Held Devices</title>\r\n");

WriteLiteral("    ");

            
            #line 15 "..\..\Areas\Public\Views\HeldDevices\Noticeboard.cshtml"
Write(Html.BundleRenderDeferred());

            
            #line default
            #line hidden
WriteLiteral("\r\n</head>\r\n<body");

WriteAttribute("class", Tuple.Create(" class=\"", 562), Tuple.Create("\"", 615)
, Tuple.Create(Tuple.Create("", 570), Tuple.Create("theme-", 570), true)
            
            #line 17 "..\..\Areas\Public\Views\HeldDevices\Noticeboard.cshtml"
, Tuple.Create(Tuple.Create("", 576), Tuple.Create<System.Object, System.Int32>(Model.DefaultTheme
            
            #line default
            #line hidden
, 576), false)
, Tuple.Create(Tuple.Create(" ", 597), Tuple.Create("status-connecting", 598), true)
);

WriteLiteral(">\r\n    <div");

WriteLiteral(" id=\"page\"");

WriteLiteral(">\r\n        <header");

WriteLiteral(" id=\"header\"");

WriteLiteral(">\r\n            <div");

WriteLiteral(" id=\"heading\"");

WriteLiteral(">Held Devices</div>\r\n            <div");

WriteLiteral(" id=\"statusConnecting\"");

WriteLiteral("><i");

WriteLiteral(" class=\"fa fa-cog fa-spin\"");

WriteLiteral("></i><span>connecting...</span></div>\r\n            <div");

WriteLiteral(" id=\"statusError\"");

WriteLiteral("><i");

WriteLiteral(" class=\"fa fa-cog fa-spin\"");

WriteLiteral("></i><span>disconnected, reconnecting...</span></div>\r\n            <div");

WriteLiteral(" id=\"credits\"");

WriteLiteral(">\r\n                powered by Disco ICT <i");

WriteLiteral(" title=\"Disco ICT - Jobs\"");

WriteLiteral("></i>\r\n            </div>\r\n        </header>\r\n        <section");

WriteLiteral(" id=\"mainSection\"");

WriteLiteral(">\r\n            <div");

WriteLiteral(" id=\"inProcess\"");

WriteLiteral(" class=\"list\"");

WriteLiteral(">\r\n                <h3>In Process (<span");

WriteLiteral(" data-bind=\"text: inProcess().length\"");

WriteLiteral("></span>)\r\n                </h3>\r\n                <div");

WriteLiteral(" class=\"content\"");

WriteLiteral(">\r\n                    <!-- ko if: inProcess().length == 0 -->\r\n                 " +
"   <div");

WriteLiteral(" class=\"noContent\"");

WriteLiteral(">&lt;None&gt;</div>\r\n                    <!-- /ko -->\r\n                    <ul");

WriteLiteral(" data-bind=\"template: { name: \'item-template\', foreach: inProcess, afterRender: o" +
"nAdd, beforeRemove: onRemove }\"");

WriteLiteral("></ul>\r\n                </div>\r\n            </div>\r\n            <div");

WriteLiteral(" id=\"readyForReturn\"");

WriteLiteral(" class=\"list\"");

WriteLiteral(">\r\n                <h3>Ready for Return (<span");

WriteLiteral(" data-bind=\"text: readyForReturn().length\"");

WriteLiteral("></span>)\r\n                </h3>\r\n                <div");

WriteLiteral(" class=\"content\"");

WriteLiteral(">\r\n                    <!-- ko if: readyForReturn().length == 0 -->\r\n            " +
"        <div");

WriteLiteral(" class=\"noContent\"");

WriteLiteral(">&lt;None&gt;</div>\r\n                    <!-- /ko -->\r\n                    <ul");

WriteLiteral(" data-bind=\"template: { name: \'item-template\', foreach: readyForReturn, afterRend" +
"er: onAdd, beforeRemove: onRemove }\"");

WriteLiteral("></ul>\r\n                </div>\r\n            </div>\r\n            <div");

WriteLiteral(" id=\"waitingForUserAction\"");

WriteLiteral(" class=\"list\"");

WriteLiteral(">\r\n                <h3>Waiting for User Action (<span");

WriteLiteral(" data-bind=\"text: waitingForUserAction().length\"");

WriteLiteral("></span>)\r\n                </h3>\r\n                <div");

WriteLiteral(" class=\"content\"");

WriteLiteral(">\r\n                    <!-- ko if: waitingForUserAction().length == 0 -->\r\n      " +
"              <div");

WriteLiteral(" class=\"noContent\"");

WriteLiteral(">&lt;None&gt;</div>\r\n                    <!-- /ko -->\r\n                    <ul");

WriteLiteral(" data-bind=\"template: { name: \'item-template\', foreach: waitingForUserAction, aft" +
"erAdd: onAdd, beforeRemove: onRemove }\"");

WriteLiteral("></ul>\r\n                </div>\r\n            </div>\r\n            <footer");

WriteLiteral(" id=\"footer\"");

WriteLiteral(">\r\n            </footer>\r\n        </section>\r\n    </div>\r\n    <script");

WriteLiteral(" type=\"text/html\"");

WriteLiteral(" id=\"item-template\"");

WriteLiteral(">\r\n        <li data-bind=\"css: { alert: IsAlert }\">\r\n            <span data-bind=" +
"\"text: DeviceDescription\"></span>\r\n            <!-- ko if: !ReadyForReturn && Es" +
"timatedReturnTimeUnixEpoc -->\r\n            <span class=\"small\">(Expected <span d" +
"ata-bind=\"livestamp: EstimatedReturnTimeUnixEpoc\"></span>)</span>\r\n            <" +
"!-- /ko -->\r\n            <!-- ko if: WaitingForUserAction -->\r\n            <span" +
" class=\"small\">(Since <span data-bind=\"livestamp: WaitingForUserActionSinceUnixE" +
"poc\"></span>)</span>\r\n            <!-- /ko -->\r\n            <!-- ko if: ReadyFor" +
"Return && !WaitingForUserAction -->\r\n            <span class=\"small\">(Ready <spa" +
"n data-bind=\"livestamp: ReadyForReturnSinceUnixEpoc\"></span>)</span>\r\n          " +
"  <!-- /ko -->\r\n        </li>\r\n    </script>\r\n    <script>\r\n        ko.bindingHa" +
"ndlers.livestamp = {\r\n            init: function (element, valueAccessor, allBin" +
"dings, viewModel, bindingContext) {\r\n                var value = valueAccessor()" +
";\r\n                var valueUnwrapped = ko.unwrap(value);\r\n\r\n                if " +
"(valueUnwrapped)\r\n                    $(element).livestamp(valueUnwrapped);\r\n   " +
"             else\r\n                    $(element).livestamp(\'destroy\');\r\n       " +
"     }\r\n        };\r\n    </script>\r\n    <script>\r\n        $(function () {\r\n      " +
"      var hub;\r\n            var viewModel;\r\n\r\n            var rotateSpeed = 3000" +
";\r\n            var itemFilters;\r\n            var fixedTheme = null;\r\n\r\n         " +
"   var $inProcessList = $(\'#inProcess\').find(\'ul\');\r\n            var $readyForRe" +
"turnList = $(\'#readyForReturn\').find(\'ul\');\r\n            var $waitingForUserActi" +
"onList = $(\'#waitingForUserAction\').find(\'ul\');\r\n\r\n            function noticebo" +
"ardViewModel(inProcess, readyForReturn, waitingForUserAction) {\r\n               " +
" var self = this;\r\n\r\n                self.initialized = false;\r\n\r\n              " +
"  self.inProcess = ko.observableArray(inProcess);\r\n                self.readyFor" +
"Return = ko.observableArray(readyForReturn);\r\n                self.waitingForUse" +
"rAction = ko.observableArray(waitingForUserAction);\r\n\r\n                self.onRe" +
"move = function (element, index, data) {\r\n                    $(element).slideUp" +
"(400, function () {\r\n                        $(this).remove();\r\n                " +
"    });\r\n                }\r\n                self.onAdd = function (element, inde" +
"x, data) {\r\n                    if (self.initialized)\r\n                        $" +
"(element).hide().slideDown(400);\r\n                }\r\n            }\r\n\r\n          " +
"  function init() {\r\n                monitorMouseMove();\r\n                applyQ" +
"ueryString();\r\n\r\n                // Connect to Hub\r\n                hub = $.conn" +
"ection.noticeboardUpdates;\r\n\r\n                // Map Functions\r\n                " +
"hub.client.updateHeldDevice = updateHeldDevice;\r\n                hub.client.setT" +
"heme = setTheme;\r\n\r\n                $.connection.hub.qs = { Noticeboard: \'");

            
            #line 133 "..\..\Areas\Public\Views\HeldDevices\Noticeboard.cshtml"
                                                  Write(Disco.Services.Jobs.Noticeboards.HeldDevices.Name);

            
            #line default
            #line hidden
WriteLiteral(@"' };
                $.connection.hub.error(function (error) {
                    console.log('Server connection error: ' + error);
                });
                $.connection.hub.disconnected(connectionError);

                // Start Connection
                $.connection.hub.start().fail(connectionError).done(loadData);
            }

            // Called after SignalR is connected
            function loadData() {
                $.getJSON('");

            
            #line 145 "..\..\Areas\Public\Views\HeldDevices\Noticeboard.cshtml"
                       Write(Url.Action(MVC.Public.HeldDevices.HeldDevices()));

            
            #line default
            #line hidden
WriteLiteral("\', null, function (data) {\r\n\r\n                    var inProcess = [];\r\n          " +
"          var readyForReturn = [];\r\n                    var waitingForUserAction" +
" = [];\r\n\r\n                    data.filter(function (heldDeviceItem) {\r\n         " +
"               return includeItem(heldDeviceItem)\r\n                    }).forEac" +
"h(function (heldDeviceItem) {\r\n                        if (isWaitingForUserActio" +
"n(heldDeviceItem))\r\n                            waitingForUserAction.push(heldDe" +
"viceItem);\r\n                        else if (isReadyForReturn(heldDeviceItem))\r\n" +
"                            readyForReturn.push(heldDeviceItem);\r\n              " +
"          else if (isInProcess(heldDeviceItem))\r\n                            inP" +
"rocess.push(heldDeviceItem);\r\n                    });\r\n\r\n                    inP" +
"rocess.sort(sortFunction);\r\n                    readyForReturn.sort(sortFunction" +
");\r\n                    waitingForUserAction.sort(sortFunction);\r\n\r\n            " +
"        viewModel = new noticeboardViewModel(inProcess, readyForReturn, waitingF" +
"orUserAction);\r\n\r\n                    ko.applyBindings(viewModel);\r\n            " +
"        viewModel.initialized = true;\r\n\r\n                    $(\'body\').removeCla" +
"ss(\'status-connecting\');\r\n\r\n                    window.setTimeout(scheduleRotati" +
"on, rotateSpeed);\r\n                });\r\n            }\r\n\r\n            // Called b" +
"y SignalR\r\n            function updateHeldDevice(updates) {\r\n                if " +
"(viewModel) {\r\n\r\n                    $.each(updates, function (deviceSerialNumbe" +
"r, heldDeviceItem) {\r\n                        // Remove Existing\r\n              " +
"          removeItem(deviceSerialNumber);\r\n\r\n                        // Add Item" +
"\r\n                        addItem(heldDeviceItem);\r\n                    });\r\n   " +
"             }\r\n            }\r\n\r\n            function removeItem(deviceSerialNum" +
"ber) {\r\n                removeItemFromArray(viewModel.inProcess, deviceSerialNum" +
"ber);\r\n                removeItemFromArray(viewModel.readyForReturn, deviceSeria" +
"lNumber);\r\n                removeItemFromArray(viewModel.waitingForUserAction, d" +
"eviceSerialNumber);\r\n            }\r\n\r\n            function addItem(heldDeviceIte" +
"m) {\r\n                if (heldDeviceItem !== null &&\r\n                    heldDe" +
"viceItem !== undefined &&\r\n                    includeItem(heldDeviceItem)) {\r\n\r" +
"\n                    var array;\r\n\r\n                    if (isWaitingForUserActio" +
"n(heldDeviceItem))\r\n                        array = viewModel.waitingForUserActi" +
"on;\r\n                    else if (isReadyForReturn(heldDeviceItem))\r\n           " +
"             array = viewModel.readyForReturn;\r\n                    else if (isI" +
"nProcess(heldDeviceItem))\r\n                        array = viewModel.inProcess;\r" +
"\n\r\n                    if (array().length === 0) {\r\n                        arra" +
"y.push(heldDeviceItem);\r\n                    } else {\r\n                        v" +
"ar index = findSortedInsertIndex(array, heldDeviceItem);\r\n                      " +
"  if (index === -1)\r\n                            array.push(heldDeviceItem);\r\n  " +
"                      else\r\n                            array.splice(index, 0, h" +
"eldDeviceItem);\r\n                    }\r\n                }\r\n            }\r\n\r\n    " +
"        function rotateArrays() {\r\n                rotateArray(viewModel.inProce" +
"ss, $inProcessList);\r\n                rotateArray(viewModel.readyForReturn, $rea" +
"dyForReturnList);\r\n                rotateArray(viewModel.waitingForUserAction, $" +
"waitingForUserActionList);\r\n            }\r\n\r\n            function scheduleRotati" +
"on() {\r\n                rotateArrays();\r\n\r\n                window.setTimeout(sch" +
"eduleRotation, rotateSpeed);\r\n            }\r\n\r\n            function includeItem(" +
"heldDeviceItem) {\r\n                if (itemFilters == null || itemFilters.length" +
" == 0)\r\n                    return true;\r\n\r\n                return itemFilters.r" +
"educe(function (previousValue, currentValue, index, array) {\r\n                  " +
"  if (previousValue === false)\r\n                        return false;\r\n         " +
"           return currentValue(heldDeviceItem);\r\n                }, true);\r\n    " +
"        }\r\n\r\n            function setTheme(theme) {\r\n                if (!!fixed" +
"Theme)\r\n                    return;\r\n\r\n                var $body = $(document.bo" +
"dy);\r\n\r\n                // Existing classes\r\n                var c = $body.attr(" +
"\'class\').split(\' \');\r\n                // Remove existing theme\r\n                " +
"c = $.grep(c, function (i) { return (i.indexOf(\'theme-\') !== 0) });\r\n\r\n         " +
"       c.push(\'theme-\' + theme);\r\n\r\n                $body.attr(\'class\', c.join(\'" +
" \'));\r\n            }\r\n\r\n            function monitorMouseMove() {\r\n             " +
"   var token = null,\r\n                    $body = $(document.body);\r\n\r\n         " +
"       $body.mousemove(function () {\r\n                    if (!!token)\r\n        " +
"                window.clearTimeout(token);\r\n                    else if ($body." +
"css(\'cursor\') == \'none\')\r\n                        $body.css(\'cursor\', \'auto\');\r\n" +
"\r\n                    token = window.setTimeout(function () {\r\n                 " +
"       $body.css(\'cursor\', \'none\');\r\n                        token = null;\r\n    " +
"                }, 3500);\r\n                });\r\n\r\n            }\r\n\r\n            f" +
"unction applyQueryString() {\r\n                var queryStringParameters = getQue" +
"ryStringParameters();\r\n\r\n                if (queryStringParameters !== null) {\r\n" +
"                    var filters = [];\r\n\r\n                    $.each(queryStringP" +
"arameters, function (key, value) {\r\n                        switch (key.toLowerC" +
"ase()) {\r\n                            case \'components\':\r\n                      " +
"          const showComponents = value.split(\",\");\r\n                            " +
"    if (showComponents.length > 0) {\r\n                                    const " +
"components = [\'inProcess\', \'readyForReturn\', \'waitingForUserAction\'];\r\n         " +
"                           components.forEach(function (component) {\r\n          " +
"                              if (!showComponents.includes(component)) {\r\n      " +
"                                      $(\'body\').addClass(\'hide-\' + component);\r\n" +
"                                        }\r\n                                    }" +
");\r\n                                }\r\n                                break;\r\n " +
"                           case \'theme\': // THEME\r\n                             " +
"   setTheme(value);\r\n                                fixedTheme = value;\r\n      " +
"                          break;\r\n                            case \'deviceaddres" +
"sinclude\': // FILTER: Device Address Include\r\n                                va" +
"r deviceAddresses = value.split(\",\").map(function (v) { return v.toLowerCase(); " +
"});\r\n                                if (deviceAddresses.length > 0) {\r\n        " +
"                            filters.push(function (heldDeviceItem) {\r\n          " +
"                              // false if DeviceAddressShortName is null\r\n      " +
"                                  if (!heldDeviceItem.DeviceAddressShortName)\r\n " +
"                                           return false;\r\n\r\n                    " +
"                    // true if DeviceAddressShortName is included\r\n             " +
"                           return $.inArray(heldDeviceItem.DeviceAddressShortNam" +
"e.toLowerCase(), deviceAddresses) >= 0;\r\n                                    });" +
"\r\n                                }\r\n                                break;\r\n   " +
"                         case \'deviceaddressexclude\': // FILTER: Device Address " +
"Exclude\r\n                                var deviceAddresses = value.split(\",\")." +
"map(function (v) { return v.toLowerCase(); });\r\n                                " +
"if (deviceAddresses.length > 0) {\r\n                                    filters.p" +
"ush(function (heldDeviceItem) {\r\n                                        // true" +
" if DeviceAddressShortName is null\r\n                                        if (" +
"!heldDeviceItem.DeviceAddressShortName)\r\n                                       " +
"     return true;\r\n\r\n                                        // true if DeviceAd" +
"dressShortName is excluded\r\n                                        return $.inA" +
"rray(heldDeviceItem.DeviceAddressShortName.toLowerCase(), deviceAddresses) < 0;\r" +
"\n                                    });\r\n                                }\r\n   " +
"                             break;\r\n                            case \'devicepro" +
"fileinclude\': // FILTER: Device Profile Include\r\n                               " +
" var deviceProfiles = value.split(\",\").map(function (v) { return parseInt(v); })" +
";\r\n                                if (deviceProfiles.length > 0) {\r\n           " +
"                         filters.push(function (heldDeviceItem) {\r\n             " +
"                           // true if DeviceProfileId is included\r\n             " +
"                           return $.inArray(heldDeviceItem.DeviceProfileId, devi" +
"ceProfiles) >= 0;\r\n                                    });\r\n                    " +
"            }\r\n                                break;\r\n                         " +
"   case \'deviceprofileexclude\': // FILTER: Device Profile Exclude\r\n             " +
"                   var deviceProfiles = value.split(\",\").map(function (v) { retu" +
"rn parseInt(v); });\r\n                                if (deviceProfiles.length >" +
" 0) {\r\n                                    filters.push(function (heldDeviceItem" +
") {\r\n                                        // true if DeviceProfileId is exclu" +
"ded\r\n                                        return $.inArray(heldDeviceItem.Dev" +
"iceProfileId, deviceProfiles) < 0;\r\n                                    });\r\n   " +
"                             }\r\n                                break;\r\n        " +
"                }\r\n                    });\r\n\r\n                    if (filters.le" +
"ngth > 0)\r\n                        itemFilters = filters;\r\n                    e" +
"lse\r\n                        itemFilters = null;\r\n                }\r\n           " +
" }\r\n\r\n            function connectionError() {\r\n                try {\r\n         " +
"           $(\'body\').addClass(\'status-error\');\r\n                } catch (e) {\r\n " +
"                   // Ignore\r\n                }\r\n\r\n                window.setTim" +
"eout(function () {\r\n                    window.location.reload(true);\r\n         " +
"       }, 10000);\r\n            }\r\n\r\n            // Helpers\r\n            function" +
" rotateArray(koArray, element) {\r\n                var items = koArray();\r\n\r\n    " +
"            if (items.length <= 1)\r\n                    return 0;\r\n\r\n           " +
"     if (element.height() < (element.parent().height() - 30)) {\r\n\r\n             " +
"       if (findUnsortedArrayTopIndex(items) !== 0)\r\n                        koAr" +
"ray.sort(sortFunction);\r\n\r\n                    // Don\'t rotate if small & sorted" +
" correctly\r\n                    return;\r\n                }\r\n\r\n                //" +
" Move Last Item to Top\r\n                var item = koArray.pop();\r\n             " +
"   koArray.unshift(item);\r\n            }\r\n            function removeItemFromArr" +
"ay(koArray, deviceSerialNumber) {\r\n                var items = koArray();\r\n     " +
"           for (var i = 0; i < items.length; i++) {\r\n                    if (ite" +
"ms[i].DeviceSerialNumber == deviceSerialNumber) {\r\n                        koArr" +
"ay.splice(i, 1);\r\n                        items = koArray();\r\n                  " +
"      i--;\r\n                    }\r\n                }\r\n            }\r\n           " +
" function findUnsortedArrayTopIndex(items) {\r\n                // Only one Item\r\n" +
"                if (items.length <= 1)\r\n                    return 0;\r\n\r\n       " +
"         for (var i = 1; i < items.length; i++) {\r\n                    var s = s" +
"ortFunction(items[i - 1], items[i]);\r\n                    if (s > 0)\r\n          " +
"              return i;\r\n                }\r\n\r\n                return 0;\r\n       " +
"     }\r\n            function findSortedInsertIndex(koArray, heldDeviceItem) {\r\n " +
"               var items = koArray();\r\n                var startIndex = findUnso" +
"rtedArrayTopIndex(items);\r\n                for (var i = startIndex; i < items.le" +
"ngth; i++) {\r\n                    var s = sortFunction(heldDeviceItem, items[i])" +
";\r\n                    if (s <= 0)\r\n                        return i;\r\n         " +
"       }\r\n                if (startIndex !== 0) {\r\n                    for (var " +
"i = 0; i < startIndex; i++) {\r\n                        var s = sortFunction(held" +
"DeviceItem, items[i]);\r\n                        if (s <= 0)\r\n                   " +
"         return i;\r\n                    }\r\n                    return startIndex" +
";\r\n                } else {\r\n                    return -1;\r\n                }\r\n" +
"            }\r\n            function sortFunction(l, r) {\r\n                return" +
" l.DeviceDescription.toLowerCase() == r.DeviceDescription.toLowerCase() ? 0 : (l" +
".DeviceDescription.toLowerCase() < r.DeviceDescription.toLowerCase() ? -1 : 1)\r\n" +
"            }\r\n            function isInProcess(i) {\r\n                return !i." +
"ReadyForReturn && !i.WaitingForUserAction;\r\n            }\r\n            function " +
"isReadyForReturn(i) {\r\n                return i.ReadyForReturn && !i.WaitingForU" +
"serAction;\r\n            }\r\n            function isWaitingForUserAction(i) {\r\n   " +
"             return i.WaitingForUserAction;\r\n            }\r\n            function" +
" getQueryStringParameters() {\r\n\r\n                if (window.location.search.leng" +
"th === 0)\r\n                    return null;\r\n\r\n                var params = {};\r" +
"\n                window.location.search.substr(1).split(\"&\").forEach(function (p" +
"air) {\r\n                    if (pair === \"\") return;\r\n                    var pa" +
"rts = pair.split(\"=\");\r\n                    params[parts[0]] = parts[1] && decod" +
"eURIComponent(parts[1].replace(/\\+/g, \" \"));\r\n                });\r\n             " +
"   return params;\r\n            }\r\n\r\n            init();\r\n        });\r\n    </scri" +
"pt>\r\n</body>\r\n</html>");

        }
    }
}
#pragma warning restore 1591
