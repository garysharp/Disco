using System.Collections.Generic;

namespace Disco.Models.UI.Config.UserFlag
{
    public interface ConfigUserFlagShowModel : BaseUIModel
    {
        Repository.UserFlag UserFlag { get; set; }

        int CurrentAssignmentCount { get; set; }
        int TotalAssignmentCount { get; set; }

        IEnumerable<KeyValuePair<string, string>> Icons { get; set; }
        IEnumerable<KeyValuePair<string, string>> ThemeColours { get; set; }
    }
}