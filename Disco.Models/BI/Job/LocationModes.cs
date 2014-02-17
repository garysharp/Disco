using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.BI.Job
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
