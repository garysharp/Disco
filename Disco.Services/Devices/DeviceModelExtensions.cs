using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Users;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;

namespace Disco.Services
{
    public static class DeviceModelExtensions
    {
        private static object _CreateDeviceModelLock = new object();

        public static bool CanDelete(this DeviceModel dm, DiscoDataContext Database)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Config.DeviceModel.Delete))
                return false;

            // Can't Delete Default Model (Id: 1)
            if (dm.Id == 1)
                return false;

            // Can't Delete if Contains Devices
            if (Database.Devices.Count(d => d.DeviceModelId == dm.Id) > 0)
                return false;

            return true;
        }
        public static void Delete(this DeviceModel dm, DiscoDataContext Database)
        {
            if (!dm.CanDelete(Database))
                throw new InvalidOperationException("The state of this Device Model doesn't allow it to be deleted");

            // Delete Image
            var deviceModelImagePath = dm.ImageFilePath();
            if (File.Exists(deviceModelImagePath))
                File.Delete(deviceModelImagePath);

            // Delete any Device Model Components
            foreach (var deviceModelComponent in Database.DeviceComponents.Where(dc => dc.DeviceModelId == dm.Id).ToList())
            {
                Database.DeviceComponents.Remove(deviceModelComponent);
            }

            // Delete Model
            Database.DeviceModels.Remove(dm);
        }

        public static Tuple<DeviceModel, bool> GetOrCreateDeviceModel(this DbSet<DeviceModel> DeviceModelsSet, string Manufacturer, string Model, string ModelType)
        {
            if (string.IsNullOrWhiteSpace(Manufacturer))
                Manufacturer = "Unknown";
            else
                Manufacturer = Manufacturer.Trim();

            if (string.IsNullOrWhiteSpace(Model))
                Model = "Unknown";
            else
                Model = Model.Trim();

            if (string.IsNullOrWhiteSpace(ModelType))
                ModelType = null;
            else
                ModelType = ModelType.Trim();

            // Already Exists?
            var deviceModel = DeviceModelsSet.FirstOrDefault(dm => dm.Manufacturer == Manufacturer && dm.Model == Model);
            if (deviceModel == null)
            {
                // Ensure only one thread/request at a time
                lock (_CreateDeviceModelLock)
                {
                    // Check again now that lock is enforced
                    deviceModel = DeviceModelsSet.FirstOrDefault(dm => dm.Manufacturer == Manufacturer && dm.Model == Model);

                    if (deviceModel == null)
                    {
                        // Create the Device Model in a different DataContext so we don't have to commit unrelated changes
                        using (DiscoDataContext database = new DiscoDataContext())
                        {
                            var description = $"{Manufacturer} {Model}";

                            if (Model.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                            {
                                description = $"Unknown {Manufacturer} Model";
                            }

                            var addDeviceModel = new DeviceModel
                            {
                                Manufacturer = Manufacturer,
                                Model = Model,
                                ModelType = ModelType,
                                Description = description
                            };
                            database.DeviceModels.Add(addDeviceModel);
                            database.SaveChanges();
                        }

                        // Obtain the Device Model with the in-scope DataContext
                        // - Overhead acknowledged, but reasonable given the infrequency of occurrence
                        deviceModel = DeviceModelsSet.FirstOrDefault(dm => dm.Manufacturer == Manufacturer && dm.Model == Model);
                        return new Tuple<DeviceModel, bool>(deviceModel, true);
                    }
                }
            }
            else
            {
                if (deviceModel.ModelType != ModelType)
                    deviceModel.ModelType = ModelType;
            }

            return new Tuple<DeviceModel, bool>(deviceModel, false);
        }

    }
}
