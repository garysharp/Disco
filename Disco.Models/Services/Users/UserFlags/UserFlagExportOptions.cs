using Disco.Models.Exporting;
using Disco.Models.Services.Exporting;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Disco.Models.Services.Users.UserFlags
{
    public class UserFlagExportOptions : IExportOptions
    {
        public int Version { get; set; } = 1;
        public ExportFormat Format { get; set; }

        [Required]
        public List<int> UserFlagIds { get; set; } = new List<int>();

        [Display(Name = "Current Only")]
        public bool CurrentOnly { get; set; }

        // User Flag
        [Display(GroupName = "User Flag", Name = "Identifier", Description = "The identifier of the user flag")]
        public bool Id { get; set; }
        [Display(GroupName = "User Flag", Name = "Name", Description = "The name of the user flag")]
        public bool Name { get; set; }
        [Display(GroupName = "User Flag", Name = "Description", Description = "The description of the user flag")]
        public bool Description { get; set; }
        [Display(GroupName = "User Flag", Name = "Icon", Description = "The icon assigned to the user flag")]
        public bool Icon { get; set; }
        [Display(GroupName = "User Flag", Name = "Icon Colour", Description = "The icon colour assigned to the user flag")]
        public bool IconColour { get; set; }
        [Display(GroupName = "User Flag", Name = "Assignment Identifier", Description = "The identifier of the user flag assignment")]
        public bool AssignmentId { get; set; }
        [Display(GroupName = "User Flag", Name = "Added Date", Description = "The date the user flag was assigned to the user")]
        public bool AddedDate { get; set; }
        [Display(GroupName = "User Flag", Name = "Added User Identifier", Description = "The identifier of the user who assigned the user flag")]
        public bool AddedUserId { get; set; }
        [Display(GroupName = "User Flag", Name = "Removed Date", Description = "The date the user flag was unassigned from the user")]
        public bool RemovedDate { get; set; }
        [Display(GroupName = "User Flag", Name = "Removed User Identifier", Description = "The identifier of the user who unassigned the user flag")]
        public bool RemovedUserId { get; set; }
        [Display(GroupName = "User Flag", Name = "Comments", Description = "The comments associated with the user flag assignment")]
        public bool Comments { get; set; }


        // User
        [Display(GroupName = "User", Name = "Identifier", Description = "The identifier of the user assigned to the user flag")]
        public bool UserId { get; set; }
        [Display(GroupName = "User", Name = "Display Name", Description = "The display name of the user assigned to the user flag")]
        public bool UserDisplayName { get; set; }
        [Display(GroupName = "User", Name = "Surname", Description = "The surname of the user assigned to the user flag")]
        public bool UserSurname { get; set; }
        [Display(GroupName = "User", Name = "Given Name", Description = "The given name of the user assigned to the user flag")]
        public bool UserGivenName { get; set; }
        [Display(GroupName = "User", Name = "Phone Number", Description = "The phone number of the user assigned to the user flag")]
        public bool UserPhoneNumber { get; set; }
        [Display(GroupName = "User", Name = "Email Address", Description = "The email address of the user assigned to the user flag")]
        public bool UserEmailAddress { get; set; }
        public List<string> UserDetailCustom { get; set; } = new List<string>();
        public bool HasAssignedUserDetails()
            => UserDisplayName || UserSurname || UserGivenName || UserPhoneNumber || UserEmailAddress || (UserDetailCustom?.Any() ?? false);

        public static UserFlagExportOptions DefaultOptions()
        {
            return new UserFlagExportOptions()
            {
                Format = ExportFormat.Xlsx,
                CurrentOnly = true,
                Name = true,
                AddedDate = true,
                UserId = true,
                UserDisplayName = true,
                Comments = true,
            };
        }
    }
}
