using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.BI.Search;
using Disco.Models.Repository;
using Disco.Data.Repository;

namespace Disco.BI.UserBI
{
    public static class Searching
    {

        public static List<User> SearchUpstream(string Term)
        {
            return Interop.ActiveDirectory.ActiveDirectory.SearchUsers(Term).Select(adU => adU.ToRepositoryUser()).ToList();
        }

        private static List<UserSearchResultItem> Search_SelectUserSearchResultItems(IQueryable<User> Query, int? LimitCount = null)
        {
            if (LimitCount.HasValue)
                Query = Query.Take(LimitCount.Value);

            return Query.Select(u => new UserSearchResultItem()
            {
                Id = u.Id,
                Surname = u.Surname,
                GivenName = u.GivenName,
                DisplayName = u.DisplayName,
                AssignedDevicesCount = u.DeviceUserAssignments.Where(dua => !dua.UnassignedDate.HasValue).Count(),
                JobCount = u.Jobs.Count()
            }).ToList();
        }

        public static List<UserSearchResultItem> Search(DiscoDataContext dbContext, string Term, int? LimitCount = null)
        {
            if (string.IsNullOrWhiteSpace(Term) || Term.Length < 2)
                throw new ArgumentException("Search Term must contain at least two characters", "Term");

            // Search Active Directory & Import Relevant Users
            var adImportedUsers = Interop.ActiveDirectory.ActiveDirectory.SearchUsers(Term).Select(adU => adU.ToRepositoryUser());
            foreach (var adU in adImportedUsers)
            {
                var existingUser = dbContext.Users.Find(adU.Id);
                if (existingUser != null)
                    existingUser.UpdateSelf(adU);
                else
                    dbContext.Users.Add(adU);
                dbContext.SaveChanges();
                UserCache.InvalidateValue(adU.Id);
            }

            return Search_SelectUserSearchResultItems(dbContext.Users.Where(u =>
                u.Id.Contains(Term) ||
                u.Surname.Contains(Term) ||
                u.GivenName.Contains(Term) ||
                u.DisplayName.Contains(Term)
                ), LimitCount);
        }
    }
}
