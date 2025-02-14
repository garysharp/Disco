using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Documents;
using Disco.Services.Documents;
using Disco.Services.Interop.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services
{
    public static class UserExtensions
    {
        public static bool IsInPrimaryDomain(this User u)
        {
            return u.Domain.Equals(ActiveDirectory.Context.PrimaryDomain.NetBiosName, StringComparison.OrdinalIgnoreCase);
        }

        public static string ToStringFriendly(this User u)
        {
            return string.Format("{0} ({1})", u.DisplayName, u.FriendlyId());
        }

        public static string FriendlyId(this User u)
        {
            return ActiveDirectory.FriendlyAccountId(u.UserId);
        }

        public static List<DocumentTemplate> AvailableDocumentTemplates(this User u, DiscoDataContext Database, User User, DateTime TimeStamp)
        {
            var dts = Database.DocumentTemplates.Include("JobSubTypes")
               .Where(dt => !dt.IsHidden && dt.Scope == DocumentTemplate.DocumentTemplateScopes.User)
               .ToArray()
               .Where(dt => dt.FilterExpressionMatches(u, Database, User, TimeStamp, DocumentState.DefaultState())).ToList();

            return dts;
        }

        public static List<DocumentTemplatePackage> AvailableDocumentTemplatePackages(this User u, DiscoDataContext Database, User TechnicianUser)
        {
            return DocumentTemplatePackages.AvailablePackages(u, Database, TechnicianUser);
        }

        public static List<DeviceUserAssignment> CurrentDeviceUserAssignments(this User u)
        {
            return u.DeviceUserAssignments?
                .Where(dua => !dua.UnassignedDate.HasValue)
                .ToList() ?? new List<DeviceUserAssignment>(0);
        }

        public static bool CanCreateJob(this User u)
        {
            if (!JobActionExtensions.CanCreate())
                return false;

            return true;
        }

    }
}
