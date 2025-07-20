using System;
using System.Runtime.InteropServices;

namespace Disco.ClientBootstrapper.Interop
{
    class RegistryInterop : IDisposable
    {

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID
        {
            public int LowPart;
            public int HighPart;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_PRIVILEGES
        {
            public LUID Luid;
            public int Attributes;
            public int PrivilegeCount;
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        private static extern int OpenProcessToken(int ProcessHandle, int DesiredAccess, ref int tokenhandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetCurrentProcess();

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        private static extern int LookupPrivilegeValue(string lpsystemname, string lpname, [MarshalAs(UnmanagedType.Struct)] ref LUID lpLuid);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        private static extern int AdjustTokenPrivileges(int tokenhandle, int disableprivs, [MarshalAs(UnmanagedType.Struct)] ref TOKEN_PRIVILEGES Newstate, int bufferlength, int PreivousState, int Returnlength);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int RegLoadKey(uint hKey, string lpSubKey, string lpFile);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int RegUnLoadKey(uint hKey, string lpSubKey);

        private const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        private const int TOKEN_QUERY = 0x00000008;
        private const int SE_PRIVILEGE_ENABLED = 0x00000002;
        private const string SE_RESTORE_NAME = "SeRestorePrivilege";
        private const string SE_BACKUP_NAME = "SeBackupPrivilege";
        private const uint HKEY_USERS = 0x80000003;

        private RegistryHives Hive { get; set; }
        private string SubKey { get; set; }
        private bool IsUnloaded { get; set; }

        public enum RegistryHives : uint
        {
            HKEY_USERS = 0x80000003,
            HKEY_LOCAL_MACHINE = 0x80000002
        }

        public RegistryInterop(RegistryHives hive, string subKey, string filePath)
        {
            int token = 0;
            int retval = 0;

            TOKEN_PRIVILEGES TP = new TOKEN_PRIVILEGES();
            TOKEN_PRIVILEGES TP2 = new TOKEN_PRIVILEGES();
            LUID RestoreLuid = new LUID();
            LUID BackupLuid = new LUID();

            retval = OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref token);
            retval = LookupPrivilegeValue(null, SE_RESTORE_NAME, ref RestoreLuid);
            retval = LookupPrivilegeValue(null, SE_BACKUP_NAME, ref BackupLuid);
            TP.PrivilegeCount = 1;
            TP.Attributes = SE_PRIVILEGE_ENABLED;
            TP.Luid = RestoreLuid;
            TP2.PrivilegeCount = 1;
            TP2.Attributes = SE_PRIVILEGE_ENABLED;
            TP2.Luid = BackupLuid;

            retval = AdjustTokenPrivileges(token, 0, ref TP, 1024, 0, 0);
            retval = AdjustTokenPrivileges(token, 0, ref TP2, 1024, 0, 0);

            uint regHive = (uint)hive;

            Hive = hive;
            SubKey = subKey;
            RegLoadKey(regHive, subKey, filePath);
            IsUnloaded = false;
        }

        public void Unload()
        {
            if (!IsUnloaded)
            {
                uint regHive = (uint)Hive;
                string subKey = SubKey;
                RegUnLoadKey(regHive, subKey);
                IsUnloaded = true;
            }
        }

        public void Dispose()
        {
            Unload();
        }
    }
}
