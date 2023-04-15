using System;

namespace Disco.Web.Areas.API.Models.DocumentTemplate
{
    public class AddOnImportUserFlagRuleModel
    {
        public Guid Id { get; set; }
        public int FlagId { get; set; }
        public string UserId { get; set; }
        public bool AddFlag { get; set; }
        public string Comments { get; set; }

        public string UserDisplayName { get; set; }
        public string UserIdFriendly { get; set; }

        public string UserFlagIcon { get; set; }
        public string UserFlagColour { get; set; }
        public string UserFlagName { get; set; }
    }
}
