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

        #region Scope Declaration

        public const string ScopeHardware = "Hardware";
        
        #endregion

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
        public const string KeyLanMacAddress = "LanMacAddress";
        /// <summary>
        /// Gets the LanMacAddress Device Detail Value
        /// </summary>
        /// <returns>The LanMacAddress or null</returns>
        public static string LanMacAddress(this IEnumerable<DeviceDetail> details)
        {
            return details.GetDetail(ScopeHardware, KeyLanMacAddress);
        }
        /// <summary>
        /// Sets the LanMacAddress Device Detail Value
        /// </summary>
        public static void LanMacAddress(this IEnumerable<DeviceDetail> details, Device device, string LanMacAddress)
        {
            device.SetDetail(ScopeHardware, KeyLanMacAddress, LanMacAddress);
        } 
        #endregion

        #region WLanMacAddress
        public const string KeyWLanMacAddress = "WLanMacAddress";
        /// <summary>
        /// Gets the WLanMacAddress Device Detail Value
        /// </summary>
        /// <returns>The WLanMacAddress or null</returns>
        public static string WLanMacAddress(this IEnumerable<DeviceDetail> details)
        {
            return details.GetDetail(ScopeHardware, KeyWLanMacAddress);
        }
        /// <summary>
        /// Sets the WLanMacAddress Device Detail Value
        /// </summary>
        public static void WLanMacAddress(this IEnumerable<DeviceDetail> details, Device device, string WLanMacAddress)
        {
            device.SetDetail(ScopeHardware, KeyWLanMacAddress, WLanMacAddress);
        } 
        #endregion

        #region ACAdapter
        public const string KeyACAdapter = "ACAdapter";
        /// <summary>
        /// Gets the ACAdapter Device Detail Value
        /// </summary>
        /// <returns>The ACAdapter or null</returns>
        public static string ACAdapter(this IEnumerable<DeviceDetail> details)
        {
            return details.GetDetail(ScopeHardware, KeyACAdapter);
        }
        /// <summary>
        /// Sets the ACAdapter Device Detail Value
        /// </summary>
        public static void ACAdapter(this IEnumerable<DeviceDetail> details, Device device, string ACAdapter)
        {
            device.SetDetail(ScopeHardware, KeyACAdapter, ACAdapter);
        }
        #endregion

    }
}
