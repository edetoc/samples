using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GetToolTipTextFromSystrayIcons
{


    internal class ProcessRights
    {
        public const UInt32 TERMINATE = 0x0001;
        public const UInt32 CREATE_THREAD = 0x0002;
        public const UInt32 SET_SESSIONID = 0x0004;
        public const UInt32 VM_OPERATION = 0x0008;
        public const UInt32 VM_READ = 0x0010;
        public const UInt32 VM_WRITE = 0x0020;
        public const UInt32 DUP_HANDLE = 0x0040;
        public const UInt32 CREATE_PROCESS = 0x0080;
        public const UInt32 SET_QUOTA = 0x0100;
        public const UInt32 SET_INFORMATION = 0x0200;
        public const UInt32 QUERY_INFORMATION = 0x0400;
        public const UInt32 SUSPEND_RESUME = 0x0800;

        private const UInt32 STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        private const UInt32 SYNCHRONIZE = 0x00100000;

        public const UInt32 ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFF;
    }

    internal class MemoryProtection
    {
        public const UInt32 PAGE_NOACCESS = 0x01;
        public const UInt32 PAGE_READONLY = 0x02;
        public const UInt32 PAGE_READWRITE = 0x04;
        public const UInt32 PAGE_WRITECOPY = 0x08;
        public const UInt32 PAGE_EXECUTE = 0x10;
        public const UInt32 PAGE_EXECUTE_READ = 0x20;
        public const UInt32 PAGE_EXECUTE_READWRITE = 0x40;
        public const UInt32 PAGE_EXECUTE_WRITECOPY = 0x80;
        public const UInt32 PAGE_GUARD = 0x100;
        public const UInt32 PAGE_NOCACHE = 0x200;
        public const UInt32 PAGE_WRITECOMBINE = 0x400;
    }

    internal class MemAllocationType
    {
        public const UInt32 COMMIT = 0x1000;
        public const UInt32 RESERVE = 0x2000;
        public const UInt32 DECOMMIT = 0x4000;
        public const UInt32 RELEASE = 0x8000;
        public const UInt32 FREE = 0x10000;
        public const UInt32 PRIVATE = 0x20000;
        public const UInt32 MAPPED = 0x40000;
        public const UInt32 RESET = 0x80000;
        public const UInt32 TOP_DOWN = 0x100000;
        public const UInt32 WRITE_WATCH = 0x200000;
        public const UInt32 PHYSICAL = 0x400000;
        public const UInt32 LARGE_PAGES = 0x20000000;
        public const UInt32 FOURMB_PAGES = 0x80000000;
    }
    internal class Kernel32
    {

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(
            uint dwDesiredAccess,
            bool bInheritHandle,
            uint dwProcessId);


        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            UIntPtr dwSize,
            uint flAllocationType,
            uint flProtect);

        [DllImport("kernel32.dll")]
        public static extern bool VirtualFreeEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            UIntPtr dwSize,
            UInt32 dwFreeType);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            UIntPtr nSize,
            IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(
            IntPtr hObject);


    }
}
