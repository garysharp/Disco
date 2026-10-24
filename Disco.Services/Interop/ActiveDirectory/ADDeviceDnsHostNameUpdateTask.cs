using Disco.Data.Repository;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.Linq;

namespace Disco.Services.Interop.ActiveDirectory
{
    public class ADDeviceDnsHostNameUpdateTask : ScheduledTask
    {
        public override string TaskName => "Active Directory - Device DNS Host Name Update Task";
        public override bool SingleInstanceTask => true;
        public override bool CancelInitiallySupported => false;
        public override bool LogExceptionsOnly => true;

        public override void InitalizeScheduledTask(DiscoDataContext Database)
        {
            // Trigger in 1 + 0-9 minutes
            //var rng = new Random();
            //var delay = rng.Next(10) + 1;
            var delay = 0.1;

            TriggerBuilder triggerBuilder = TriggerBuilder.Create()
                .StartAt(DateTimeOffset.Now.AddMinutes(delay));

            ScheduleTask(triggerBuilder);
        }

        protected override void ExecuteTask()
        {
            using (DiscoDataContext db = new DiscoDataContext())
            {
                var devices = db.Devices.Where(d => d.DeviceDomainId != null && d.DnsHostName == null).ToList();

                for (int i = 0; i < devices.Count; i++)
                {
                    var device = devices[i];

                    device.DnsHostName = device.ComputerName;
                    try
                    {
                        if (ActiveDirectory.IsValidDomainAccountId(device.DeviceDomainId))
                        {
                            var adMachineAccount = ActiveDirectory.RetrieveADMachineAccount(device.DeviceDomainId);
                            if (!string.IsNullOrWhiteSpace(adMachineAccount?.DnsName))
                            {
                                var dnsName = adMachineAccount.DnsName;
                                var index = dnsName.IndexOf('.');
                                if (index > 0)
                                    dnsName = dnsName.Substring(0, index);
                                device.DnsHostName = dnsName;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignore ad integration errors
                    }

                    if (((i + 1) % 50) == 0)
                        db.SaveChanges();
                }

                db.SaveChanges();
            }
        }
    }
}
