using System.Collections.Generic;
using System.Linq;

namespace Disco.Web.Areas.API.Models.DeviceModel
{
    public class ComponentModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Cost { get; set; }
        public List<string> JobSubTypes { get; set; }

        public static ComponentModel FromDeviceComponent(Disco.Models.Repository.DeviceComponent dc)
        {
            return new ComponentModel
            {
                Id = dc.Id,
                Description = dc.Description,
                Cost = dc.Cost.ToString("C"),
                JobSubTypes = dc.JobSubTypes.Select(j => $"{j.JobTypeId}_{j.Id}").ToList()
            };

        }
    }
}
