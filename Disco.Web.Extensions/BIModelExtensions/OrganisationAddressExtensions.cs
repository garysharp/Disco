using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Disco.Models.BI.Config;

namespace Disco.Web.Extensions
{
    public static class OrganisationAddressExtensions
    {
        public static List<SelectListItem> ToSelectListItems(this IEnumerable<OrganisationAddress> organisationAddressess, OrganisationAddress SelectedItem)
        {
            int? selectedId = default(int?);

            if (SelectedItem != null)
                selectedId = SelectedItem.Id;

            return organisationAddressess.ToSelectListItems(selectedId);
        }

        public static List<SelectListItem> ToSelectListItems(this IEnumerable<OrganisationAddress> organisationAddressess, int? SelectedId = null, bool IncludeInstructionFirst = false, string InstructionMessage = "Select an Address")
        {
            var selectItems = default(List<SelectListItem>);
            if (!SelectedId.HasValue)
                selectItems = organisationAddressess.Select(wpd => new SelectListItem { Value = wpd.Id.Value.ToString(), Text = string.Format("{0} ({1})", wpd.Name, wpd.ShortName) }).ToList();
            else
                selectItems = organisationAddressess.Select(wpd => new SelectListItem { Value = wpd.Id.Value.ToString(), Text = string.Format("{0} ({1})", wpd.Name, wpd.ShortName), Selected = (SelectedId.Equals(wpd.Id)) }).ToList();

            if (IncludeInstructionFirst)
                selectItems.Insert(0, new SelectListItem() { Value = String.Empty, Text = String.Format("<{0}>", InstructionMessage), Selected = !SelectedId.HasValue });

            return selectItems;
        }
    }
}
