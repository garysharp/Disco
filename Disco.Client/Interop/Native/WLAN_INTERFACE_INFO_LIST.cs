using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Disco.Client.Interop.Native
{
    /// <summary>
    /// The WLAN_INTERFACE_INFO_LIST structure contains an array of NIC interface information.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct WLAN_INTERFACE_INFO_LIST
    {
        /// <summary>
        /// Contains the number of items in the InterfaceInfo member.
        /// </summary>
        public uint dwNumberOfItems;
        /// <summary>
        /// The index of the current item. The index of the first item is 0. dwIndex must be less than dwNumberOfItems.
        /// This member is not used by the wireless service. Applications can use this member when processing individual
        ///   interfaces in the WLAN_INTERFACE_INFO_LIST structure. When an application passes this structure from one
        ///   function to another, it can set the value of dwIndex to the index of the item currently being processed.
        ///   This can help an application maintain state.
        /// dwIndex should always be initialized before use.
        /// </summary>
        public uint dwIndex;

        private IntPtr InterfaceInfoPtr;

        /// <summary>
        /// An array of WLAN_INTERFACE_INFO structures containing interface information.
        /// </summary>
        public IEnumerable<WLAN_INTERFACE_INFO> InterfaceInfo
        {
            get
            {
                var size = Marshal.SizeOf(typeof(WLAN_INTERFACE_INFO));
                for (int i = 0; i < dwNumberOfItems; i++)
                {
                    yield return (WLAN_INTERFACE_INFO)Marshal.PtrToStructure(InterfaceInfoPtr + (i * size), typeof(WLAN_INTERFACE_INFO));
                }
            }
        }

        public WLAN_INTERFACE_INFO_LIST(IntPtr Pointer)
        {
            dwNumberOfItems = (uint)Marshal.ReadInt32(Pointer, 0);
            dwIndex = (uint)Marshal.ReadInt32(Pointer, 4);
            InterfaceInfoPtr = Pointer + 8;
        }
    }
}
