using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.Services.Devices.Importing
{
    public enum DeviceImportFieldTypes
    {
        [Required, Display(Name = "Device Serial Number", Description = "The device serial number")]
        DeviceSerialNumber,
        [Display(Name = "Device Asset Number", Description = "The device asset number")]
        DeviceAssetNumber,
        [Display(Name = "Device Location", Description = "The device location")]
        DeviceLocation,
        [Display(Name = "Device Decommissioned Date", Description = "The date the device was decommissioned in Disco")]
        DeviceDecommissionedDate,
        [Display(Name = "Device Decommissioned Reason", Description = "The reason the device was decommissioned")]
        DeviceDecommissionedReason,

        [Display(Name = "Device LAN MAC Address", Description = "The LAN MAC Address associated with the device")]
        DetailLanMacAddress,
        [Display(Name = "Device Wireless LAN MAC Address", Description = "The Wireless LAN MAC Address associated with the device")]
        DetailWLanMacAddress,
        [Display(Name = "Device AC Adapter", Description = "The AC Adapter associated with the device")]
        DetailACAdapter,
        [Display(Name = "Device Battery", Description = "The Battery associated with the device")]
        DetailBattery,

        [Display(Name = "Model Identifier", Description = "The identifier of the device model associated with the device")]
        ModelId,

        [Display(Name = "Batch Identifier", Description = "The identifier of the device batch associated with the device")]
        BatchId,

        [Display(Name = "Profile Identifier", Description = "The identifier of the device profile associated with the device")]
        ProfileId,

        [Display(Name = "Assigned User Identifier", Description = "The identifier of the user assigned to the device")]
        AssignedUserId,

        [Display(Name = "Ignore Column", Description = "The column will be ignored during the import")]
        IgnoreColumn
    }
}
