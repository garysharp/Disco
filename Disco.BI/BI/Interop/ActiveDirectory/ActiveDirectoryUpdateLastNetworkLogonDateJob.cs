using Disco.Data.Repository;
using Disco.Services.Logging;
using Disco.Models.Repository;
using Quartz;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Reflection;
using Disco.Services.Tasks;
namespace Disco.BI.Interop.ActiveDirectory
{
    public class ActiveDirectoryUpdateLastNetworkLogonDateJob : ScheduledTask
    {

        public override string TaskName { get { return "Active Directory - Update Last Network Logon Dates Task"; } }
        public override bool SingleInstanceTask { get { return true; } }
        public override bool CancelInitiallySupported { get { return false; } }

        public override void InitalizeScheduledTask(DiscoDataContext dbContext)
        {
            // ActiveDirectoryUpdateLastNetworkLogonDateJob @ 11:30pm
            TriggerBuilder triggerBuilder = TriggerBuilder.Create().
                WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(23, 30));

            this.ScheduleTask(triggerBuilder);
        }

        protected override void ExecuteTask()
        {
            int changeCount;

            this.Status.UpdateStatus(1, "Starting", "Connecting to the Database and initializing the environment");
            using (DiscoDataContext dbContext = new DiscoDataContext())
            {
                UpdateLastNetworkLogonDates(dbContext, this.Status);
                this.Status.UpdateStatus(95, "Updating Database", "Writing last network logon dates to the Database");
                changeCount = dbContext.SaveChanges();
                this.Status.Finished(string.Format("{0} Device last network logon dates updated", changeCount), "/Config/SystemConfig");
            }

            SystemLog.LogInformation(new string[]
                {
                    "Updated LastNetworkLogon Device Property for Device/s", 
                    changeCount.ToString()
                });
        }

        public static ScheduledTaskStatus ScheduleImmediately()
        {
            var existingTask = ScheduledTasks.GetTaskStatuses(typeof(ActiveDirectoryUpdateLastNetworkLogonDateJob)).Where(s => s.IsRunning).FirstOrDefault();
            if (existingTask != null)
                return existingTask;

            var instance = new ActiveDirectoryUpdateLastNetworkLogonDateJob();
            return instance.ScheduleTask();
        }

        public static bool UpdateLastNetworkLogonDate(Device Device)
        {
            System.DateTime? computerLastLogonDate = Device.LastNetworkLogonDate;
            if (!string.IsNullOrEmpty(Device.ComputerName))
            {
                foreach (var dcName in ActiveDirectoryHelpers.DefaultDomainDCNames)
                {
                    try
                    {
                        Ping p = new Ping();
                        PingReply pr;
                        try
                        {
                            pr = p.Send(dcName, 500);
                        }
                        finally
                        {
                            if (p != null)
                            {
                                ((System.IDisposable)p).Dispose();
                            }
                        }
                        if (pr.Status == IPStatus.Success)
                        {
                            using (DirectoryEntry dRootEntry = ActiveDirectoryHelpers.DefaultDCLdapRoot(dcName))
                            {
                                DirectorySearcher dSearcher = new DirectorySearcher(dRootEntry, string.Format("(&(objectClass=computer)(sAMAccountName={0}$))", ActiveDirectoryHelpers.EscapeLdapQuery(Device.ComputerName)), new string[]
									{
										"lastLogon"
									}, SearchScope.Subtree);
                                SearchResult dResult = dSearcher.FindOne();
                                if (dResult != null)
                                {
                                    ResultPropertyValueCollection dProp = dResult.Properties["lastLogon"];
                                    if (dProp != null && dProp.Count > 0)
                                    {
                                        long lastLogonInt = (long)dProp[0];
                                        if (lastLogonInt > 0L)
                                        {
                                            System.DateTime computerNameDate = System.DateTime.FromFileTime(lastLogonInt);
                                            if (computerLastLogonDate.HasValue)
                                            {
                                                if (System.DateTime.Compare(computerLastLogonDate.Value, computerNameDate) < 0)
                                                {
                                                    computerLastLogonDate = computerNameDate;
                                                }
                                            }
                                            else
                                            {
                                                computerLastLogonDate = computerNameDate;
                                            }
                                        }
                                    }
                                }

                            }
                        }
                        else
                        {
                            SystemLog.LogError(new string[]
								{
									string.Format("Unable to ping Domain Controller: '{0}' (ref: Disco.BI.Interop.ActiveDirectory.ActiveDirectoryUpdateLastNetworkLogonDateJob.UpdateDeviceLastNetworkLogonDate)", dcName)
								});
                        }
                    }
                    catch (System.Exception ex)
                    {
                        SystemLog.LogException("UpdateDeviceLastNetworkLogonDate", ex);
                    }
                }
            }
            bool UpdateLastNetworkLogonDate;
            if (computerLastLogonDate.HasValue)
            {
                if (!Device.LastNetworkLogonDate.HasValue)
                {
                    Device.LastNetworkLogonDate = computerLastLogonDate;
                    UpdateLastNetworkLogonDate = true;
                    return UpdateLastNetworkLogonDate;
                }
                if (System.DateTime.Compare(computerLastLogonDate.Value, Device.LastNetworkLogonDate.Value) > 0)
                {
                    Device.LastNetworkLogonDate = computerLastLogonDate;
                    UpdateLastNetworkLogonDate = true;
                    return UpdateLastNetworkLogonDate;
                }
            }
            UpdateLastNetworkLogonDate = false;
            return UpdateLastNetworkLogonDate;
        }
        private static void UpdateLastNetworkLogonDates(DiscoDataContext context, ScheduledTaskStatus status)
        {
            System.Collections.Generic.Dictionary<string, System.DateTime> computerLastLogonDates = new System.Collections.Generic.Dictionary<string, System.DateTime>();

            int progressDCCountTotal = ActiveDirectoryHelpers.DefaultDomainDCNames.Count;
            int progressDCCount = 0;
            double progressDCProgress = 0;
            if (progressDCCountTotal > 0)
                progressDCProgress = 90 / progressDCCountTotal;

            foreach (var dcName in ActiveDirectoryHelpers.DefaultDomainDCNames)
            {
                try
                {
                    PingReply pr;
                    using (Ping p = new Ping())
                    {
                        pr = p.Send(dcName, 2000);
                    }
                    if (pr.Status == IPStatus.Success)
                    {
                        using (DirectoryEntry dRootEntry = ActiveDirectoryHelpers.DefaultDCLdapRoot(dcName))
                        {
                            double progressDCStart = 5 + (progressDCCount * progressDCProgress);
                            status.UpdateStatus(progressDCStart, string.Format("Querying Domain Controller: {0}", dcName), "Searching...");

                            using (DirectorySearcher dSearcher = new DirectorySearcher(dRootEntry, "(objectClass=computer)", new string[] { "sAMAccountName", "lastLogon" }, SearchScope.Subtree))
                            {
                                using (SearchResultCollection dResults = dSearcher.FindAll())
                                {

                                    int progressItemCount = 0;
                                    double progressItemProgress = dResults.Count == 0 ? 0 : (progressDCProgress / dResults.Count);

                                    foreach (SearchResult dResult in dResults)
                                    {
                                        ResultPropertyValueCollection dProp = dResult.Properties["sAMAccountName"];
                                        if (dProp != null && dProp.Count > 0)
                                        {
                                            string computerName = ((string)dProp[0]).TrimEnd(new char[] { '$' }).ToUpper();

                                            if (progressItemCount % 150 == 0) // Only Update Status every 150 devices
                                                status.UpdateStatus(progressDCStart + (progressItemProgress * progressItemCount), string.Format("Analysing Device: {0}", computerName));

                                            dProp = dResult.Properties["lastLogon"];
                                            if (dProp != null && dProp.Count > 0)
                                            {
                                                long lastLogonInt = (long)dProp[0];
                                                if (lastLogonInt > 0L)
                                                {
                                                    System.DateTime computerNameDate = System.DateTime.FromFileTime(lastLogonInt);
                                                    System.DateTime existingDate;
                                                    if (computerLastLogonDates.TryGetValue(computerName, out existingDate))
                                                    {
                                                        if (System.DateTime.Compare(existingDate, computerNameDate) < 0)
                                                        {
                                                            computerLastLogonDates[computerName] = computerNameDate;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        computerLastLogonDates[computerName] = computerNameDate;
                                                    }
                                                }
                                            }
                                        }
                                        progressItemCount++;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        SystemLog.LogError(new string[]
							{
								string.Format("Unable to ping Domain Controller: '{0}' (ref: Disco.BI.Interop.ActiveDirectory.ActiveDirectoryUpdateLastNetworkLogonDateJob.UpdateLastNetworkLogonDates)", dcName)
							});
                    }
                }
                catch (System.Exception ex)
                {
                    SystemLog.LogException("UpdateLastNetworkLogonDates", ex);
                }
                progressDCCount++;
            }


            foreach (Device d in context.Devices.Where(device => device.ComputerName != null))
            {
                DateTime computerLastLogonDate;
                if (computerLastLogonDates.TryGetValue(d.ComputerName.ToUpper(), out computerLastLogonDate))
                {
                    if (d.LastNetworkLogonDate.HasValue)
                    {
                        if (System.DateTime.Compare(d.LastNetworkLogonDate.Value, computerLastLogonDate) < 0)
                        {
                            d.LastNetworkLogonDate = computerLastLogonDate;
                        }
                    }
                    else
                    {
                        d.LastNetworkLogonDate = computerLastLogonDate;
                    }
                }
            }
        }
    }
}
