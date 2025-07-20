using System;

namespace Disco.Models.Services.Searching
{
    public class JobSearchResultItem : ISearchResultItem
    {
        private const string type = "Job";
        private Lazy<string[]> LazyScoreValue;

        public JobSearchResultItem()
        {
            LazyScoreValue = new Lazy<string[]>(BuildScoreValues, false);
        }

        public virtual string Id { get; set; }
        public string Type { get { return type; } }
        public string Description { get { return string.Format("{0} ({1}; {2})", Id, UserId, DeviceSerialNumber); } }
        public string[] ScoreValues { get { return LazyScoreValue.Value; } }

        public string DeviceSerialNumber { get; set; }

        public string UserId { get; set; }
        public string UserFriendlyId { get; set; }
        public string UserDisplayName { get; set; }

        private string[] BuildScoreValues()
        {
            if (UserId == null)
            {
                return new string[] {
                    Id,
                    DeviceSerialNumber
                };
            }
            else
            {
                return new string[] {
                    Id,
                    UserId.Substring(UserId.IndexOf('\\') + 1),
                    UserId,
                    UserDisplayName,
                    DeviceSerialNumber
                };
            }
        }
    }
}
