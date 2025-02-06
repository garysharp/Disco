using Disco.Models.Exporting;
using Disco.Models.Services.Exporting;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        [Display(ShortName = "User Flag", Name = "Identifier", Description = "The identifier of the user flag")]
        public bool Id { get; set; }
        [Display(ShortName = "User Flag", Name = "Name", Description = "The name of the user flag")]
        public bool Name { get; set; }
        [Display(ShortName = "User Flag", Name = "Description", Description = "The description of the user flag")]
        public bool Description { get; set; }
        [Display(ShortName = "User Flag", Name = "Icon", Description = "The icon assigned to the user flag")]
        public bool Icon { get; set; }
        [Display(ShortName = "User Flag", Name = "Icon Colour", Description = "The icon colour assigned to the user flag")]
        public bool IconColour { get; set; }
        [Display(ShortName = "User Flag", Name = "Assignment Identifier", Description = "The identifier of the user flag assignment")]
        public bool AssignmentId { get; set; }
        [Display(ShortName = "User Flag", Name = "Added Date", Description = "The date the user flag was assigned to the user")]
        public bool AddedDate { get; set; }
        [Display(ShortName = "User Flag", Name = "Added User Identifier", Description = "The identifier of the user who assigned the user flag")]
        public bool AddedUserId { get; set; }
        [Display(ShortName = "User Flag", Name = "Removed Date", Description = "The date the user flag was unassigned from the user")]
        public bool RemovedDate { get; set; }
        [Display(ShortName = "User Flag", Name = "Removed User Identifier", Description = "The identifier of the user who unassigned the user flag")]
        public bool RemovedUserId { get; set; }
        [Display(ShortName = "User Flag", Name = "Comments", Description = "The comments associated with the user flag assignment")]
        public bool Comments { get; set; }


        // User
        [Display(ShortName = "User", Name = "Identifier", Description = "The identifier of the user assigned to the user flag")]
        public bool UserId { get; set; }
        [Display(ShortName = "User", Name = "Display Name", Description = "The display name of the user assigned to the user flag")]
        public bool UserDisplayName { get; set; }
        [Display(ShortName = "User", Name = "Surname", Description = "The surname of the user assigned to the user flag")]
        public bool UserSurname { get; set; }
        [Display(ShortName = "User", Name = "Given Name", Description = "The given name of the user assigned to the user flag")]
        public bool UserGivenName { get; set; }
        [Display(ShortName = "User", Name = "Phone Number", Description = "The phone number of the user assigned to the user flag")]
        public bool UserPhoneNumber { get; set; }
        [Display(ShortName = "User", Name = "Email Address", Description = "The email address of the user assigned to the user flag")]
        public bool UserEmailAddress { get; set; }
        [Display(ShortName = "User", Name = "Custom Details", Description = "The custom details provided by plugins for the user assigned to the user flag")]
        public bool UserDetailCustom { get; set; }

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
