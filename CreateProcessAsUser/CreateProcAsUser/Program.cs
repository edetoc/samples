using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

/*
 * Based on :
 * https://blogs.msdn.microsoft.com/winsdk/2013/04/30/how-to-launch-a-process-interactively-from-a-windows-service/
 * 
 * 
 */


namespace CreateProcAsUser
{
    class Program
    {
        static public IntPtr WTS_CURRENT_SERVER_HANDLE = (IntPtr)0;

        [DllImport("wtsapi32", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        static extern bool WTSEnumerateSessions(IntPtr hServer, int Reserved, uint Version, out IntPtr ppSessionInfo, out int pCount);

        [DllImport("wtsapi32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public extern static void WTSFreeMemory(IntPtr pMemory);

        [DllImport("wtsapi32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public extern static bool WTSQueryUserToken(int SessionId, out IntPtr phToken);

        public enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous,
            SecurityIdentification,
            SecurityImpersonation,
            SecurityDelegation
        }

        public enum TOKEN_TYPE
        {
            TokenPrimary = 1,
            TokenImpersonation
        }

        public enum WTSConnectState
        {
            Active,
            Connected,
            ConnectQuery,
            Shadow,
            Disconnected,
            Idle,
            Listen,
            Reset,
            Down,
            Init
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WTS_SESSION_INFO
        {
            public int SessionId;
            [MarshalAs(UnmanagedType.LPTStr)]
            public String pWinStationName;
            public WTSConnectState State;
        }

        [DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public extern static bool CloseHandle(IntPtr handle);

        public const int LOGON32_LOGON_INTERACTIVE = 2;
        public const int LOGON32_PROVIDER_DEFAULT = 0;

        private static uint STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        private static uint STANDARD_RIGHTS_READ = 0x00020000;
        private static uint TOKEN_ASSIGN_PRIMARY = 0x0001;
        private static uint TOKEN_DUPLICATE = 0x0002;
        private static uint TOKEN_IMPERSONATE = 0x0004;
        private static uint TOKEN_QUERY = 0x0008;
        private static uint TOKEN_QUERY_SOURCE = 0x0010;
        private static uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
        private static uint TOKEN_ADJUST_GROUPS = 0x0040;
        private static uint TOKEN_ADJUST_DEFAULT = 0x0080;
        private static uint TOKEN_ADJUST_SESSIONID = 0x0100;
        private static uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);
        private static uint TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY | TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE | TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT | TOKEN_ADJUST_SESSIONID);


        // new Process priority class
        private const uint NORMAL_PRIORITY_CLASS = 0x0020;

        // // dwCreationFlags  ( new process creation flags)
        private const uint CREATE_UNICODE_ENVIRONMENT = 0x00000400;
        private const uint CREATE_BREAKAWAY_FROM_JOB = 0x01000000;
        private const uint CREATE_NO_WINDOW = 0x08000000;


        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public int cb;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpReserved;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpDesktop;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateTokenEx(IntPtr existingToken, uint desiredAccess, IntPtr tokenAttributes, SECURITY_IMPERSONATION_LEVEL impersonationLevel, TOKEN_TYPE tokenType, out IntPtr newToken);

        [DllImport("advapi32.dll", EntryPoint = "CreateProcessAsUserA", SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public extern static bool CreateProcessAsUser(IntPtr hToken, [MarshalAs(UnmanagedType.LPStr)] string lpApplicationName, [MarshalAs(UnmanagedType.LPStr)] string lpCommandLine, IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes, bool bInheritHandle, uint dwCreationFlags, IntPtr lpEnvironment,
            [MarshalAs(UnmanagedType.LPStr)] string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);


        [DllImport("userenv.dll", SetLastError = true)]
        internal static extern bool CreateEnvironmentBlock(out IntPtr lpEnvironment, IntPtr hToken, bool bInherit);

        [DllImport("userenv.dll", SetLastError = true)]
        internal static extern bool DestroyEnvironmentBlock(IntPtr lpEnvironment);

        static void LaunchProcess()
        {

            IntPtr ppBuffer = IntPtr.Zero;

            int count;

            // Enumerate the sessions
            if (!WTSEnumerateSessions(WTS_CURRENT_SERVER_HANDLE, 0, 1, out ppBuffer, out count))
            {
                Console.WriteLine("WTSEnumerateSessions failed with " + Marshal.GetLastWin32Error().ToString());
                return;
            }

            WTS_SESSION_INFO wsi = new WTS_SESSION_INFO();

            UInt32 StructSize = (UInt32)Marshal.SizeOf(typeof(WTS_SESSION_INFO));
            IntPtr CurrentStruct;
            IntPtr currentToken = IntPtr.Zero;
            IntPtr primaryToken = IntPtr.Zero;

            for (int i = 0; i < count; i++)
            {
                CurrentStruct = (IntPtr)(ppBuffer.ToInt32() + (StructSize * i));
                wsi = (WTS_SESSION_INFO)(Marshal.PtrToStructure(CurrentStruct, typeof(WTS_SESSION_INFO)));
                Console.WriteLine("Session: " + wsi.SessionId.ToString());
                Console.WriteLine("State:   " + wsi.State.ToString());

                if (wsi.State == WTSConnectState.Active)  // Find the interactive session
                {
                    if (!WTSQueryUserToken(wsi.SessionId, out currentToken))
                    {
                        Console.WriteLine("WTSQueryUserToken failed with " + Marshal.GetLastWin32Error().ToString());
                        return;
                    }

                    // Convert the impersonation token into a primary token. https://docs.microsoft.com/en-us/windows/win32/api/securitybaseapi/nf-securitybaseapi-duplicatetokenex
                    if (!DuplicateTokenEx(currentToken, TOKEN_ASSIGN_PRIMARY | TOKEN_ALL_ACCESS, IntPtr.Zero, SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, TOKEN_TYPE.TokenPrimary, out primaryToken))
                    {
                        Console.WriteLine("DuplicateTokenEx failed with " + Marshal.GetLastWin32Error().ToString());
                        return;
                    }
                   
                    CloseHandle(currentToken);

                    bool ret;

                    STARTUPINFO si = new STARTUPINFO();
                    si.cb = Marshal.SizeOf(si);
                    si.lpDesktop = "winsta0\\default";

                    PROCESS_INFORMATION pi = new PROCESS_INFORMATION();

                    uint dwCreationFlags = CREATE_BREAKAWAY_FROM_JOB | CREATE_NO_WINDOW | NORMAL_PRIORITY_CLASS;
                    IntPtr UserEnvironment = IntPtr.Zero;

                    bool inherit = true;
                    if (CreateEnvironmentBlock(out UserEnvironment, primaryToken, inherit))
                    {
                        Console.WriteLine("Unable to create user's enviroment block " + Marshal.GetLastWin32Error());
                        UserEnvironment = IntPtr.Zero;
                    }
                    else
                    {
                        dwCreationFlags |= CREATE_UNICODE_ENVIRONMENT;
                    }

                    ret = CreateProcessAsUser(primaryToken, null, "notepad.exe", IntPtr.Zero, IntPtr.Zero, true, dwCreationFlags, UserEnvironment, null, ref si, out pi);
                    if (ret == false)
                        Console.WriteLine("CreateProcessAsUser failed with " + Marshal.GetLastWin32Error().ToString());
                    else
                    {
                        Console.WriteLine("CreateProcessAsUser SUCCESS.  The child PID is " + pi.dwProcessId.ToString());

                        //
                        // close Handles
                        //
                        CloseHandle(pi.hProcess);
                        CloseHandle(pi.hThread);
                    }

                    if (UserEnvironment != IntPtr.Zero)
                    {
                        DestroyEnvironmentBlock(UserEnvironment);
                    }

                    CloseHandle(primaryToken);
                }
            }

            WTSFreeMemory(ppBuffer);
        }
        static void Main(string[] args)
        {

            LaunchProcess();
        }
    }
}
