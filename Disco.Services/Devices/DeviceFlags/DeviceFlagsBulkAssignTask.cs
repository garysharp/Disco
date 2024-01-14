using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Disco.Services.Devices.DeviceFlags
{
    public class DeviceFlagBulkAssignTask : ScheduledTask
    {
        public override string TaskName { get { return "Device Flags - Bulk Assign Devices"; } }

        public override bool SingleInstanceTask { get { return false; } }
        public override bool CancelInitiallySupported { get { return false; } }
        public override bool LogExceptionsOnly { get { return true; } }

        protected override void ExecuteTask()
        {
            int deviceFlagId = (int)ExecutionContext.JobDetail.JobDataMap["DeviceFlagId"];
            string technicianUserId = (string)ExecutionContext.JobDetail.JobDataMap["TechnicianUserId"];
            string comments = (string)ExecutionContext.JobDetail.JobDataMap["Comments"];
            List<string> deviceSerialNumbers = (List<string>)ExecutionContext.JobDetail.JobDataMap["DeviceSerialNumbers"];
            bool @override = (bool)ExecutionContext.JobDetail.JobDataMap["Override"];

            using (DiscoDataContext Database = new DiscoDataContext())
            {
                // Load Flag
                var flag = Database.DeviceFlags.FirstOrDefault(uf => uf.Id == deviceFlagId);
                if (flag == null)
                    throw new Exception("Invalid Device Flag Id");

                Status.UpdateStatus(0, string.Format("Bulk Assigning Devices to Device Flag: {0}", flag.Name), "Preparing to start");

                // Load Technician
                var technician = Database.Users.FirstOrDefault(user => user.UserId == technicianUserId);
                if (technician == null)
                    throw new Exception("Invalid Technician User Id");

                // Parse Devices
                Status.UpdateStatus(10, "Loading devices from the database");
                var devices = Database.Devices
                    .Include(d => d.DeviceFlagAssignments)
                    .Where(d => deviceSerialNumbers.Contains(d.SerialNumber)).ToList();

                var missingDevices = deviceSerialNumbers.Where(sn => !devices.Any(u => string.Equals(sn, u.SerialNumber, StringComparison.OrdinalIgnoreCase))).ToList();
                if (missingDevices.Count > 0)
                {
                    throw new InvalidOperationException(string.Format("Bulk assignment aborted, invalid Serial Numbers: {0}", string.Join(", ", missingDevices)));
                }
                devices = devices.OrderBy(d => d.SerialNumber).ToList();

                Status.ProgressOffset = 50;
                Status.ProgressMultiplier = 0.5;

                if (@override)
                {
                    DeviceFlagService.BulkAssignOverrideDevices(Database, flag, technician, comments, devices, Status);
                }
                else
                {
                    DeviceFlagService.BulkAssignAddDevices(Database, flag, technician, comments, devices, Status);
                }
            }
        }

        public static ScheduledTaskStatus ScheduleBulkAssignDevices(DeviceFlag deviceFlag, User technician, string comments, List<string> deviceSerialNumbers, bool @override)
        {
            JobDataMap taskData = new JobDataMap() {
                {"DeviceFlagId", deviceFlag.Id },
                {"TechnicianUserId", technician.UserId },
                {"Comments", comments },
                {"DeviceSerialNumbers", deviceSerialNumbers },
                {"Override", @override }
            };

            var instance = new DeviceFlagBulkAssignTask();

            return instance.ScheduleTask(taskData);
        }
    }
}