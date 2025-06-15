using Disco.Models.ClientServices.EnrolmentInformation;
using Disco.Models.Repository;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services
{
    public static class DeviceDetailExtensions
    {
        #region Helpers
        public static string GetDetail(this IEnumerable<DeviceDetail> details, string scope, string key)
        {
            if (details == null)
                throw new ArgumentNullException("details");
            if (string.IsNullOrEmpty(scope))
                throw new ArgumentNullException("Scope");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("Key");

            var detail = details.Where(d => d.Scope == scope && d.Key == key).FirstOrDefault();

            if (detail == null)
                return null;
            else
                return detail.Value;
        }

        public static void SetDetail(this Device device, string scope, string key, string value)
        {
            if (device == null)
                throw new ArgumentNullException("device");
            if (string.IsNullOrEmpty(scope))
                throw new ArgumentNullException("Scope");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("Key");

            var detail = device.DeviceDetails.Where(d => d.Scope == scope && d.Key == key).FirstOrDefault();

            // No Detail Stored & Set to Null
            if (detail == null && value == null)
                return;

            if (detail == null)
            {
                detail = new DeviceDetail()
                {
                    DeviceSerialNumber = device.SerialNumber,
                    Scope = scope,
                    Key = key,
                    Value = value
                };
                device.DeviceDetails.Add(detail);
            }

            if (detail.Value != value)
            {
                if (value == null)
                {
                    device.DeviceDetails.Remove(detail);
                }
                else
                {
                    detail.Value = value;
                }
            }
        }
        #endregion

        #region LanMacAddress
        /// <summary>
        /// Gets the LanMacAddress Device Detail Value
        /// </summary>
        /// <returns>The LanMacAddress or null</returns>
        public static string LanMacAddress(this IEnumerable<DeviceDetail> details)
        {
            return details.GetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyLanMacAddress);
        }
        /// <summary>
        /// Sets the LanMacAddress Device Detail Value
        /// </summary>
        public static void LanMacAddress(this IEnumerable<DeviceDetail> details, Device device, string LanMacAddress)
        {
            device.SetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyLanMacAddress, LanMacAddress);
        }
        #endregion

        #region WLanMacAddress
        /// <summary>
        /// Gets the WLanMacAddress Device Detail Value
        /// </summary>
        /// <returns>The WLanMacAddress or null</returns>
        public static string WLanMacAddress(this IEnumerable<DeviceDetail> details)
        {
            return details.GetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyWLanMacAddress);
        }
        /// <summary>
        /// Sets the WLanMacAddress Device Detail Value
        /// </summary>
        public static void WLanMacAddress(this IEnumerable<DeviceDetail> details, Device device, string WLanMacAddress)
        {
            device.SetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyWLanMacAddress, WLanMacAddress);
        }
        #endregion

        #region ACAdapter
        /// <summary>
        /// Gets the ACAdapter Device Detail Value
        /// </summary>
        /// <returns>The ACAdapter or null</returns>
        public static string ACAdapter(this IEnumerable<DeviceDetail> details)
        {
            return details.GetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyACAdapter);
        }
        /// <summary>
        /// Sets the ACAdapter Device Detail Value
        /// </summary>
        public static void ACAdapter(this IEnumerable<DeviceDetail> details, Device device, string ACAdapter)
        {
            device.SetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyACAdapter, ACAdapter);
        }
        #endregion

        #region Battery
        /// <summary>
        /// Gets the Battery Device Detail Value
        /// </summary>
        /// <returns>The Battery or null</returns>
        public static string Battery(this IEnumerable<DeviceDetail> details)
        {
            return details.GetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyBattery);
        }
        /// <summary>
        /// Sets the Battery Device Detail Value
        /// </summary>
        public static void Battery(this IEnumerable<DeviceDetail> details, Device device, string Battery)
        {
            device.SetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyBattery, Battery);
        }
        #endregion

        #region Keyboard
        /// <summary>
        /// Gets the Keyboard Device Detail Value
        /// </summary>
        /// <returns>The Keyboard or null</returns>
        public static string Keyboard(this IEnumerable<DeviceDetail> details)
        {
            return details.GetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyKeyboard);
        }
        /// <summary>
        /// Sets the Keyboard Device Detail Value
        /// </summary>
        public static void Keyboard(this IEnumerable<DeviceDetail> details, Device device, string Keyboard)
        {
            device.SetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyKeyboard, Keyboard);
        }
        #endregion

        /// <summary>
        /// Gets the Network Adapters Device Detail Value
        /// </summary>
        public static List<NetworkAdapter> NetworkAdapters(this IEnumerable<DeviceDetail> details)
        {
            var json = details.GetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyNetworkAdapters);

            if (string.IsNullOrEmpty(json))
                return null;

            return JsonConvert.DeserializeObject<List<NetworkAdapter>>(json);
        }
        /// <summary>
        /// Sets the Network Adapters Device Detail Value
        /// </summary>
        public static void NetworkAdapters(this IEnumerable<DeviceDetail> details, Device device, List<NetworkAdapter> networkAdapters)
        {
            var json = default(string);

            if (networkAdapters != null && networkAdapters.Count > 0)
                json = JsonConvert.SerializeObject(networkAdapters);

            device.SetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyNetworkAdapters, json);
        }

        /// <summary>
        /// Gets the Processors Device Detail Value
        /// </summary>
        public static List<Processor> Processors(this IEnumerable<DeviceDetail> details)
        {
            var json = details.GetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyProcessors);

            if (string.IsNullOrEmpty(json))
                return null;

            return JsonConvert.DeserializeObject<List<Processor>>(json);
        }
        /// <summary>
        /// Sets the Processors Device Detail Value
        /// </summary>
        public static void Processors(this IEnumerable<DeviceDetail> details, Device device, List<Processor> processors)
        {
            var json = default(string);

            if (processors != null && processors.Count > 0)
                json = JsonConvert.SerializeObject(processors);

            device.SetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyProcessors, json);
        }

        /// <summary>
        /// Gets the Physical Memory Device Detail Value
        /// </summary>
        public static List<PhysicalMemory> PhysicalMemory(this IEnumerable<DeviceDetail> details)
        {
            var json = details.GetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyPhysicalMemory);

            if (string.IsNullOrEmpty(json))
                return null;

            return JsonConvert.DeserializeObject<List<PhysicalMemory>>(json);
        }
        /// <summary>
        /// Sets the Physical Memory Device Detail Value
        /// </summary>
        public static void PhysicalMemory(this IEnumerable<DeviceDetail> details, Device device, List<PhysicalMemory> physicalMemory)
        {
            var json = default(string);

            if (physicalMemory != null && physicalMemory.Count > 0)
                json = JsonConvert.SerializeObject(physicalMemory);

            device.SetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyPhysicalMemory, json);
        }

        /// <summary>
        /// Gets the Disk Drives Device Detail Value
        /// </summary>
        public static List<DiskDrive> DiskDrives(this IEnumerable<DeviceDetail> details)
        {
            var json = details.GetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyDiskDrives);

            if (string.IsNullOrEmpty(json))
                return null;

            return JsonConvert.DeserializeObject<List<DiskDrive>>(json);
        }
        /// <summary>
        /// Sets the Disk Drives Device Detail Value
        /// </summary>
        public static void DiskDrives(this IEnumerable<DeviceDetail> details, Device device, List<DiskDrive> diskDrives)
        {
            var json = default(string);

            if (diskDrives != null && diskDrives.Count > 0)
                json = JsonConvert.SerializeObject(diskDrives);

            device.SetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyDiskDrives, json);
        }

        /// <summary>
        /// Gets the Bios Device Detail Value
        /// </summary>
        public static List<Bios> Bios(this IEnumerable<DeviceDetail> details)
        {
            var json = details.GetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyBios);

            if (string.IsNullOrEmpty(json))
                return null;

            return JsonConvert.DeserializeObject<List<Bios>>(json);
        }
        /// <summary>
        /// Sets the Bios Device Detail Value
        /// </summary>
        public static void Bios(this IEnumerable<DeviceDetail> details, Device device, List<Bios> bios)
        {
            var json = default(string);

            if (bios != null && bios.Count > 0)
                json = JsonConvert.SerializeObject(bios);

            device.SetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyBios, json);
        }

        /// <summary>
        /// Gets the Base Board Device Detail Value
        /// </summary>
        public static List<BaseBoard> BaseBoard(this IEnumerable<DeviceDetail> details)
        {
            var json = details.GetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyBaseBoard);

            if (string.IsNullOrEmpty(json))
                return null;

            return JsonConvert.DeserializeObject<List<BaseBoard>>(json);
        }
        /// <summary>
        /// Sets the Base Board Device Detail Value
        /// </summary>
        public static void BaseBoard(this IEnumerable<DeviceDetail> details, Device device, List<BaseBoard> baseBoard)
        {
            var json = default(string);

            if (baseBoard != null && baseBoard.Count > 0)
                json = JsonConvert.SerializeObject(baseBoard);

            device.SetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyBaseBoard, json);
        }

        /// <summary>
        /// Gets the Computer System Device Detail Value
        /// </summary>
        public static List<ComputerSystem> ComputerSystem(this IEnumerable<DeviceDetail> details)
        {
            var json = details.GetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyComputerSystem);

            if (string.IsNullOrEmpty(json))
                return null;

            return JsonConvert.DeserializeObject<List<ComputerSystem>>(json);
        }
        /// <summary>
        /// Sets the Computer System Device Detail Value
        /// </summary>
        public static void ComputerSystem(this IEnumerable<DeviceDetail> details, Device device, List<ComputerSystem> computerSystem)
        {
            var json = default(string);

            if (computerSystem != null && computerSystem.Count > 0)
                json = JsonConvert.SerializeObject(computerSystem);

            device.SetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyComputerSystem, json);
        }

        /// <summary>
        /// Gets the Batteries Device Detail Value
        /// </summary>
        public static List<Battery> Batteries(this IEnumerable<DeviceDetail> details)
        {
            var json = details.GetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyBatteries);

            if (string.IsNullOrEmpty(json))
                return null;

            return JsonConvert.DeserializeObject<List<Battery>>(json);
        }
        /// <summary>
        /// Sets the Batteries Device Detail Value
        /// </summary>
        public static void Batteries(this IEnumerable<DeviceDetail> details, Device device, List<Battery> batteries)
        {
            var json = default(string);

            if (batteries != null && batteries.Count > 0)
                json = JsonConvert.SerializeObject(batteries);

            device.SetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyBatteries, json);
        }

        public static string MdmHardwareData(this IEnumerable<DeviceDetail> details)
        {
            return details.GetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyMdmHardwareData);
        }

        /// <summary>
        /// Sets the Mobile Device Management Hardware Data
        /// </summary>
        public static void MdmHardwareData(this IEnumerable<DeviceDetail> details, Device device, string mdmHardwareData)
        {
            device.SetDetail(DeviceDetail.ScopeHardware, DeviceDetail.HardwareKeyMdmHardwareData, mdmHardwareData);
        }

    }
}
