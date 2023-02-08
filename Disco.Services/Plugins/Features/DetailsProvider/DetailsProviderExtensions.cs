using Disco.Data.Repository;
using Disco.Models.UI.Device;
using Disco.Models.UI.Job;
using Disco.Models.UI.User;

namespace Disco.Services.Plugins.Features.DetailsProvider
{
    public static class DetailsProviderExtensions
    {

        public static void PopulateDetails(this UserShowModel model, DiscoDataContext database)
        {
            var service = new DetailsProviderService(database);

            model.UserDetails = service.GetDetails(model.User);
            model.HasUserPhoto = service.HasUserPhoto(model.User);
        }

        public static void PopulateDetails(this DeviceShowModel model, DiscoDataContext database)
        {
            if (model.Device.AssignedUser != null)
            {
                var service = new DetailsProviderService(database);
                model.AssignedUserDetails = service.GetDetails(model.Device.AssignedUser);
                model.HasAssignedUserPhoto = service.HasUserPhoto(model.Device.AssignedUser);
            }
        }

        public static void PopulateDetails(this JobShowModel model, DiscoDataContext database)
        {
            if (model.Job.User != null)
            {
                var service = new DetailsProviderService(database);
                model.UserDetails = service.GetDetails(model.Job.User);
                model.HasUserPhoto = service.HasUserPhoto(model.Job.User);
            }
        }

    }
}
