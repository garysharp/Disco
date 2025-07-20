using Disco.Models.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Extensions
{
    public static class DeviceProfileExtensions
    {
        public static IEnumerable<SelectListItem> ToSelectListItems(this IEnumerable<DeviceProfile> deviceProfiles, DeviceProfile SelectedDeviceProfile = null)
        {
            var selectedId = 1;

            if (SelectedDeviceProfile != null)
                selectedId = SelectedDeviceProfile.Id;

            return deviceProfiles.ToSelectListItems(selectedId);
        }
        public static IEnumerable<SelectListItem> ToSelectListItems(this IEnumerable<DeviceProfile> deviceProfiles, int SelectedDeviceProfileId = 1)
        {
            return deviceProfiles.Select(dp => new SelectListItem()
            {
                Value = dp.Id.ToString(),
                Text = dp.ToString(),
                Selected = (dp.Id == SelectedDeviceProfileId)
            });
        }
    }
}
