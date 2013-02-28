using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Disco.Models.Repository;

namespace Disco.Web.Extensions
{
    public static class DeviceBatchExtensions
    {

        public static List<SelectListItem> ToSelectListItems(this IEnumerable<DeviceBatch> deviceBatches, int? SelectedId = null, bool IncludeNoBatchItem = true)
        {
            var items = deviceBatches.Select(db => new SelectListItem() { Value = db.Id.ToString(), Text = db.Name }).ToList();

            if (SelectedId.HasValue)
            {
                string selectedIdString = SelectedId.Value.ToString();
                var selectedItem = items.Where(i => i.Value == selectedIdString).FirstOrDefault();
                if (selectedItem != null)
                    selectedItem.Selected = true;
            }

            if (IncludeNoBatchItem)
                items.Insert(0, new SelectListItem() { Value = string.Empty, Text = "Unknown", Selected = !SelectedId.HasValue });

            return items;
        }

    }
}
