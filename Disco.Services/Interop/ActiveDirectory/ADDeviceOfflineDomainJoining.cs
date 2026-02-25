using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Disco.Services.Interop.ActiveDirectory
{
    public static class ADDeviceOfflineDomainJoining
    {
        [Flags]
        private enum NETSETUP_PROVISION_FLAGS : int
        {
            NETSETUP_PROVISION_DOWNLEVEL_PRIV_SUPPORT = 0x00000001,
            NETSETUP_PROVISION_REUSE_ACCOUNT = 0x00000002,
            NETSETUP_PROVISION_USE_DEFAULT_PASSWORD = 0x00000004,
            NETSETUP_PROVISION_SKIP_ACCOUNT_SEARCH = 0x00000008,
            NETSETUP_PROVISION_ROOT_CA_CERTS = 0x00000010,
            NETSETUP_PROVISION_PERSISTENTSITE = 0x00000020,
            NETSETUP_PROVISION_ONLINE_CALLER = 0x40000000,
            NETSETUP_PROVISION_CHECK_PWD_ONLY = unchecked((int)0x80000000),
        }

        [DllImport("Netapi32.dll", CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.I4)]
        private static extern int NetProvisionComputerAccount(
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpDomain,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpMachineName,
            [In, Optional, MarshalAs(UnmanagedType.LPWStr)] string lpMachineAccountOU,
            [In, Optional, MarshalAs(UnmanagedType.LPWStr)] string lpDcName,
            [In, MarshalAs(UnmanagedType.I4)] NETSETUP_PROVISION_FLAGS dwOptions,
            out IntPtr pProvisionBinData,
            out int pdwProvisionBinDataSize,
            [Optional] IntPtr pProvisionTextData
            );

        [DllImport("Netapi32.dll", SetLastError = true)]
        private static extern int NetApiBufferFree(IntPtr Buffer);

        public static string OfflineDomainJoinProvision(this ADDomainController dc, string computerSamAccountName, string organisationalUnit, ref ADMachineAccount machineAccount)
        {
            if (machineAccount != null && machineAccount.IsCriticalSystemObject)
                throw new InvalidOperationException($"This account {machineAccount.DistinguishedName} is a Critical System Active Directory Object and Disco ICT refuses to modify it");

            if (!dc.IsWritable)
                throw new InvalidOperationException($"The domain controller [{dc.Name}] is not writable. This action (Offline Domain Join Provision) requires a writable domain controller.");

            if (!string.IsNullOrWhiteSpace(computerSamAccountName))
                computerSamAccountName = computerSamAccountName.TrimEnd('$');
            if (!string.IsNullOrWhiteSpace(computerSamAccountName) && computerSamAccountName.Contains('\\'))
                computerSamAccountName = computerSamAccountName.Substring(computerSamAccountName.IndexOf('\\') + 1);

            // NetBIOS Limit (16 characters; "{ComputerName}$"; 15 characters allowed)
            if (string.IsNullOrWhiteSpace(computerSamAccountName) || computerSamAccountName.Length > 15)
                throw new ArgumentException("Invalid Computer Name; > 0 and <= 15", "ComputerName");

            // Ensure Specified OU Exists
            if (!string.IsNullOrEmpty(organisationalUnit))
            {
                try
                {
                    using (var deOU = dc.RetrieveDirectoryEntry(organisationalUnit, new string[] { "distinguishedName" }))
                    {
                        if (deOU == null)
                            throw new Exception($"OU's Directory Entry couldn't be found at [{organisationalUnit}]");
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"An error occurred while trying to locate the specified OU: {organisationalUnit}", "OrganisationalUnit", ex);
                }
            }

            if (machineAccount != null && !string.Equals(machineAccount.Name, computerSamAccountName, StringComparison.Ordinal))
            {
                // rename the account
                machineAccount.RenameAccount(dc, computerSamAccountName);
            }

            var result = NetProvisionComputerAccount(dc.Domain.Name, computerSamAccountName, string.IsNullOrWhiteSpace(organisationalUnit) ? null : organisationalUnit, dc.Name, NETSETUP_PROVISION_FLAGS.NETSETUP_PROVISION_REUSE_ACCOUNT, out var provisionDataPointer, out var provisionDataLength);

            if (result != 0)
            {
                var win32Exception = new System.ComponentModel.Win32Exception(result);
                throw new InvalidOperationException($"NetProvisionComputerAccount failed with error code {result}: {win32Exception.Message}");
            }

            if (provisionDataPointer == IntPtr.Zero || provisionDataLength == 0)
                throw new InvalidOperationException("NetProvisionComputerAccount did not return valid provisioning data.");

            var buffer = new byte[provisionDataLength];
            Marshal.Copy(provisionDataPointer, buffer, 0, provisionDataLength);
            var freeResult = NetApiBufferFree(provisionDataPointer);
            var encodedResult = Convert.ToBase64String(buffer);

            // Reload Machine Account
            machineAccount = dc.RetrieveADMachineAccount($@"{dc.Domain.NetBiosName}\{computerSamAccountName}", (machineAccount == null ? null : machineAccount.LoadedProperties.Keys.ToArray()));

            return encodedResult;
        }
    }
}
