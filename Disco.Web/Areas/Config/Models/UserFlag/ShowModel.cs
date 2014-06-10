using Disco.Models.UI.Config.UserFlag;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.UserFlag
{
    public class ShowModel : ConfigUserFlagShowModel
    {
        public Disco.Models.Repository.UserFlag UserFlag { get; set; }

        public int CurrentAssignmentCount { get; set; }
        public int TotalAssignmentCount { get; set; }

        public IEnumerable<KeyValuePair<string, string>> Icons { get; set; }
        public IEnumerable<KeyValuePair<string, string>> ThemeColours { get; set; }
    }
}