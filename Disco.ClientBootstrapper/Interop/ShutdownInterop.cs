using System;
using System.Runtime.InteropServices;

namespace Disco.ClientBootstrapper.Interop
{
    public static class ShutdownInterop
    {
        public static void Shutdown()
        {
            // 8 = Power Off
            Shutdown(EWX_POWEROFF);
        }
        public static void Reboot()
        {
            // 2 = Reboot
            Shutdown(EWX_REBOOT);
        }

        private static void Shutdown(int flag)
        {
            // Removed 2012-11-23 G# - Migrate to Win32 PInvoke Shutdown for better Privilege Handling
            //ManagementBaseObject mboShutdown = null;
            //ManagementClass mcWin32 = new ManagementClass("Win32_OperatingSystem");
            //mcWin32.Get();

            //// You can't shutdown without security privileges 
            //mcWin32.Scope.Options.EnablePrivileges = true;
            //ManagementBaseObject mboShutdownParams =
            //         mcWin32.GetMethodParameters("Win32Shutdown");

            //// Flag 1 means we want to shut down the system. Use "2" to reboot. 
            //mboShutdownParams["Flags"] = flag;
            //mboShutdownParams["Reserved"] = "0";
            //foreach (ManagementObject manObj in mcWin32.GetInstances())
            //{
            //    mboShutdown = manObj.InvokeMethod("Win32Shutdown",
            //                                   mboShutdownParams, null);
            //}
            // End Removed 2012-11-23 G#

            // Added 2012-11-23 G# - Migrate to Win32 PInvoke Shutdown
            TokPriv1Luid tp;

            IntPtr hproc = GetCurrentProcess();
            IntPtr htok = IntPtr.Zero;

            OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);

            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = SE_PRIVILEGE_ENABLED;

            LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref tp.Luid);
            AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
            ExitWindowsEx(flag, 0);
            // End Added 2012-11-23 G#
        }

        #region Win32 PInvoke Interop

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool ExitWindowsEx(int flg, int rea);

        private const int SE_PRIVILEGE_ENABLED = 0x00000002;
        private const int TOKEN_QUERY = 0x00000008;
        private const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        private const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        private const int EWX_LOGOFF = 0x00000000;
        private const int EWX_SHUTDOWN = 0x00000001;
        private const int EWX_REBOOT = 0x00000002;
        private const int EWX_FORCE = 0x00000004;
        private const int EWX_POWEROFF = 0x00000008;
        private const int EWX_FORCEIFHUNG = 0x00000010;

        #endregion

    }
}
