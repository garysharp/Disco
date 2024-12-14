using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.IO;
using System.Web.Mvc;

namespace Disco.Services.Logging
{
    public static class Utilities
    {

        public static List<SelectListItem> ToSelectListItems(this List<Models.LogEventType> items)
        {
            return items.Select(et => new SelectListItem() { Value = et.Id.ToString(), Text = et.Name }).ToList();
        }

        #region Win32 APIs
        /// <summary>
        /// The CreateFile function creates or opens a file, file stream, directory, physical disk, volume, console buffer, tape drive,
        /// communications resource, mailslot, or named pipe. The function returns a handle that can be used to access an object.
        /// </summary>
        /// <param name="lpFileName"></param>
        /// <param name="dwDesiredAccess"> access to the object, which can be read, write, or both</param>
        /// <param name="dwShareMode">The sharing mode of an object, which can be read, write, both, or none</param>
        /// <param name="SecurityAttributes">A pointer to a SECURITY_ATTRIBUTES structure that determines whether or not the returned handle can 
        /// be inherited by child processes. Can be null</param>
        /// <param name="dwCreationDisposition">An action to take on files that exist and do not exist</param>
        /// <param name="dwFlagsAndAttributes">The file attributes and flags. </param>
        /// <param name="hTemplateFile">A handle to a template file with the GENERIC_READ access right. The template file supplies file attributes 
        /// and extended attributes for the file that is being created. This parameter can be null</param>
        /// <returns>If the function succeeds, the return value is an open handle to a specified file. If a specified file exists before the function 
        /// all and dwCreationDisposition is CREATE_ALWAYS or OPEN_ALWAYS, a call to GetLastError returns ERROR_ALREADY_EXISTS, even when the function 
        /// succeeds. If a file does not exist before the call, GetLastError returns 0 (zero).
        /// If the function fails, the return value is INVALID_HANDLE_VALUE. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(
              string lpFileName,
              EFileAccess dwDesiredAccess,
              EFileShare dwShareMode,
              IntPtr SecurityAttributes,
              ECreationDisposition dwCreationDisposition,
              EFileAttributes dwFlagsAndAttributes,
              IntPtr hTemplateFile
              );
        [Flags]
        private enum EFileAccess : uint
        {
            Delete = 0x10000,
            ReadControl = 0x20000,
            WriteDAC = 0x40000,
            WriteOwner = 0x80000,
            Synchronize = 0x100000,

            StandardRightsRequired = 0xF0000,
            StandardRightsRead = ReadControl,
            StandardRightsWrite = ReadControl,
            StandardRightsExecute = ReadControl,
            StandardRightsAll = 0x1F0000,
            SpecificRightsAll = 0xFFFF,

            AccessSystemSecurity = 0x1000000,       // AccessSystemAcl access type

            MaximumAllowed = 0x2000000,         // MaximumAllowed access type

            GenericRead = 0x80000000,
            GenericWrite = 0x40000000,
            GenericExecute = 0x20000000,
            GenericAll = 0x10000000
        }
        [Flags]
        private enum EFileShare : uint
        {
            /// <summary>
            /// 
            /// </summary>
            None = 0x00000000,
            /// <summary>
            /// Enables subsequent open operations on an object to request read access. 
            /// Otherwise, other processes cannot open the object if they request read access. 
            /// If this flag is not specified, but the object has been opened for read access, the function fails.
            /// </summary>
            Read = 0x00000001,
            /// <summary>
            /// Enables subsequent open operations on an object to request write access. 
            /// Otherwise, other processes cannot open the object if they request write access. 
            /// If this flag is not specified, but the object has been opened for write access, the function fails.
            /// </summary>
            Write = 0x00000002,
            /// <summary>
            /// Enables subsequent open operations on an object to request delete access. 
            /// Otherwise, other processes cannot open the object if they request delete access.
            /// If this flag is not specified, but the object has been opened for delete access, the function fails.
            /// </summary>
            Delete = 0x00000004
        }
        private enum ECreationDisposition : uint
        {
            /// <summary>
            /// Creates a new file. The function fails if a specified file exists.
            /// </summary>
            New = 1,
            /// <summary>
            /// Creates a new file, always. 
            /// If a file exists, the function overwrites the file, clears the existing attributes, combines the specified file attributes, 
            /// and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security descriptor that the SECURITY_ATTRIBUTES structure specifies.
            /// </summary>
            CreateAlways = 2,
            /// <summary>
            /// Opens a file. The function fails if the file does not exist. 
            /// </summary>
            OpenExisting = 3,
            /// <summary>
            /// Opens a file, always. 
            /// If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
            /// </summary>
            OpenAlways = 4,
            /// <summary>
            /// Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does not exist.
            /// The calling process must open the file with the GENERIC_WRITE access right. 
            /// </summary>
            TruncateExisting = 5
        }
        [Flags]
        private enum EFileAttributes : uint
        {
            None = 0x0000000,
            Readonly = 0x00000001,
            Hidden = 0x00000002,
            System = 0x00000004,
            Directory = 0x00000010,
            Archive = 0x00000020,
            Device = 0x00000040,
            Normal = 0x00000080,
            Temporary = 0x00000100,
            SparseFile = 0x00000200,
            ReparsePoint = 0x00000400,
            Compressed = 0x00000800,
            Offline = 0x00001000,
            NotContentIndexed = 0x00002000,
            Encrypted = 0x00004000,
            Write_Through = 0x80000000,
            Overlapped = 0x40000000,
            NoBuffering = 0x20000000,
            RandomAccess = 0x10000000,
            SequentialScan = 0x08000000,
            DeleteOnClose = 0x04000000,
            BackupSemantics = 0x02000000,
            PosixSemantics = 0x01000000,
            OpenReparsePoint = 0x00200000,
            OpenNoRecall = 0x00100000,
            FirstPipeInstance = 0x00080000
        }

        private const int FSCTL_SET_COMPRESSION = 0x9C040;
        private const short COMPRESSION_FORMAT_DEFAULT = 1;
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int DeviceIoControl(
            SafeFileHandle hDevice,
            int dwIoControlCode,
            ref short lpInBuffer,
            int nInBufferSize,
            IntPtr lpOutBuffer,
            int nOutBufferSize,
            ref int lpBytesReturned,
            IntPtr lpOverlapped);
        #endregion

        public static void CompressDirectory(string DirectoryPath)
        {
            if (DirectoryPath.Length > 250)
                throw new InvalidOperationException(string.Format("Directory Path to Long (>250) to Compress: {0}", DirectoryPath));

            DirectoryInfo dirInfo = new DirectoryInfo(DirectoryPath);
            if (dirInfo.Exists)
            {
                if ((dirInfo.Attributes & FileAttributes.Compressed) != FileAttributes.Compressed)
                {
                    var dirHandle = CreateFile(DirectoryPath, EFileAccess.GenericWrite, EFileShare.Read, IntPtr.Zero, ECreationDisposition.OpenExisting, EFileAttributes.None, IntPtr.Zero);
                    if (dirHandle.IsInvalid)
                    {
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                    }
                    else
                    {
                        EnableCompression(dirHandle);
                        dirHandle.Close();
                    }
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("Directory doesn't exist: {0}", DirectoryPath));
            }
        }

        private static void EnableCompression(SafeFileHandle handle)
        {
            int lpBytesReturned = 0;
            short lpInBuffer = COMPRESSION_FORMAT_DEFAULT;

            int result = DeviceIoControl(handle, FSCTL_SET_COMPRESSION,
                ref lpInBuffer, sizeof(short), IntPtr.Zero, 0,
                ref lpBytesReturned, IntPtr.Zero);

            if (result != 0)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
            }
        }

    }
}
