using Disco.Data.Repository;
using Disco.Services.Tasks;
using Microsoft.Web.Administration;
using Microsoft.Win32;
using Quartz;
using System;
using System.Linq;
using System.Text;

namespace Disco.Services.Interop.DiscoServices
{
    public class PreserveIisBindingsTask : ScheduledTask
    {
        private const string DiscoRegistryKey = @"SOFTWARE\Disco";

        public override string TaskName { get { return "Disco ICT - Preserve IIS Bindings"; } }
        public override bool SingleInstanceTask { get { return true; } }
        public override bool CancelInitiallySupported { get { return false; } }

        public override void InitalizeScheduledTask(DiscoDataContext Database)
        {
            TriggerBuilder triggerBuilder = TriggerBuilder.Create()
                .StartAt(DateTimeOffset.Now.AddMinutes(10));

            ScheduleTask(triggerBuilder);
        }

        protected override void ExecuteTask()
        {
            // retrieve IIS bindings
            var bindings = GetWebsiteBindings();

            if (bindings == null || !bindings.Any())
            {
                ScheduledTasksLog.LogScheduledTaskInformation(Status.TaskName, Status.SessionId, "Bindings for IIS Web Site 'Disco' not found");
                return;
            }

            using (var key = Registry.LocalMachine.CreateSubKey(DiscoRegistryKey))
            {
                key.SetValue("WebsiteBindings", bindings, RegistryValueKind.MultiString);
            }
        }

        private string[] GetWebsiteBindings()
        {
            using (var manager = new ServerManager())
            {
                var site = manager.Sites["Disco"];

                if (site == null)
                    return null;

                var bindings = site.Bindings;
                var sb = new StringBuilder();
                var result = new string[bindings.Count];
                for (int i = 0; i < bindings.Count; i++)
                {
                    var binding = bindings[i];
                    sb.Append(binding.BindingInformation);
                    sb.Append(';');
                    sb.Append(binding.Protocol);
                    if (string.Equals(binding.Protocol, "https", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append(';');
                        sb.Append(Convert.ToBase64String(binding.CertificateHash));
                        sb.Append(';');
                        sb.Append(binding.CertificateStoreName);
                    }

                    result[i] = sb.ToString();
                    sb.Clear();
                }

                return result;
            }
        }
    }
}