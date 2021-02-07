using Disco.Data.Repository;
using Disco.Models.Services.Plugins.Details;
using Disco.Models.UI.Device;
using Disco.Models.UI.Job;
using Disco.Models.UI.User;
using System.Collections.Generic;

namespace Disco.Services.Plugins.Features.DetailsProvider
{
    public static class DetailsProviderExtensions
    {

        public static void PopulateDetails(this UserShowModel model, DiscoDataContext database)
        {
            var service = new DetailsProviderService(database);

            model.UserDetails = service.GetDetails(model.User);
            model.HasUserPhoto = service.HasUserPhoto(model.User);

            var currentAssignments = model.User.CurrentDeviceUserAssignments();
            if (currentAssignments.Count > 0)
            {
                model.AssignedDevicesDetails = new Dictionary<string, DetailsResult>(currentAssignments.Count);

                foreach (var device in currentAssignments)
                {
                    model.AssignedDevicesDetails[device.DeviceSerialNumber] = service.GetDetails(device.Device);
                }
            }
        }

        public static void PopulateDetails(this DeviceShowModel model, DiscoDataContext database)
        {
            var service = new DetailsProviderService(database);

            model.DeviceDetails = service.GetDetails(model.Device);

            if (model.Device.AssignedUser != null)
            {
                model.AssignedUserDetails = service.GetDetails(model.Device.AssignedUser);
                model.HasAssignedUserPhoto = service.HasUserPhoto(model.Device.AssignedUser);
            }
        }

        public static void PopulateDetails(this JobShowModel model, DiscoDataContext database)
        {
            var service = new DetailsProviderService(database);

            if (model.Job.Device != null)
                model.DeviceDetails = service.GetDetails(model.Job.Device);

            if (model.Job.User != null)
            {
                model.UserDetails = service.GetDetails(model.Job.User);
                model.HasUserPhoto = service.HasUserPhoto(model.Job.User);
            }
        }

    }
}
