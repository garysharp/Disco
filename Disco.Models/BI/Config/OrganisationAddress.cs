using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        // Added 2012-12-11 G#
        // http://discoict.com.au/forum/support/2012/12/address-details.aspx
        public string PhoneNumber { get; set; }
        public string FaxNumber { get; set; }
        // End Added 2012-12-11 G#

        public string ToConfigurationEntry()
        {
            StringBuilder entryBuilder = new StringBuilder();
            
            entryBuilder.AppendLine(Name.Trim());
            entryBuilder.AppendLine(Address.Trim());
            entryBuilder.AppendLine(Suburb.Trim());
            entryBuilder.AppendLine(Postcode.Trim());
            entryBuilder.AppendLine(State.Trim());
            entryBuilder.AppendLine(Country.Trim());

            if (!string.IsNullOrEmpty(ShortName))
            {
                entryBuilder.AppendLine(ShortName.Trim());
            }
            
            return entryBuilder.ToString();
        }

        public static OrganisationAddress FromConfigurationEntry(int Id, string Entry)
        {
            string[] entryLines = Entry.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            if (entryLines.Length >= 6)
            {
                return new OrganisationAddress()
                {
                    Id = Id,
                    Name = entryLines[0].Trim(),
                    Address = entryLines[1].Trim(),
                    Suburb = entryLines[2].Trim(),
                    Postcode = entryLines[3].Trim(),
                    State = entryLines[4].Trim(),
                    Country = entryLines[5].Trim(),
                    ShortName = (entryLines.Length > 6 ? entryLines[6].Trim() : string.Empty)
                };
            }
            throw new ArgumentException("Invalid Configuration Address Entry", "entry");
        }

    }
}
