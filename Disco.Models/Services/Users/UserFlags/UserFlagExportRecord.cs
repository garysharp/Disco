using Disco.Models.Exporting;
using Disco.Models.Repository;
using System.Collections.Generic;

namespace Disco.Models.Services.Users.UserFlags
{
    public class UserFlagExportRecord : IExportRecord
    {
        public UserFlagAssignment Assignment { get; set; }
        public Dictionary<string, string> UserCustomDetails { get; set; }
    }
}
