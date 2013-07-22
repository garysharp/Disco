using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.BI.Device
{
    public class ImportDevice
    {
        [Required, StringLength(60)]
        public string SerialNumber { get; set; }

        public int? DeviceModelId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A valid Device Profile is Required")]
        public int DeviceProfileId { get; set; }

        public int? DeviceBatchId { get; set; }

        [StringLength(50)]
        public string AssignedUserId { get; set; }

        [StringLength(250)]
        public string Location { get; set; }
        
        [StringLength(40)]
        public string AssetNumber { get; set; }


        public Repository.Device Device { get; set; }
        public Repository.DeviceModel DeviceModel { get; set; }
        public Repository.DeviceProfile DeviceProfile { get; set; }
        public Repository.DeviceBatch DeviceBatch { get; set; }        
        public Repository.User AssignedUser { get; set; }
        
        public Dictionary<string, string> Errors { get; set; }
    }
}
