using Disco.Models.Repository;
using System;
using System.Collections.Generic;

namespace Disco.Models.Services.Searching
{
    public class UserSearchResultItem : ISearchResultItem
    {
        private const string type = "User";
        private Lazy<string[]> LazyScoreValue;

        public UserSearchResultItem()
        {
            LazyScoreValue = new Lazy<string[]>(BuildScoreValues, false);
        }

        public string Id { get; set; }
        public string FriendlyId { get; set; }
        public string Type { get { return type; } }
        public string Description { get { return string.Format("{0} ({1})", DisplayName, Id); } }
        public string[] ScoreValues { get { return LazyScoreValue.Value; } }

        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }

        public int AssignedDevicesCount { get; set; }

        public int JobCount { get; set; }
        public int JobCountOpen { get; set; }

        public IList<UserFlagAssignment> UserFlagAssignments { get; set; }

        private string[] BuildScoreValues()
        {
            return new string[] {
                Id.Substring(Id.IndexOf('\\') + 1),
                Id,
                DisplayName
            };
        }
    }
}
