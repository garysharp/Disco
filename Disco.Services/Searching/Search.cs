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
        public static List<JobSearchResultItem> SearchJobs(DiscoDataContext Database, string Term, int? LimitCount = null)
        {
            int termInt = default(int);

            IQueryable<Job> query = default(IQueryable<Job>);

            if (int.TryParse(Term, out termInt))
            {
                // Term is a Number (int)
                query = Database.Jobs.Where(j =>
                        j.Id == termInt ||
                        j.Device.SerialNumber.Contains(Term) ||
                        j.Device.AssetNumber.Contains(Term) ||
                        j.User.UserId == Term ||
                        j.User.DisplayName.Contains(Term));
            }
            else
            {
                query = Database.Jobs.Where(j =>
                        j.Device.SerialNumber.Contains(Term) ||
                        j.Device.AssetNumber.Contains(Term) ||
                        j.User.UserId == Term ||
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
            }).ToList();
        }

        public static JobTableModel SearchJobsTable(DiscoDataContext Database, string Term, int? LimitCount = null, bool IncludeJobStatus = true, bool SearchDetails = false)
        {
            int termInt = default(int);

            IQueryable<Job> query = default(IQueryable<Job>);

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
                        j.User.UserId == Term ||
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
                        j.User.UserId == Term ||
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
                        j.User.UserId == Term ||
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
                        j.User.UserId == Term ||
                        j.User.Surname.Contains(Term) ||
                        j.User.GivenName.Contains(Term) ||
                        j.User.DisplayName.Contains(Term));
                }
            }

            if (LimitCount.HasValue)
                query = query.Take(LimitCount.Value);

            JobTableModel model = new JobTableModel() { ShowStatus = IncludeJobStatus };
            model.Fill(Database, query, true);

            return model;
        }

        public static IQueryable<Job> BuildJobTableModel(DiscoDataContext Database)
        {
            return Database.Jobs.Include("JobType").Include("Device").Include("User").Include("OpenedTechUser");
        }
        #endregion

        #region Users
        public static List<UserSearchResultItem> SearchUsers(DiscoDataContext Database, string Term, int? LimitCount = null)
        {
            if (string.IsNullOrWhiteSpace(Term) || Term.Length < 2)
                throw new ArgumentException("Search Term must contain at least two characters", "Term");

            // Search Active Directory & Import Relevant Users
            UserService.SearchUsers(Database, Term);

            var matches = Database.Users.Where(u =>
                u.UserId.Contains(Term) ||
                u.Surname.Contains(Term) ||
                u.GivenName.Contains(Term) ||
                u.DisplayName.Contains(Term)
                );

            if (LimitCount.HasValue)
                matches = matches.Take(LimitCount.Value);

            return matches.Select(u => new UserSearchResultItem()
            {
                Id = u.UserId,
                Surname = u.Surname,
                GivenName = u.GivenName,
                DisplayName = u.DisplayName,
                AssignedDevicesCount = u.DeviceUserAssignments.Where(dua => !dua.UnassignedDate.HasValue).Count(),
                JobCount = u.Jobs.Count()
            }).ToList();
        }

        public static List<User> SearchUsersUpstream(string Term, int? LimitCount = null)
        {
            IEnumerable<ADUserAccount> matches = ActiveDirectory.SearchADUserAccounts(Term, Quick: true);

            if (LimitCount.HasValue)
                matches = matches.Take(LimitCount.Value);

            return matches.Select(m => m.ToRepositoryUser()).ToList();
        }
        #endregion

        #region Devices
        public static List<DeviceSearchResultItem> SearchDevices(DiscoDataContext Database, string Term, int? LimitCount = null, bool SearchDetails = false)
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

            return query.ToDeviceSearchResultItems(LimitCount);
        }

        public static List<DeviceSearchResultItem> SearchDeviceModel(DiscoDataContext Database, int DeviceModelId, int? LimitCount = null)
        {
            return Database.Devices.Where(d => d.DeviceModelId == DeviceModelId).ToDeviceSearchResultItems(LimitCount);
        }
        public static List<DeviceSearchResultItem> SearchDeviceProfile(DiscoDataContext Database, int DeviceProfileId, int? LimitCount = null)
        {
            return Database.Devices.Where(d => d.DeviceProfileId == DeviceProfileId).ToDeviceSearchResultItems(LimitCount);
        }
        public static List<DeviceSearchResultItem> SearchDeviceBatch(DiscoDataContext Database, int DeviceBatchId, int? LimitCount = null)
        {
            return Database.Devices.Where(d => d.DeviceBatchId == DeviceBatchId).ToDeviceSearchResultItems(LimitCount);
        }

        private static List<DeviceSearchResultItem> ToDeviceSearchResultItems(this IQueryable<Device> Query, int? LimitCount = null)
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
                DecommissionedDate = d.DecommissionedDate,
                AssignedUserId = d.AssignedUserId,
                AssignedUserDisplayName = d.AssignedUser.DisplayName,
                JobCount = d.Jobs.Count()
            }).ToList();
        }
        #endregion

    }
}
