namespace Disco.Client.Interop.Native
{
    /// <summary>
    /// The WLAN_INTERFACE_STATE enumerated type indicates the state of an interface.
    /// Windows XP with SP3 and Wireless LAN API for Windows XP with SP2:  Only the wlan_interface_state_connected,
    ///   wlan_interface_state_disconnected, and wlan_interface_state_authenticating values are supported.
    /// </summary>
    public enum WLAN_INTERFACE_STATE
    {
        /// <summary>
        /// The interface is not ready to operate.
        /// </summary>
        wlan_interface_state_not_ready = 0,
        /// <summary>
        /// The interface is connected to a network.
        /// </summary>
        wlan_interface_state_connected = 1,
        /// <summary>
        /// The interface is the first node in an ad hoc network. No peer has connected.
        /// </summary>
        wlan_interface_state_ad_hoc_network_formed = 2,
        /// <summary>
        /// The interface is disconnecting from the current network.
        /// </summary>
        wlan_interface_state_disconnecting = 3,
        /// <summary>
        /// The interface is not connected to any network.
        /// </summary>
        wlan_interface_state_disconnected = 4,
        /// <summary>
        /// The interface is attempting to associate with a network.
        /// </summary>
        wlan_interface_state_associating = 5,
        /// <summary>
        /// Auto configuration is discovering the settings for the network.
        /// </summary>
        wlan_interface_state_discovering = 6,
        /// <summary>
        /// The interface is in the process of authenticating.
        /// </summary>
        wlan_interface_state_authenticating = 7,
    }
}
