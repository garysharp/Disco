using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Disco.Models.Repository;

namespace Disco.Web.Extensions
{
    public static class DeviceModelExtensions
    {

        public static List<SelectListItem> ToSelectListItems(this IEnumerable<DeviceModel> deviceModels, int? SelectedId = null, bool IncludeNoModelItem = false)
        {
            var items = deviceModels.Select(db => new SelectListItem() { Value = db.Id.ToString(), Text = db.Description }).ToList();

            if (SelectedId.HasValue)
            {
                string selectedIdString = SelectedId.Value.ToString();
                var selectedItem = items.Where(i => i.Value == selectedIdString).FirstOrDefault();
                if (selectedItem != null)
                    selectedItem.Selected = true;
            }

            if (IncludeNoModelItem)
                items.Insert(0, new SelectListItem() { Value = string.Empty, Text = "<None Selected>", Selected = !SelectedId.HasValue });

            return items;
        }

    }
}
