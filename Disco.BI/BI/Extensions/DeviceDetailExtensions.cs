using Disco.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Disco.BI.Extensions
{
    public static class DeviceDetailExtensions
    {
        #region Helpers
        private static string GetDetail(this IEnumerable<DeviceDetail> details, string Scope, string Key)
        {
            if (details == null)
                throw new ArgumentNullException("details");
            if (string.IsNullOrEmpty(Scope))
                throw new ArgumentNullException("Scope");
            if (string.IsNullOrEmpty(Key))
                throw new ArgumentNullException("Key");

            var detail = details.Where(d => d.Key == Key).FirstOrDefault();

            if (detail == null)
                return null;
            else
                return detail.Value;
        }

        private static void SetDetail(this Device device, string Scope, string Key, string Value)
        {
            if (device == null)
                throw new ArgumentNullException("device");
            if (string.IsNullOrEmpty(Scope))
                throw new ArgumentNullException("Scope");
            if (string.IsNullOrEmpty(Key))
                throw new ArgumentNullException("Key");

            var detail = device.DeviceDetails.Where(d => d.Scope == Scope && d.Key == Key).FirstOrDefault();

            // No Detail Stored & Set to Null
            if (detail == null && Value == null)
                return;

            if (detail == null)
            {
                detail = new DeviceDetail()
                {
                    DeviceSerialNumber = device.SerialNumber,
                    Scope = Scope,
                    Key = Key,
                    Value = Value
                };
                device.DeviceDetails.Add(detail);
            }

            if (detail.Value != Value)
            {
                if (Value == null)
                {
                    device.DeviceDetails.Remove(detail);
                }
                else
                {
                    detail.Value = Value;
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
    }
}
