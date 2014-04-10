using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.SignalR;
using System.Reactive.Linq;
using Disco.Data.Repository.Monitor;
using Disco.Models.Repository;

namespace Disco.BI.Interop.SignalRHandlers
{
    public class HeldDeviceNotifications : PersistentConnection
    {
        private static bool subscribed = false;
        private static object subscribeLock = new object();
        private static IPersistentConnectionContext notificationContext;

        static HeldDeviceNotifications()
        {
            if (!subscribed)
                lock (subscribeLock)
                    if (!subscribed)
                    {
                        notificationContext = GlobalHost.ConnectionManager.GetConnectionContext<HeldDeviceNotifications>();

                        Disco.Data.Repository.Monitor.RepositoryMonitor.StreamAfterCommit.Where(e => e.EntityType == typeof(Job)).Subscribe(JobUpdated);

                        Disco.Data.Repository.Monitor.RepositoryMonitor.StreamAfterCommit.Where(e =>
                            e.EntityType == typeof(Device) &&
                            (e.ModifiedProperties.Contains("Location") ||
                            e.ModifiedProperties.Contains("DeviceModelId") ||
                            e.ModifiedProperties.Contains("DeviceProfileId") ||
                            e.ModifiedProperties.Contains("DeviceBatchId") ||
                            e.ModifiedProperties.Contains("ComputerName") ||
                            e.ModifiedProperties.Contains("AssignedUserId"))
                            ).Subscribe(DeviceUpdated);

                        Disco.Data.Repository.Monitor.RepositoryMonitor.StreamAfterCommit.Where(e =>
                            e.EntityType == typeof(User) &&
                            e.ModifiedProperties.Contains("DisplayName")
                            ).Subscribe(UserUpdated);

                        subscribed = true;
                    }
        }

        private static void JobUpdated(RepositoryMonitorEvent e)
        {
            Job j = (Job)e.Entity;

            if (j.DeviceSerialNumber != null)
                notificationContext.Connection.Broadcast(j.DeviceSerialNumber);
        }
        private static void DeviceUpdated(RepositoryMonitorEvent e)
        {
            Device d = (Device)e.Entity;

            notificationContext.Connection.Broadcast(d.SerialNumber);
        }
        private static void UserUpdated(RepositoryMonitorEvent e)
        {
            User u = (User)e.Entity;

            var userDevices = e.Database.Devices.Where(d => d.AssignedUserId == u.UserId);

            foreach (var userDevice in userDevices)
            {
                notificationContext.Connection.Broadcast(userDevice.SerialNumber);
            }
        }
    }
}
