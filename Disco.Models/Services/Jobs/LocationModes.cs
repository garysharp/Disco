using System.ComponentModel.DataAnnotations;

namespace Disco.Models.Services.Jobs
{
    public enum LocationModes
    {
        [Display(Name = "Unrestricted")]
        Unrestricted,
        [Display(Name = "Optional List")]
        OptionalList,
        [Display(Name = "Restricted List")]
        RestrictedList
    }
}
