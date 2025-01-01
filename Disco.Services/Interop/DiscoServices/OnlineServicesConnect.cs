using Disco.Models.Services.Interop.DiscoServices;
using Disco.Services.Logging;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace Disco.Services.Interop.DiscoServices
{
    public static class OnlineServicesConnect
    {

        private static readonly HubConnection connection;

        public static string State => connection.State.ToString();

        static OnlineServicesConnect()
        {
            connection = new HubConnectionBuilder()
                .WithUrl(new Uri(ActivationService.BaseUrl, "/connect"), options =>
                {
                    options.AccessTokenProvider = () => OnlineServicesAuthentication.GetTokenAsync();
                })
                .WithAutomaticReconnect()
                .WithStatefulReconnect()
                .Build();
        }

        public static async Task StartAsync()
        {
            try
            {
                await connection.StartAsync();
            }
            catch (Exception ex)
            {
                SystemLog.LogException("Online Services", ex);
            }
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

    }
}
