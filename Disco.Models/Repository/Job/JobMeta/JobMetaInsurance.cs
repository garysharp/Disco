using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class JobMetaInsurance
    {
        [Required, Key]
        public int JobId { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:yyyy/MM/dd hh:mm tt}", HtmlEncode = false)]
        public DateTime? LossOrDamageDate { get; set; }

        [StringLength(200)]
        public string EventLocation { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "Caused by Third Party")]
        public bool ThirdPartyCaused { get; set; }
        [StringLength(200)]
        public string ThirdPartyCausedName { get; set; }
        [DataType(DataType.MultilineText), StringLength(600)]
        public string ThirdPartyCausedWhy { get; set; }

        [StringLength(1200), DataType(DataType.MultilineText)]
        public string WitnessesNamesAddresses { get; set; }

        [StringLength(200)]
        public string BurglaryTheftMethodOfEntry { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:yyyy/MM/dd hh:mm tt}", HtmlEncode = false)]
        public DateTime? PropertyLastSeenDate { get; set; }

        [Display(Name = "Police Notified")]
        public bool PoliceNotified { get; set; }
        [StringLength(200)]
        public string PoliceNotifiedStation { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:yyyy/MM/dd}", HtmlEncode = false)]
        public DateTime? PoliceNotifiedDate { get; set; }
        [StringLength(400)]
        public string PoliceNotifiedCrimeReportNo { get; set; }

        [DataType(DataType.MultilineText), StringLength(800)]
        public string RecoverReduceAction { get; set; }

        [StringLength(500)]
        public string OtherInterestedParties { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:yyyy/MM/dd}", HtmlEncode = false)]
        public DateTime? DateOfPurchase { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:yyyy/MM/dd hh:mm tt}", HtmlEncode = false)]
        public DateTime? ClaimFormSentDate { get; set; }
        public string ClaimFormSentUserId { get; set; }
        
        [Required, ForeignKey("JobId")]
        public virtual Job Job { get; set; }

        [ForeignKey("ClaimFormSentUserId")]
        public virtual User ClaimFormSentUser { get; set; }
    }
}
