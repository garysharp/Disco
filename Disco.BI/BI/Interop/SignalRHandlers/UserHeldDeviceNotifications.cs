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
    public class UserHeldDeviceNotifications : PersistentConnection
    {
        private static bool subscribed = false;
        private static object subscribeLock = new object();
        private static IPersistentConnectionContext notificationContext;

        static UserHeldDeviceNotifications()
        {
            if (!subscribed)
                lock (subscribeLock)
                    if (!subscribed)
                    {
                        notificationContext = GlobalHost.ConnectionManager.GetConnectionContext<UserHeldDeviceNotifications>();

                        Disco.Data.Repository.Monitor.RepositoryMonitor.StreamAfterCommit.Where(e => e.EntityType == typeof(Job)).Subscribe(JobUpdated);

                        Disco.Data.Repository.Monitor.RepositoryMonitor.StreamBeforeCommit.Where(e =>
                            e.EntityType == typeof(Device) &&
                            (e.ModifiedProperties.Contains("DeviceModelId") ||
                            e.ModifiedProperties.Contains("DeviceProfileId") ||
                            e.ModifiedProperties.Contains("DeviceBatchId") ||
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
            {
                var jobDevice = e.Database.Devices.Where(d => d.SerialNumber == j.DeviceSerialNumber).FirstOrDefault();

                if (jobDevice.AssignedUserId != null)
                    notificationContext.Connection.Broadcast(jobDevice.AssignedUserId);
            }
        }
        private static void DeviceUpdated(RepositoryMonitorEvent e)
        {
            Device d = (Device)e.Entity;

            string previouslyAssignedUserId = null;

            if (e.ModifiedProperties.Contains("AssignedUserId"))
                previouslyAssignedUserId = e.GetPreviousPropertyValue<string>("AssignedUserId");

            e.ExecuteAfterCommit(me =>
            {
                if (previouslyAssignedUserId != null)
                    notificationContext.Connection.Broadcast(previouslyAssignedUserId);

                if (d.AssignedUserId != null)
                    notificationContext.Connection.Broadcast(d.AssignedUserId);
            });
        }
        private static void UserUpdated(RepositoryMonitorEvent e)
        {
            User u = (User)e.Entity;

            notificationContext.Connection.Broadcast(u.Id);
        }
    }
}
