using Disco.Models.Services.Interop.DiscoServices;
using Disco.Services.Logging;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Disco.Services.Interop.DiscoServices
{
    public static class OnlineServicesConnect
    {

        private static readonly HubConnection connection;

        public static ConnectionState State
        {
            get
            {
                switch (connection.State)
                {
                    case HubConnectionState.Connected:
                        return ConnectionState.Connected;
                    case HubConnectionState.Connecting:
                        return ConnectionState.Connecting;
                    case HubConnectionState.Reconnecting:
                        return ConnectionState.Reconnecting;
                    default:
                        return ConnectionState.Disconnected;
                }
            }
        }

        static OnlineServicesConnect()
        {
            connection = new HubConnectionBuilder()
                .WithUrl(new Uri(DiscoServiceHelpers.ActivationServiceUrl, "/connect"), options =>
                {
                    options.AccessTokenProvider = () => OnlineServicesAuthentication.GetTokenAsync();
                })
                .WithAutomaticReconnect()
                .WithStatefulReconnect()
                .Build();

            connection.Closed += ex =>
            {
                if (ex != null)
                    SystemLog.LogException("Online Services: Connection Closed", ex);
                else
                    SystemLog.LogInformation("Online Services: Connection Closed");
                return Task.CompletedTask;
            };
            connection.Reconnected += connectionId =>
            {
                SystemLog.LogInformation("Online Services: Connection Reconnected");
                return Task.CompletedTask;
            };
            connection.Reconnecting += ex =>
            {
                if (ex != null)
                    SystemLog.LogException("Online Services: Connection Reconnecting", ex);
                else
                    SystemLog.LogInformation("Online Services: Connection Reconnecting");

                return Task.CompletedTask;
            };
        }

        public static async Task StartAsync()
        {
            try
            {
                if (connection.State == HubConnectionState.Disconnected)
                    await connection.StartAsync();
            }
            catch (Exception ex)
            {
                SystemLog.LogException("Online Services", ex);
            }
        }

        public static void QueueStart()
        {
            if (connection.State == HubConnectionState.Disconnected)
                ThreadPool.QueueUserWorkItem(async _ => await StartAsync());
        }

        public static async Task StopAsync()
        {
            await connection.StopAsync();
        }

        public static IDisposable SubscribeToNotifications(Action<IConnectNotification> handler, params int[] notificationTypes)
        {
            return new NotificationSubscription(handler, notificationTypes);
        }

        private class NotificationSubscription : IDisposable
        {
            private readonly IDisposable subscription;
            private readonly Action<IConnectNotification> handler;
            private readonly int[] notificationTypes;

            public NotificationSubscription(Action<IConnectNotification> handler, params int[] notificationTypes)
            {
                if (notificationTypes == null || notificationTypes.Length == 0)
                {
                    handler = null;
                    subscription = null;
                    notificationTypes = null;
                }
                else
                {
                    this.handler = handler;
                    this.notificationTypes = notificationTypes;
                    subscription = connection.On<ConnectNotification>("Notify", Handler);
                }
            }

            public void Handler(ConnectNotification notification)
            {
                if (Array.IndexOf(notificationTypes, notification.Type) >= 0)
                    handler(notification);
            }

            public void Dispose()
            {
                subscription?.Dispose();
            }
        }

        private class ConnectNotification : IConnectNotification
        {
            public int Version { get; set; }
            public int Type { get; set; }
            public string Content { get; set; }
        }

        public enum ConnectionState
        {
            Disconnected,
            Connected,
            Connecting,
            Reconnecting
        }
    }
}
