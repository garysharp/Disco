using Disco.Models.BI.Config;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Extensions
{
    public static class OrganisationAddressExtensions
    {
        public static List<SelectListItem> ToSelectListItems(this IEnumerable<OrganisationAddress> organisationAddressess, OrganisationAddress SelectedItem)
        {
            var selectedId = default(int?);

            if (SelectedItem != null)
                selectedId = SelectedItem.Id;

            return organisationAddressess.ToSelectListItems(selectedId);
        }

        public static List<SelectListItem> ToSelectListItems(this IEnumerable<OrganisationAddress> organisationAddressess, int? SelectedId = null, bool IncludeInstructionFirst = false, string InstructionMessage = "Select an Address")
        {
            var selectItems = default(List<SelectListItem>);
            if (!SelectedId.HasValue)
                selectItems = organisationAddressess.Select(wpd => new SelectListItem { Value = wpd.Id.Value.ToString(), Text = $"{wpd.Name} ({wpd.ShortName})" }).ToList();
            else
                selectItems = organisationAddressess.Select(wpd => new SelectListItem { Value = wpd.Id.Value.ToString(), Text = $"{wpd.Name} ({wpd.ShortName})", Selected = (SelectedId.Equals(wpd.Id)) }).ToList();

            if (IncludeInstructionFirst)
                selectItems.Insert(0, new SelectListItem() { Value = string.Empty, Text = $"<{InstructionMessage}>", Selected = !SelectedId.HasValue });

            return selectItems;
        }
    }
}
