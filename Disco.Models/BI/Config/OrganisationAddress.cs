using System.ComponentModel.DataAnnotations;

namespace Disco.Models.BI.Config
{
    public class OrganisationAddress
    {
        public int? Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Suburb { get; set; }
        [Required]
        public string Postcode { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public string ShortName { get; set; }

        public string PhoneNumber { get; set; }
        public string FaxNumber { get; set; }
        public string EmailAddress { get; set; }

        public override string ToString()
            => $"{Name} ({ShortName})";
    }
}
