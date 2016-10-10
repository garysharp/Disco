using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Disco.Client.Interop.Native
{
    public static class WlanApi
    {

        public const uint WLAN_API_VERSION_2_0 = 2; // Windows Vista WiFi API Version
        public const int ERROR_SUCCESS = 0;
        public const int ERROR_INVALID_PARAMETER = 87;
        public const int ERROR_NOT_ENOUGH_MEMORY = 8;
        public const int ERROR_SERVICE_NOT_ACTIVE = 1062; // The service has not been started.

        /// <summary>
        /// The WlanOpenHandle function opens a connection to the server.
        /// </summary>
        /// <param name="dwClientVersion">The highest version of the WLAN API that the client supports.
        /// 1 = Client version for Windows XP with SP3 and Wireless LAN API for Windows XP with SP2.
        /// 2 = Client version for Windows Vista and Windows Server 2008</param>
        /// <param name="pReserved">Reserved for future use. Must be set to NULL.</param>
        /// <param name="pdwNegotiatedVersion">The version of the WLAN API that will be used in this session. This value is usually the highest version supported by both the client and server.</param>
        /// <param name="phClientHandle">A handle for the client to use in this session. This handle is used by other functions throughout the session.</param>
        /// <returns>
        /// If the function succeeds, the return value is ERROR_SUCCESS.
        /// If the function fails, the return value may be one of the following return codes.
        /// ERROR_INVALID_PARAMETER: pdwNegotiatedVersion is NULL, phClientHandle is NULL, or pReserved is not NULL.
        /// ERROR_NOT_ENOUGH_MEMORY: Failed to allocate memory to create the client context.
        /// RPC_STATUS: Various error codes.
        /// ERROR_REMOTE_SESSION_LIMIT_EXCEEDED: Too many handles have been issued by the server.
        /// </returns>
        /// <remarks>
        /// The version number specified by dwClientVersion and pdwNegotiatedVersion is a composite version number
        ///   made up of both major and minor versions. The major version is specified by the low-order word, and the
        ///   minor version is specified by the high-order word. The macros WLAN_API_VERSION_MAJOR(_v) and
        ///   WLAN_API_VERSION_MINOR(_v) return the major and minor version numbers respectively.
        ///   You can construct a version number using the macro WLAN_API_MAKE_VERSION(_major, _minor).
        /// Windows XP with SP3 and Wireless LAN API for Windows XP with SP2:  WlanOpenHandle will return an
        ///   error message if the Wireless Zero Configuration (WZC) service has not been started or if the WZC service is not responsive.
        /// </remarks>
        [DllImport("Wlanapi.dll", SetLastError = true)]
        public static extern uint WlanOpenHandle(uint dwClientVersion, IntPtr pReserved, out uint pdwNegotiatedVersion, out IntPtr phClientHandle);

        /// <summary>
        /// The WlanCloseHandle function closes a connection to the server.
        /// </summary>
        /// <param name="hClientHandle">The client's session handle, which identifies the connection to be closed. This handle was obtained by a previous call to the WlanOpenHandle function.</param>
        /// <param name="pReserved">Reserved for future use. Set this parameter to NULL.</param>
        /// <returns>
        /// If the function succeeds, the return value is ERROR_SUCCESS.
        /// If the function fails, the return value may be one of the following return codes.
        /// ERROR_INVALID_PARAMETER: hClientHandle is NULL or invalid, or pReserved is not NULL.
        /// ERROR_INVALID_HANDLE: The handle hClientHandle was not found in the handle table.
        /// RPC_STATUS: Various error codes.
        /// </returns>
        /// <remarks>
        /// After a connection has been closed, any attempted use of the closed handle can cause unexpected errors.
        ///   Upon closing, all outstanding notifications are discarded.
        /// Do not call WlanCloseHandle from a callback function. If the client is in the middle of a
        ///   notification callback when WlanCloseHandle is called, the function waits for the callback to
        ///   finish before returning a value. Calling this function inside a callback function will result in
        ///   the call never completing. If both the callback function and the thread that closes the handle try
        ///   to acquire the same lock, a deadlock may occur. In addition, do not call WlanCloseHandle from
        ///   the DllMain function in an application DLL. This could also cause a deadlock.
        /// </remarks>
        [DllImport("Wlanapi", SetLastError = true)]
        public static extern uint WlanCloseHandle(IntPtr hClientHandle, IntPtr pReserved);

        /// <summary>
        /// The WlanEnumInterfaces function enumerates all of the wireless LAN interfaces currently enabled on the local computer.
        /// </summary>
        /// <param name="hClientHandle">The client's session handle, obtained by a previous call to the WlanOpenHandle function.</param>
        /// <param name="pReserved">Reserved for future use. This parameter must be set to NULL.</param>
        /// <param name="ppInterfaceList">
        /// A pointer to storage for a pointer to receive the returned list of wireless LAN interfaces in a WLAN_INTERFACE_INFO_LIST structure.
        /// The buffer for the WLAN_INTERFACE_INFO_LIST returned is allocated by the WlanEnumInterfaces function if the call succeeds.
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is ERROR_SUCCESS.
        /// If the function fails, the return value may be one of the following return codes.
        /// ERROR_INVALID_PARAMETER: A parameter is incorrect. This error is returned if the hClientHandle or ppInterfaceList parameter is NULL. This error is returned if the pReserved is not NULL.
        ///   This error is also returned if the hClientHandle parameter is not valid.
        /// ERROR_INVALID_HANDLE: The handle hClientHandle was not found in the handle table.
        /// RPC_STATUS: Various error codes.
        /// ERROR_NOT_ENOUGH_MEMORY: Not enough memory is available to process this request and allocate memory for the query results.
        /// </returns>
        /// <remarks>
        /// The WlanEnumInterfaces function allocates memory for the list of returned interfaces that is returned in the
        ///   buffer pointed to by the ppInterfaceList parameter when the function succeeds. The memory used for the buffer
        ///   pointed to by ppInterfaceList parameter should be released by calling the WlanFreeMemory function
        ///   after the buffer is no longer needed.
        /// </remarks>
        [DllImport("Wlanapi", SetLastError = true)]
        public static extern uint WlanEnumInterfaces(IntPtr hClientHandle, IntPtr pReserved, out IntPtr ppInterfaceList);

        /// <summary>
        /// The WlanGetProfileList function retrieves the list of profiles in preference order.
        /// </summary>
        /// <param name="hClientHandle">The client's session handle, obtained by a previous call to the WlanOpenHandle function.</param>
        /// <param name="pInterfaceGuid">The GUID of the wireless interface.</param>
        /// <param name="pReserved">Reserved for future use. Must be set to NULL.</param>
        /// <param name="ppProfileList">A PWLAN_PROFILE_INFO_LIST structure that contains the list of profile information.</param>
        /// <returns>
        /// If the function succeeds, the return value is ERROR_SUCCESS.
        /// If the function fails, the return value may be one of the following return codes.
        /// ERROR_INVALID_HANDLE: The handle hClientHandle was not found in the handle table.
        /// ERROR_INVALID_PARAMETER: A parameter is incorrect.
        /// ERROR_NOT_ENOUGH_MEMORY: Not enough memory is available to process this request and allocate memory for the query results.
        /// RPC_STATUS: Various error codes.
        /// </returns>
        /// <remarks>
        /// The WlanGetProfileList function returns only the basic information on the wireless profiles on a wireless interface.
        ///   The list of wireless profiles on a wireless interface are retrieved in the preference order. The WlanSetProfilePosition
        ///   can be used to change the preference order for the wireless profiles on a wireless interface.
        /// More detailed information for a wireless profile on a wireless interface can be retrieved by using the WlanGetProfile
        ///   function. The WlanGetProfileCustomUserData function can be used to retrieve custom user data for a wireless profile on
        ///   a wireless interface. A list of the wireless interfaces and associated GUIDs on the local computer can be retrieved
        ///   using the WlanEnumInterfaces function.
        /// The WlanGetProfileList function allocates memory for the list of profiles returned in the buffer pointed to by the
        ///   ppProfileList parameter. The caller is responsible for freeing this memory using the WlanFreeMemory function when
        ///   this buffer is no longer needed.
        /// Windows XP with SP3 and Wireless LAN API for Windows XP with SP2:  Guest profiles, profiles with Wireless Provisioning
        ///   Service (WPS) authentication, and profiles with Wi-Fi Protected Access-None (WPA-None) authentication are not
        ///   supported. These types of profiles are not returned by WlanGetProfileList, even if a profile of this type appears
        ///   on the preferred profile list.
        /// </remarks>
        [DllImport("Wlanapi", SetLastError = true)]
        public static extern uint WlanGetProfileList(IntPtr hClientHandle, [MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid, IntPtr pReserved, out IntPtr ppProfileList);

        /// <summary>
        /// The WlanGetProfile function retrieves all information about a specified wireless profile.
        /// </summary>
        /// <param name="hClientHandle">The client's session handle, obtained by a previous call to the WlanOpenHandle function.</param>
        /// <param name="pInterfaceGuid">The GUID of the wireless interface. </param>
        /// <param name="strProfileName">The name of the profile. Profile names are case-sensitive. This string must be NULL-terminated. The maximum length of the profile name is 255 characters. This means that the maximum length of this string, including the NULL terminator, is 256 characters.</param>
        /// <param name="pReserved">Reserved for future use. Must be set to NULL.</param>
        /// <param name="pstrProfileXml">A string that is the XML representation of the queried profile. There is no predefined maximum string length.</param>
        /// <param name="pdwFlags">On input, a pointer to the address location used to provide additional information about the request. If this parameter is NULL on input, then no information on profile flags will be returned. On output, a pointer to the address location used to receive profile flags.</param>
        /// <param name="pdwGrantedAccess">The access mask of the all-user profile.</param>
        /// <returns>If the function succeeds, the return value is ERROR_SUCCESS.</returns>
        [DllImport("Wlanapi", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint WlanGetProfile(IntPtr hClientHandle, [MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid, [MarshalAs(UnmanagedType.LPWStr)] string strProfileName, IntPtr pReserved, out IntPtr pstrProfileXml, out uint pdwFlags, out IntPtr pdwGrantedAccess);

        /// <summary>
        /// The WlanSetProfile function sets the content of a specific profile.
        /// </summary>
        /// <param name="hClientHandle">The client's session handle, obtained by a previous call to the WlanOpenHandle function.</param>
        /// <param name="pInterfaceGuid">The GUID of the interface.</param>
        /// <param name="dwFlags">The flags to set on the profile.</param>
        /// <param name="strProfileXml">Contains the XML representation of the profile. The WLANProfile element is the root profile element. To view sample profiles, see Wireless Profile Samples. There is no predefined maximum string length.</param>
        /// <param name="strAllUserProfileSecurity">Sets the security descriptor string on the all-user profile. For more information about profile permissions, see the Remarks section.</param>
        /// <param name="bOverwrite">Specifies whether this profile is overwriting an existing profile. If this parameter is FALSE and the profile already exists, the existing profile will not be overwritten and an error will be returned.</param>
        /// <param name="pReserved">Reserved for future use. Must be set to NULL.</param>
        /// <param name="pdwReasonCode">A WLAN_REASON_CODE value that indicates why the profile is not valid.</param>
        /// <returns>If the function succeeds, the return value is ERROR_SUCCESS.</returns>
        [DllImport("Wlanapi", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint WlanSetProfile(IntPtr hClientHandle, [MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid, uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] string strProfileXml, [MarshalAs(UnmanagedType.LPWStr)] string strAllUserProfileSecurity, bool bOverwrite, IntPtr pReserved, out uint pdwReasonCode);

        /// <summary>
        /// The WlanDeleteProfile function deletes a wireless profile for a wireless interface on the local computer.
        /// </summary>
        /// <param name="hClientHandle">The client's session handle, obtained by a previous call to the WlanOpenHandle function.</param>
        /// <param name="pInterfaceGuid">The GUID of the interface from which to delete the profile. </param>
        /// <param name="strProfileName">The name of the profile to be deleted. Profile names are case-sensitive. This string must be NULL-terminated.</param>
        /// <param name="pReserved">Reserved for future use. Must be set to NULL.</param>
        /// <returns>
        /// If the function succeeds, the return value is ERROR_SUCCESS.
        /// If the function fails, the return value may be one of the following return codes.
        /// ERROR_INVALID_PARAMETER: The hClientHandle parameter is NULL or not valid, the pInterfaceGuid parameter is NULL, the strProfileName parameter is NULL, or pReserved is not NULL.
        /// ERROR_INVALID_HANDLE: The handle specified in the hClientHandle parameter was not found in the handle table.
        /// ERROR_NOT_FOUND: The wireless profile specified by strProfileName was not found in the profile store.
        /// ERROR_ACCESS_DENIED: The caller does not have sufficient permissions to delete the profile.
        /// RPC_STATUS: Various error codes.
        /// </returns>
        /// <remarks>
        /// The WlanDeleteProfile function deletes a wireless profile for a wireless interface on the local computer. 
        /// All wireless LAN functions require an interface GUID for the wireless interface when performing profile operations.
        ///   When a wireless interface is removed, its state is cleared from Wireless LAN Service (WLANSVC) and no profile operations are possible.
        /// The WlanDeleteProfile function can fail with ERROR_INVALID_PARAMETER if the wireless interface specified in the pInterfaceGuid parameter
        ///   for the wireless LAN profile has been removed from the system (a USB wireless adapter that has been removed, for example).
        /// </remarks>
        [DllImport("Wlanapi", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint WlanDeleteProfile(IntPtr hClientHandle, [MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid, [MarshalAs(UnmanagedType.LPWStr)] string strProfileName, IntPtr pReserved);

        /// <summary>
        /// The WlanReasonCodeToString function retrieves a string that describes a specified reason code.
        /// </summary>
        /// <param name="dwReasonCode">A WLAN_REASON_CODE value of which the string description is requested.</param>
        /// <param name="dwBufferSize">The size of the buffer used to store the string, in WCHAR. If the reason code string is longer than the buffer, it will be truncated and NULL-terminated. If dwBufferSize is larger than the actual amount of memory allocated to pStringBuffer, then an access violation will occur in the calling program.</param>
        /// <param name="pStringBuffer">Pointer to a buffer that will receive the string. The caller must allocate memory to pStringBuffer before calling WlanReasonCodeToString.</param>
        /// <param name="pReserved">Reserved for future use. Must be set to NULL.</param>
        /// <returns>If the function succeeds, the return value is a pointer to a constant string.</returns>
        [DllImport("Wlanapi", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint WlanReasonCodeToString(uint dwReasonCode, uint dwBufferSize, ref StringBuilder pStringBuffer, IntPtr pReserved);

        /// <summary>
        /// The WlanFreeMemory function frees memory. Any memory returned from Native Wifi functions must be freed.
        /// </summary>
        /// <param name="pMemory">Pointer to the memory to be freed.</param>
        /// <remarks>
        /// If pMemory points to memory that has already been freed, an access violation or heap corruption may occur.
        /// </remarks>
        [DllImport("Wlanapi")]
        public static extern void WlanFreeMemory(IntPtr pMemory);

    }
}
