using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Disco.Client.Interop.Native
{
    public struct WLAN_PROFILE_INFO_LIST
    {
        /// <summary>
        /// The number of wireless profile entries in the ProfileInfo member.
        /// </summary>
        public uint dwNumberOfItems;

        /// <summary>
        /// The index of the current item. The index of the first item is 0. The dwIndex member must be less than the dwNumberOfItems member.
        /// This member is not used by the wireless service. Applications can use this member when processing individual profiles in the
        /// WLAN_PROFILE_INFO_LIST structure. When an application passes this structure from one function to another, it can set the value
        /// of dwIndex to the index of the item currently being processed. This can help an application maintain state.
        /// dwIndex should always be initialized before use.
        /// </summary>
        public uint dwIndex;

        private IntPtr ProfileInfoPointer;

        /// <summary>
        /// An array of WLAN_PROFILE_INFO structures containing interface information. The number of items in the array is specified in the dwNumberOfItems member.
        /// </summary>
        public IEnumerable<WLAN_PROFILE_INFO> ProfileInfo
        {
            get
            {
                var size = Marshal.SizeOf(typeof(WLAN_PROFILE_INFO));
                for (int i = 0; i < dwNumberOfItems; i++)
                {
                    yield return (WLAN_PROFILE_INFO)Marshal.PtrToStructure(ProfileInfoPointer + (i * size), typeof(WLAN_PROFILE_INFO));
                }
            }
        }

        public WLAN_PROFILE_INFO_LIST(IntPtr Pointer)
        {
            dwNumberOfItems = (uint)Marshal.ReadInt32(Pointer, 0);
            dwIndex = (uint)Marshal.ReadInt32(Pointer, 4);
            ProfileInfoPointer = Pointer + 8;
        }
    }
}
