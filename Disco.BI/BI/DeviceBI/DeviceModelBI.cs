using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disco.Data.Repository;
using Disco.Models.Repository;

namespace Disco.BI
{
    public static class DeviceModelBI
    {

        // Added: 2013-02-07 G#
        // Ensure Duplicate Device Models are not created by creating only one Device Model at a time
        // http://www.discoict.com.au/forum/support/2013/2/duplicate-device-models.aspx
        // Thanks to Michael Vorster for reporting this problem.
        private static object _CreateDeviceModelLock = new object();
        public static Tuple<DeviceModel, bool> GetOrCreateDeviceModel(this DbSet<DeviceModel> DeviceModelsSet, string Manufacturer, string Model, string ModelType)
        {
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
                            var addDeviceModel = new DeviceModel
                            {
                                Manufacturer = Manufacturer,
                                Model = Model,
                                ModelType = ModelType,
                                Description = string.Format("{0} {1}", Manufacturer, Model)
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

            return new Tuple<DeviceModel,bool>(deviceModel, false);
        }
        // Added: 2013-02-07 G#

    }
}
