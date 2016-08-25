using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Documents;
using Disco.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Disco.BI.Extensions
{
    public static class UserExtensions
    {
        public static List<DocumentTemplate> AvailableDocumentTemplates(this User u, DiscoDataContext Database, User User, DateTime TimeStamp)
        {
            var dts = Database.DocumentTemplates.Include("JobSubTypes")
               .Where(dt => dt.Scope == DocumentTemplate.DocumentTemplateScopes.User)
               .ToArray()
               .Where(dt => dt.FilterExpressionMatches(u, Database, User, TimeStamp, DocumentState.DefaultState())).ToList();

            return dts;
        }

        public static List<DeviceUserAssignment> CurrentDeviceUserAssignments(this User u)
        {
            return u.DeviceUserAssignments.Where(dua => !dua.UnassignedDate.HasValue).ToList();
        }

        public static bool CanCreateJob(this User u)
        {
            if (!JobActionExtensions.CanCreate())
                return false;

            return true;
        }
    }
}
