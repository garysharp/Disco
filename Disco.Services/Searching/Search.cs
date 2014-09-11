using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Jobs.JobLists;
using Disco.Models.Services.Searching;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Searching
{
    public static class Search
    {

        #region Jobs
        public static List<JobSearchResultItem> SearchJobs(DiscoDataContext Database, string Term, int? LimitCount = ActiveDirectory.DefaultSearchResultLimit)
        {
            int termInt = default(int);

            IQueryable<Job> query = default(IQueryable<Job>);

            string userIdTerm = Term.Contains('\\') ? Term : ActiveDirectory.ParseDomainAccountId(Term);

            if (int.TryParse(Term, out termInt))
            {
                // Term is a Number (int)
                query = Database.Jobs.Where(j =>
                        j.Id == termInt ||
                        j.Device.SerialNumber.Contains(Term) ||
                        j.Device.AssetNumber.Contains(Term) ||
                        j.User.UserId == userIdTerm ||
                        j.User.DisplayName.Contains(Term));
            }
            else
            {
                query = Database.Jobs.Where(j =>
                        j.Device.SerialNumber.Contains(Term) ||
                        j.Device.AssetNumber.Contains(Term) ||
                        j.User.UserId == userIdTerm ||
                        j.User.DisplayName.Contains(Term));
            }

            if (LimitCount.HasValue)
                query = query.Take(LimitCount.Value);

            return query.Select(j => new
            {
                Id = j.Id,
                DeviceSerialNumber = j.DeviceSerialNumber,
                UserId = j.UserId,
                UserDisplayName = j.User.DisplayName
            }).ToArray().Select(i => new JobSearchResultItem()
            {
                Id = i.Id.ToString(),
                DeviceSerialNumber = i.DeviceSerialNumber,
                UserId = i.UserId,
                UserDisplayName = i.UserDisplayName
            }).OrderByDescending(i => i.ScoreValues.Score(Term)).ToList();
        }

        public static JobTableModel SearchJobsTable(DiscoDataContext Database, string Term, int? LimitCount = ActiveDirectory.DefaultSearchResultLimit, bool IncludeJobStatus = true, bool SearchDetails = false)
        {
            int termInt = default(int);

            IQueryable<Job> query = default(IQueryable<Job>);

            string userIdTerm = Term.Contains('\\') ? Term : ActiveDirectory.ParseDomainAccountId(Term);

            if (int.TryParse(Term, out termInt))
            {
                // Term is a Number (int)
                if (SearchDetails)
                {
                    query = BuildJobTableModel(Database).Where(j =>
                        j.Id == termInt ||
                        j.DeviceHeldLocation.Contains(Term) ||
                        j.Device.SerialNumber.Contains(Term) ||
                        j.Device.AssetNumber.Contains(Term) ||
                        j.User.UserId == userIdTerm ||
                        j.User.Surname.Contains(Term) ||
                        j.User.GivenName.Contains(Term) ||
                        j.User.DisplayName.Contains(Term) ||
                        j.JobLogs.Any(jl => jl.Comments.Contains(Term)) ||
                        j.JobAttachments.Any(ja => ja.Comments.Contains(Term)));
                }
                else
                {
                    query = BuildJobTableModel(Database).Where(j =>
                        j.Id == termInt ||
                        j.DeviceHeldLocation.Contains(Term) ||
                        j.Device.SerialNumber.Contains(Term) ||
                        j.Device.AssetNumber.Contains(Term) ||
                        j.User.UserId == userIdTerm ||
                        j.User.Surname.Contains(Term) ||
                        j.User.GivenName.Contains(Term) ||
                        j.User.DisplayName.Contains(Term));
                }
            }
            else
            {
                if (SearchDetails)
                {
                    query = BuildJobTableModel(Database).Where(j =>
                        j.DeviceHeldLocation.Contains(Term) ||
                        j.Device.SerialNumber.Contains(Term) ||
                        j.Device.AssetNumber.Contains(Term) ||
                        j.User.UserId == userIdTerm ||
                        j.User.Surname.Contains(Term) ||
                        j.User.GivenName.Contains(Term) ||
                        j.User.DisplayName.Contains(Term) ||
                        j.JobLogs.Any(jl => jl.Comments.Contains(Term)) ||
                        j.JobAttachments.Any(ja => ja.Comments.Contains(Term)));
                }
                else
                {
                    query = BuildJobTableModel(Database).Where(j =>
                        j.DeviceHeldLocation.Contains(Term) ||
                        j.Device.SerialNumber.Contains(Term) ||
                        j.Device.AssetNumber.Contains(Term) ||
                        j.User.UserId == userIdTerm ||
                        j.User.Surname.Contains(Term) ||
                        j.User.GivenName.Contains(Term) ||
                        j.User.DisplayName.Contains(Term));
                }
            }

            if (LimitCount.HasValue)
                query = query.Take(LimitCount.Value);

            JobTableModel model = new JobTableModel() { ShowStatus = IncludeJobStatus }
                .Fill(Database, query, true)
                .Score(Term);

            return model;
        }

        public static IQueryable<Job> BuildJobTableModel(DiscoDataContext Database)
        {
            return Database.Jobs.Include("JobType").Include("Device").Include("User").Include("OpenedTechUser");
        }
        #endregion

        #region Users
        public static List<UserSearchResultItem> SearchUsersUpstream(DiscoDataContext Database, string Term, bool PersistResults, int? LimitCount = ActiveDirectory.DefaultSearchResultLimit)
        {
            return UserService.SearchUsers(Database, Term, PersistResults, LimitCount).Select(u => new UserSearchResultItem()
            {
                Id = u.UserId,
                FriendlyId = u.FriendlyId(),
                Surname = u.Surname,
                GivenName = u.GivenName,
                DisplayName = u.DisplayName,
                AssignedDevicesCount = 0,
                JobCount = 0,
                JobCountOpen = 0,
                UserFlagAssignments = null
            }).OrderByDescending(i => i.ScoreValues.Score(Term)).ToList();
        }

        public static List<UserSearchResultItem> SearchUsers(DiscoDataContext Database, string Term, bool PersistResults, int? LimitCount = ActiveDirectory.DefaultSearchResultLimit)
        {
            if (string.IsNullOrWhiteSpace(Term) || Term.Length < 2)
                throw new ArgumentException("Search Term must contain at least two characters", "Term");

            // Search Active Directory
            var adResults = SearchUsersUpstream(Database, Term, PersistResults, LimitCount);

            // Search Database
            var dbResults = Database.Users.Where(u =>
                    u.UserId.Contains(Term) ||
                    u.Surname.Contains(Term) ||
                    u.GivenName.Contains(Term) ||
                    u.DisplayName.Contains(Term))
                    .ToUserSearchResultItems(null)
                    .ToList();

            IEnumerable<UserSearchResultItem> results;
            if (PersistResults)
            {
                // AD Search persisted the results to the database
                results = dbResults;
            }
            else
            {
                var adResultsIndexed = adResults.ToDictionary(u => u.Id, StringComparer.OrdinalIgnoreCase);
                var dbResultsIndexed = dbResults.ToDictionary(u => u.Id, StringComparer.OrdinalIgnoreCase);

                // Update DB Results
                dbResults.ForEach(u =>
                {
                    UserSearchResultItem adResult;
                    if (adResultsIndexed.TryGetValue(u.Id, out adResult))
                    {
                        u.Surname = adResult.Surname;
                        u.GivenName = adResult.GivenName;
                        u.DisplayName = adResult.DisplayName;
                    }
                });

                // Add AD Results
                var adResultsAdditional = adResults.Where(u => !dbResultsIndexed.ContainsKey(u.Id));

                // Join AD & DB Results
                results = adResultsAdditional.Concat(dbResults);
            }

            results = results.OrderByDescending(i => i.ScoreValues.Score(Term));

            if (LimitCount.HasValue)
                results = results.Take(LimitCount.Value);

            return results.ToList();
        }
        public static List<UserSearchResultItem> SearchUserFlag(DiscoDataContext Database, int UserFlagId)
        {
            return Database.UserFlagAssignments
                .Where(a => a.UserFlagId == UserFlagId && !a.RemovedDate.HasValue)
                .Select(a => a.User)
                .ToUserSearchResultItems(null);
        }

        private static List<UserSearchResultItem> ToUserSearchResultItems(this IQueryable<User> Query, int? LimitCount = ActiveDirectory.DefaultSearchResultLimit)
        {
            if (LimitCount.HasValue)
                Query = Query.Take(LimitCount.Value);

            var results = Query.Select(u => new UserSearchResultItem()
            {
                Id = u.UserId,
                Surname = u.Surname,
                GivenName = u.GivenName,
                DisplayName = u.DisplayName,
                AssignedDevicesCount = u.DeviceUserAssignments.Where(dua => !dua.UnassignedDate.HasValue).Count(),
                JobCount = u.Jobs.Count(),
                JobCountOpen = u.Jobs.Count(j => !j.ClosedDate.HasValue),
                UserFlagAssignments = u.UserFlagAssignments
            }).ToList();

            results.ForEach(u => u.FriendlyId = ActiveDirectory.FriendlyAccountId(u.Id));

            return results;
        }
        #endregion

        #region Devices
        public static List<DeviceSearchResultItem> SearchDevices(DiscoDataContext Database, string Term, int? LimitCount = ActiveDirectory.DefaultSearchResultLimit, bool SearchDetails = false)
        {
            IQueryable<Device> query;

            query = null;

            if (SearchDetails)
            {
                query = Database.Devices.Where(d =>
                    d.AssetNumber.Contains(Term) ||
                    d.DeviceDomainId.Contains(Term) ||
                    d.SerialNumber.Contains(Term) ||
                    d.Location.Contains(Term) ||
                    Term.Contains(d.SerialNumber) ||
                    d.DeviceDetails.Any(dd => dd.Value.Contains(Term))
                    );
            }
            else
            {
                query = Database.Devices.Where(d =>
                    d.AssetNumber.Contains(Term) ||
                    d.DeviceDomainId.Contains(Term) ||
                    d.SerialNumber.Contains(Term) ||
                    d.Location.Contains(Term) ||
                    Term.Contains(d.SerialNumber));
            }

            return query
                .ToDeviceSearchResultItems(LimitCount)
                .OrderByDescending(i => i.ScoreValues.Score(Term))
                .ToList();
        }

        public static List<DeviceSearchResultItem> SearchDeviceModel(DiscoDataContext Database, int DeviceModelId)
        {
            return Database.Devices.Where(d => d.DeviceModelId == DeviceModelId).ToDeviceSearchResultItems(null);
        }
        public static List<DeviceSearchResultItem> SearchDeviceProfile(DiscoDataContext Database, int DeviceProfileId)
        {
            return Database.Devices.Where(d => d.DeviceProfileId == DeviceProfileId).ToDeviceSearchResultItems(null);
        }
        public static List<DeviceSearchResultItem> SearchDeviceBatch(DiscoDataContext Database, int DeviceBatchId)
        {
            return Database.Devices.Where(d => d.DeviceBatchId == DeviceBatchId).ToDeviceSearchResultItems(null);
        }

        private static List<DeviceSearchResultItem> ToDeviceSearchResultItems(this IQueryable<Device> Query, int? LimitCount = ActiveDirectory.DefaultSearchResultLimit)
        {
            if (LimitCount.HasValue)
                Query = Query.Take(LimitCount.Value);

            return Query.Select(d => new DeviceSearchResultItem()
            {
                Id = d.SerialNumber,
                AssetNumber = d.AssetNumber,
                ComputerName = d.DeviceDomainId,
                DeviceModelDescription = d.DeviceModel.Description,
                DeviceProfileDescription = d.DeviceProfile.Description,
                DeviceBatchName = d.DeviceBatch.Name,
                DecommissionedDate = d.DecommissionedDate,
                AssignedUserId = d.AssignedUserId,
                AssignedUserDisplayName = d.AssignedUser.DisplayName,
                JobCount = d.Jobs.Count()
            }).ToList();
        }
        #endregion

    }
}
