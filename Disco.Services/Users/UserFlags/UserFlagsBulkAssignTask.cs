using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services.Users.UserFlags
{
    public class UserFlagBulkAssignTask : ScheduledTask
    {
        public override string TaskName { get { return "User Flags - Bulk Assign Users"; } }

        public override bool SingleInstanceTask { get { return false; } }
        public override bool CancelInitiallySupported { get { return false; } }
        public override bool LogExceptionsOnly { get { return true; } }

        protected override void ExecuteTask()
        {
            int UserFlagId = (int)ExecutionContext.JobDetail.JobDataMap["UserFlagId"];
            string TechnicianUserId = (string)ExecutionContext.JobDetail.JobDataMap["TechnicianUserId"];
            string Comments = (string)ExecutionContext.JobDetail.JobDataMap["Comments"];
            List<string> UserIds = (List<string>)ExecutionContext.JobDetail.JobDataMap["UserIds"];
            bool Override = (bool)ExecutionContext.JobDetail.JobDataMap["Override"];

            using (DiscoDataContext Database = new DiscoDataContext())
            {
                // Load Flag
                var userFlag = Database.UserFlags.FirstOrDefault(uf => uf.Id == UserFlagId);

                if (userFlag == null)
                    throw new Exception("Invalid User Flag Id");
                Status.UpdateStatus(0, string.Format("Bulk Assigning Users to User Flag: {0}", userFlag.Name), "Preparing to start");

                // Load Technician
                var technician = Database.Users.FirstOrDefault(user => user.UserId == TechnicianUserId);
                if (technician == null)
                    throw new Exception("Invalid Technician User Id");

                // Parse Users
                var userIds = UserIds
                    .Select(u => ActiveDirectory.ParseDomainAccountId(u))
                    .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                Status.UpdateStatus(10, "Loading users from the database");
                var users = Database.Users.Include("UserFlagAssignments").Where(u => userIds.Contains(u.UserId)).ToList();

                var missingUserIds = userIds.Where(uid => !users.Any(u => u.UserId.Equals(uid, StringComparison.OrdinalIgnoreCase))).ToList();

                if (missingUserIds.Count > 0)
                {
                    var invalidUsersIds = new List<string>();

                    for (int index = 0; index < missingUserIds.Count; index++)
                    {
                        var userId = missingUserIds[index];
                        Status.UpdateStatus(20 + (index * ((double)30 / missingUserIds.Count)), string.Format("Loading user from Active Directory: {0}", userId));
                        try
                        {
                            users.Add(UserService.GetUser(userId, Database, true));
                        }
                        catch (Exception)
                        {
                            invalidUsersIds.Add(userId);
                        }
                    }

                    if (invalidUsersIds.Count > 0)
                        throw new InvalidOperationException(string.Format("Bulk assignment aborted, invalid User Ids: {0}", string.Join(", ", invalidUsersIds)));
                }
                users = users.OrderBy(u => u.UserId).ToList();

                Status.ProgressOffset = 50;
                Status.ProgressMultiplier = 0.5;

                if (Override)
                {
                    UserFlagService.BulkAssignOverrideUsers(Database, userFlag, technician, Comments, users, Status);
                }
                else
                {
                    UserFlagService.BulkAssignAddUsers(Database, userFlag, technician, Comments, users, Status);
                }
            }
        }

        public static ScheduledTaskStatus ScheduleBulkAssignUsers(UserFlag UserFlag, User Technician, string Comments, List<string> UserIds, bool Override)
        {
            JobDataMap taskData = new JobDataMap() {
                {"UserFlagId", UserFlag.Id },
                {"TechnicianUserId", Technician.UserId },
                {"Comments", Comments },
                {"UserIds", UserIds },
                {"Override", Override }
            };

            var instance = new UserFlagBulkAssignTask();

            return instance.ScheduleTask(taskData);
        }
    }
}