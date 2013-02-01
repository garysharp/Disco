using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.API.Models.DeviceModel
{
    public class _DeviceModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string ModelType { get; set; }

        public static _DeviceModel FromDeviceModel(Disco.Models.Repository.DeviceModel dm)
        {
            return new _DeviceModel()
            {
                Id = dm.Id,
                Description = dm.Description,
                Manufacturer = dm.Manufacturer,
                Model = dm.Model,
                ModelType = dm.ModelType
            };
        }
    }
}