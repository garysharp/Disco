using Disco.Models.Repository;
using Disco.Services.Logging;
using Disco.Services.Logging.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Authorization
{
    public class AuthorizationLog : LogBase
    {
        private const int moduleId = 100;
        private const string moduleName = "Authorization";
        private const string moduleDescription = "Authorization Log";

        public override int ModuleId { get { return moduleId; } }
        public override string ModuleName { get { return moduleName; } }
        public override string ModuleDescription { get { return moduleDescription; } }

        public enum EventTypeIds : int
        {
            AccessDenied = 5,
            RoleCreated = 100,
            RoleDeleted = 110,
            RoleConfiguredRenamed = 151,
            RoleConfiguredSubjectsAdded = 152,
            RoleConfiguredSubjectsRemoved = 153,
            RoleConfiguredClaimsAdded = 154,
            RoleConfiguredClaimsRemoved = 155,
        }

        public static AuthorizationLog Current { get { return (AuthorizationLog)LogContext.LogModules[moduleId]; } }

        private static void Log(EventTypeIds EventTypeId, params object[] Args)
        {
            Current.Log((int)EventTypeId, Args);
        }

        protected override List<Logging.Models.LogEventType> LoadEventTypes()
        {
            List<LogEventType> eventTypes = new List<LogEventType>() {
                new LogEventType() {
                    Id = (int)EventTypeIds.AccessDenied,
                    ModuleId = moduleId,
                    Name = "Access Denied",
                    Format = "User: {0}; Resource: {1}; Reason: {2}",
                    Severity = (int)LogEventType.Severities.Warning,
                    UseLive = true, UsePersist = true, UseDisplay = true 
                },
                new LogEventType() {
                    Id = (int)EventTypeIds.RoleCreated,
                    ModuleId = moduleId,
                    Name = "Authorization Role Created",
                    Format = "{1} [{0}] was created by {2}",
                    Severity = (int)LogEventType.Severities.Information,
                    UseLive = true, UsePersist = true, UseDisplay = true
                },
                new LogEventType() {
                    Id = (int)EventTypeIds.RoleDeleted,
                    ModuleId = moduleId,
                    Name = "Authorization Role Deleted",
                    Format = "{1} [{0}] was deleted by {2}",
                    Severity = (int)LogEventType.Severities.Information,
                    UseLive = true, UsePersist = true, UseDisplay = true
                },
                new LogEventType() {
                    Id = (int)EventTypeIds.RoleConfiguredRenamed,
                    ModuleId = moduleId,
                    Name = "Authorization Role Renamed",
                    Format = "{1} [{0}] was renamed by {2}: from {3}",
                    Severity = (int)LogEventType.Severities.Information,
                    UseLive = true, UsePersist = true, UseDisplay = true
                },
                new LogEventType() {
                    Id = (int)EventTypeIds.RoleConfiguredSubjectsAdded,
                    ModuleId = moduleId,
                    Name = "Authorization Role Subjects Added",
                    Format = "{1} [{0}] had subjects added by {2}: {3}",
                    Severity = (int)LogEventType.Severities.Information,
                    UseLive = true, UsePersist = true, UseDisplay = true
                },
                new LogEventType() {
                    Id = (int)EventTypeIds.RoleConfiguredSubjectsRemoved,
                    ModuleId = moduleId,
                    Name = "Authorization Role Subjects Removed",
                    Format = "{1} [{0}] had subjects removed by {2}: {3}",
                    Severity = (int)LogEventType.Severities.Information,
                    UseLive = true, UsePersist = true, UseDisplay = true
                },
                new LogEventType() {
                    Id = (int)EventTypeIds.RoleConfiguredClaimsAdded,
                    ModuleId = moduleId,
                    Name = "Authorization Role Claims Added",
                    Format = "{1} [{0}] had claims added by {2}: {3}",
                    Severity = (int)LogEventType.Severities.Information,
                    UseLive = true, UsePersist = true, UseDisplay = true
                },
                new LogEventType() {
                    Id = (int)EventTypeIds.RoleConfiguredClaimsRemoved,
                    ModuleId = moduleId,
                    Name = "Authorization Role Claims Removed",
                    Format = "{1} [{0}] had claims removed by {2}: {3}",
                    Severity = (int)LogEventType.Severities.Information,
                    UseLive = true, UsePersist = true, UseDisplay = true
                }
            };

            return eventTypes;
        }

        #region Log Helpers
        public static void LogAccessDenied(string UserId, string Resource, string Reason)
        {
            Log(EventTypeIds.AccessDenied, UserId ?? "<Unknown>", Resource, Reason);
        }
        public static void LogRoleCreated(AuthorizationRole Role, string UserId)
        {
            Log(EventTypeIds.RoleCreated, Role.Id, Role.Name, UserId);
        }
        public static void LogRoleDeleted(AuthorizationRole Role, string UserId)
        {
            Log(EventTypeIds.RoleDeleted, Role.Id, Role.Name, UserId);
        }
        public static void LogRoleConfiguredRenamed(AuthorizationRole Role, string UserId, string OldRoleName)
        {
            Log(EventTypeIds.RoleConfiguredRenamed, Role.Id, Role.Name, UserId, OldRoleName);
        }
        public static void LogRoleConfiguredSubjectsAdded(AuthorizationRole Role, string UserId, IEnumerable<string> SubjectsAdded)
        {
            var subjects = string.Join("; ", SubjectsAdded);
            Log(EventTypeIds.RoleConfiguredSubjectsAdded, Role.Id, Role.Name, UserId, subjects);
        }
        public static void LogRoleConfiguredSubjectsRemoved(AuthorizationRole Role, string UserId, IEnumerable<string> SubjectsRemoved)
        {
            var subjects = string.Join("; ", SubjectsRemoved);
            Log(EventTypeIds.RoleConfiguredSubjectsRemoved, Role.Id, Role.Name, UserId, subjects);
        }
        public static void LogRoleConfiguredClaimsAdded(AuthorizationRole Role, string UserId, IEnumerable<string> ClaimsAdded)
        {
            var claims = string.Join("; ", ClaimsAdded);
            Log(EventTypeIds.RoleConfiguredClaimsAdded, Role.Id, Role.Name, UserId, claims);
        }
        public static void LogRoleConfiguredClaimsRemoved(AuthorizationRole Role, string UserId, IEnumerable<string> ClaimsRemoved)
        {
            var claims = string.Join("; ", ClaimsRemoved);
            Log(EventTypeIds.RoleConfiguredClaimsRemoved, Role.Id, Role.Name, UserId, claims);
        }
        #endregion
    }
}
