using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Jobs.JobLists;
using Disco.Services;
using System.Linq;

namespace Disco.BI.JobBI
{
    public static class Searching
    {
        public static JobTableModel Search(DiscoDataContext Database, string Term, int? LimitCount = null, bool IncludeJobStatus = true, bool SearchDetails = false)
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
                        j.User.Id == Term ||
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
                        j.User.Id == Term ||
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
                        j.User.Id == Term ||
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
                        j.User.Id == Term ||
                        j.User.Surname.Contains(Term) ||
                        j.User.GivenName.Contains(Term) ||
                        j.User.DisplayName.Contains(Term));
                }
            }

            if (LimitCount.HasValue)
                query = query.Take(LimitCount.Value);

            JobTableModel model = new JobTableModel() { ShowStatus = IncludeJobStatus };
            model.Fill(Database, query);

            return model;
        }

        public static IQueryable<Job> BuildJobTableModel(DiscoDataContext Database)
        {
            return Database.Jobs.Include("JobType").Include("Device").Include("User").Include("OpenedTechUser");
        }

    }
}
