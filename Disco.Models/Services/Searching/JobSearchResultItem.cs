using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.Services.Searching
{
    public class JobSearchResultItem : ISearchResultItem
    {
        private const string type = "Job";
        private Lazy<string[]> LazyScoreValue;

        public JobSearchResultItem()
        {
            this.LazyScoreValue = new Lazy<string[]>(BuildScoreValues, false);
        }

        public virtual string Id { get; set; }
        public string Type { get { return type; } }
        public string Description { get { return string.Format("{0} ({1}; {2})", this.Id, this.UserId, this.DeviceSerialNumber); } }
        public string[] ScoreValues { get { return LazyScoreValue.Value; } }

        public string DeviceSerialNumber { get; set; }

        public string UserId { get; set; }
        public string UserFriendlyId { get; set; }
        public string UserDisplayName { get; set; }

        private string[] BuildScoreValues()
        {
            if (this.UserId == null)
            {
                return new string[] {
                    this.Id,
                    this.DeviceSerialNumber
                };
            }
            else
            {
                return new string[] {
                    this.Id,
                    this.UserId.Substring(this.UserId.IndexOf('\\') + 1),
                    this.UserId,
                    this.UserDisplayName,
                    this.DeviceSerialNumber
                };
            }
        }
    }
}
