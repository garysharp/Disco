﻿/*!
 * ASP.NET SignalR JavaScript Library v2.1.1
 * http://signalr.net/
 *
 * Copyright Microsoft Open Technologies, Inc. All rights reserved.
 * Licensed under the Apache 2.0
 * https://github.com/SignalR/SignalR/blob/master/LICENSE.md
 *
 */

/// <reference path="..\..\Core\jquery-2.1.1.js" />
"use strict";

(function ($, window, undefined) {
    /// <param name="$" type="jQuery" />
    "use strict";

    if (typeof $.signalR !== "function") {
        throw new Error("SignalR: SignalR is not loaded. Please ensure jquery.signalR-x.js is referenced before ~/signalr/js.");
    }

    var signalR = $.signalR;

    function makeProxyCallback(hub, callback) {
        return function () {
            // Call the client hub method
            callback.apply(hub, $.makeArray(arguments));
        };
    }

    function registerHubProxies(instance, shouldSubscribe) {
        var key, hub, memberKey, memberValue, subscriptionMethod;

        for (key in instance) {
            if (instance.hasOwnProperty(key)) {
                hub = instance[key];

                if (!hub.hubName) {
                    // Not a client hub
                    continue;
                }

                if (shouldSubscribe) {
                    // We want to subscribe to the hub events
                    subscriptionMethod = hub.on;
                } else {
                    // We want to unsubscribe from the hub events
                    subscriptionMethod = hub.off;
                }

                // Loop through all members on the hub and find client hub functions to subscribe/unsubscribe
                for (memberKey in hub.client) {
                    if (hub.client.hasOwnProperty(memberKey)) {
                        memberValue = hub.client[memberKey];

                        if (!$.isFunction(memberValue)) {
                            // Not a client hub function
                            continue;
                        }

                        subscriptionMethod.call(hub, memberKey, makeProxyCallback(hub, memberValue));
                    }
                }
            }
        }
    }

    $.hubConnection.prototype.createHubProxies = function () {
        var proxies = {};
        this.starting(function () {
            // Register the hub proxies as subscribed
            // (instance, shouldSubscribe)
            registerHubProxies(proxies, true);

            this._registerSubscribedHubs();
        }).disconnected(function () {
            // Unsubscribe all hub proxies when we "disconnect".  This is to ensure that we do not re-add functional call backs.
            // (instance, shouldSubscribe)
            registerHubProxies(proxies, false);
        });

        proxies['deviceBatchUpdates'] = this.createHubProxy('deviceBatchUpdates');
        proxies['deviceBatchUpdates'].client = {};
        proxies['deviceBatchUpdates'].server = {};

        proxies['deviceUpdates'] = this.createHubProxy('deviceUpdates');
        proxies['deviceUpdates'].client = {};
        proxies['deviceUpdates'].server = {};

        proxies['jobUpdates'] = this.createHubProxy('jobUpdates');
        proxies['jobUpdates'].client = {};
        proxies['jobUpdates'].server = {};

        proxies['logNotifications'] = this.createHubProxy('logNotifications');
        proxies['logNotifications'].client = {};
        proxies['logNotifications'].server = {};

        proxies['noticeboardUpdates'] = this.createHubProxy('noticeboardUpdates');
        proxies['noticeboardUpdates'].client = {};
        proxies['noticeboardUpdates'].server = {};

        proxies['scheduledTaskNotifications'] = this.createHubProxy('scheduledTaskNotifications');
        proxies['scheduledTaskNotifications'].client = {};
        proxies['scheduledTaskNotifications'].server = {
            getStatus: function getStatus() {
                /// <summary>Calls the GetStatus method on the server-side scheduledTaskNotifications hub.&#10;Returns a jQuery.Deferred() promise.</summary>
                return proxies['scheduledTaskNotifications'].invoke.apply(proxies['scheduledTaskNotifications'], $.merge(["GetStatus"], $.makeArray(arguments)));
            }
        };

        proxies['userUpdates'] = this.createHubProxy('userUpdates');
        proxies['userUpdates'].client = {};
        proxies['userUpdates'].server = {};

        return proxies;
    };

    signalR.hub = $.hubConnection("/API/Signalling", { useDefaultPath: false });
    $.extend(signalR, signalR.hub.createHubProxies());
})(window.jQuery, window);
/// <reference path="jquery.signalR-2.1.1.js" />

