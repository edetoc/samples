

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;

namespace CreateProcessSample
{
    // Below code is inspired from :

    // Creating a Child Process with Redirected Input and Output
    // https://msdn.microsoft.com/en-us/library/windows/desktop/ms682499(v=vs.85).aspx


    class Win32
    {
        // pipe for STDOUT
        public static IntPtr g_hChildStd_OUT_Rd = IntPtr.Zero;
        public static IntPtr g_hChildStd_OUT_Wr = IntPtr.Zero;

        // pipe for STDERR
        public static IntPtr g_hChildStd_ERR_Rd = IntPtr.Zero;
        public static IntPtr g_hChildStd_ERR_Wr = IntPtr.Zero;


        #region "CONTS"

        const UInt32 INFINITE = 0xFFFFFFFF;
        const UInt32 WAIT_FAILED = 0xFFFFFFFF;
        public const int STARTF_USESTDHANDLES = 0x100;
        const uint BUFSIZE = 256;

        #endregion

        #region "ENUMS"

        [Flags]
        public enum LogonType
        {
            LOGON32_LOGON_INTERACTIVE = 2,
            LOGON32_LOGON_NETWORK = 3,
            LOGON32_LOGON_BATCH = 4,
            LOGON32_LOGON_SERVICE = 5,
            LOGON32_LOGON_UNLOCK = 7,
            LOGON32_LOGON_NETWORK_CLEARTEXT = 8,
            LOGON32_LOGON_NEW_CREDENTIALS = 9
        }

        [Flags]
        public enum LogonProvider
        {
            LOGON32_PROVIDER_DEFAULT = 0,
            LOGON32_PROVIDER_WINNT35,
            LOGON32_PROVIDER_WINNT40,
            LOGON32_PROVIDER_WINNT50
        }

        #endregion

        #region "STRUCTS"

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public Int32 cb;
            public String lpReserved;
            public String lpDesktop;
            public String lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
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
            public Int32 dwProcessId;
            public Int32 dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public Int32 nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        #endregion

        #region "FUNCTIONS (P/INVOKE)"

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern Boolean LogonUser
        (
            String lpszUserName,
            String lpszDomain,
            String lpszPassword,
            LogonType dwLogonType,
            LogonProvider dwLogonProvider,
            out IntPtr phToken
        );

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Boolean CreateProcessAsUser
        (
            IntPtr hToken,
            String lpApplicationName,
            String lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            Boolean bInheritHandles,
            Int32 dwCreationFlags,
            IntPtr lpEnvironment,
            String lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation
        );

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Boolean CreateProcessWithLogonW
        (
            String lpszUsername,
            String lpszDomain,
            String lpszPassword,
            Int32 dwLogonFlags,
            String applicationName,
            String commandLine,
            Int32 creationFlags,
            IntPtr environment,
            String currentDirectory,
            ref STARTUPINFO sui,
            out PROCESS_INFORMATION processInfo
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern UInt32 WaitForSingleObject
        (
            IntPtr hHandle,
            UInt32 dwMilliseconds
        );

        [DllImport("kernel32", SetLastError = true)]
        public static extern Boolean CloseHandle(IntPtr handle);


        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CreatePipe
        (
            out IntPtr hReadPipe,
            out IntPtr hWritePipe,
            ref SECURITY_ATTRIBUTES lpPipeAttributes,
            uint nSize
        );


        enum HANDLE_FLAGS : uint
        {
            None = 0,
            INHERIT = 1,
            PROTECT_FROM_CLOSE = 2
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetHandleInformation
        (
            IntPtr hObject, 
            HANDLE_FLAGS dwMask,
            HANDLE_FLAGS dwFlags
        );


        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadFile
        (
            IntPtr hFile,
            [Out] byte[] lpBuffer,
            uint nNumberOfBytesToRead, 
            out uint lpNumberOfBytesRead, 
            IntPtr lpOverlapped
        );

        #endregion

        #region "FUNCTIONS"

        public static void LaunchCommand1(string strCommand, string strDomain, string strName, string strPassword)
        {
            // Variables
            PROCESS_INFORMATION processInfo = new PROCESS_INFORMATION();
            STARTUPINFO startInfo = new STARTUPINFO();
            bool bResult = false;
            UInt32 uiResultWait = WAIT_FAILED;

            try
            {
                // Create process
                startInfo.cb = Marshal.SizeOf(startInfo);

                bResult = CreateProcessWithLogonW(
                    strName,
                    strDomain,
                    strPassword,
                    // 0,    (orig)
                    0x2, // (helge)
                    null,
                    strCommand,
                    0,
                    IntPtr.Zero,
                    null,
                    ref startInfo,
                    out processInfo
                );
                if (!bResult) { throw new Exception("CreateProcessWithLogonW error #" + Marshal.GetLastWin32Error().ToString()); }

                // Wait for process to end
                uiResultWait = WaitForSingleObject(processInfo.hProcess, INFINITE);
                if (uiResultWait == WAIT_FAILED) { throw new Exception("WaitForSingleObject error #" + Marshal.GetLastWin32Error()); }

            }
            finally
            {
                // Close all handles
                CloseHandle(processInfo.hProcess);
                CloseHandle(processInfo.hThread);
            }
        }

        public static void LaunchCommand2(string strCommand, string strDomain, string strName, string strPassword)
        {
          
            SECURITY_ATTRIBUTES saAttr = new SECURITY_ATTRIBUTES();

            // Set the bInheritHandle flag so pipe handles are inherited. 
                  
            //saAttr.nLength = Marshal.SizeOf(SECURITY_ATTRIBUTES);
            saAttr.bInheritHandle = true;
            saAttr.lpSecurityDescriptor = IntPtr.Zero;

            // Create a pipe for the child process's STDOUT. 
            if (!CreatePipe(out g_hChildStd_OUT_Rd,  out g_hChildStd_OUT_Wr, ref saAttr, 0))
               throw new Exception ();

            // Ensure the read handle to the pipe for STDOUT is not inherited.
            if (!SetHandleInformation(g_hChildStd_OUT_Rd, HANDLE_FLAGS.INHERIT , HANDLE_FLAGS.None ))
                throw new Exception();               

            // Create a pipe for the child process's STDERR. 
            if (!CreatePipe(out g_hChildStd_ERR_Rd, out g_hChildStd_ERR_Wr, ref saAttr, 0))
                throw new Exception();

            // Ensure the read handle to the pipe for STDERR is not inherited.
            if (!SetHandleInformation(g_hChildStd_ERR_Rd, HANDLE_FLAGS.INHERIT, HANDLE_FLAGS.None))
                throw new Exception();
            
            // Variables
            PROCESS_INFORMATION processInfo = new PROCESS_INFORMATION();
            STARTUPINFO startInfo = new STARTUPINFO();
            Boolean bResult = false;
            IntPtr hToken = IntPtr.Zero;           

            try
            {
                // Logon user
                bResult = Win32.LogonUser(
                    strName,
                    strDomain,
                    strPassword,
                    Win32.LogonType.LOGON32_LOGON_INTERACTIVE,
                    Win32.LogonProvider.LOGON32_PROVIDER_DEFAULT,
                    out hToken
                );
                if (!bResult) { throw new Exception("Logon error #" + Marshal.GetLastWin32Error()); }

                // Create process
                startInfo.cb = Marshal.SizeOf(startInfo);
                startInfo.hStdOutput = g_hChildStd_OUT_Wr;
                startInfo.hStdError = g_hChildStd_ERR_Wr;
                startInfo.dwFlags |= STARTF_USESTDHANDLES;

                // (eric commented) startInfo.lpDesktop = "winsta0\\default";

                bResult = Win32.CreateProcessAsUser(
                    hToken,
                    null,
                    strCommand,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    true,          // We want to inherit handles
                    0,
                    IntPtr.Zero,
                    null,
                    ref startInfo,
                    out processInfo
                );
                if (!bResult) 
                {
                    throw new Exception("CreateProcessAsUser error #" + Marshal.GetLastWin32Error());
                }
                else
                {
                    // Close handles to the child process and its primary thread.
                    // Some applications might keep these handles to monitor the status
                    // of the child process, for example. 

                    CloseHandle(processInfo.hProcess );
                    CloseHandle(processInfo.hThread);

                    // Let's also close the pipes handles that parent process doesn't need
                    // See in the Comments part (mrCracker) of https://msdn.microsoft.com/en-us/library/windows/desktop/ms682499(v=vs.85).aspx
                    CloseHandle(g_hChildStd_OUT_Wr);   
                    CloseHandle(g_hChildStd_ERR_Wr);  
                }


                Console.WriteLine(); Console.WriteLine("-> Contents of child process STDERR:"); Console.WriteLine();

                ReadFromPipe(); 
              
            }
            finally
            {
                // Close all handles
                CloseHandle(hToken);
                CloseHandle(processInfo.hProcess);
                CloseHandle(processInfo.hThread);
            }
        }

        public static void ReadFromPipe()        
        {
           uint dwRead;             
           byte[] chBuf = new byte[BUFSIZE];
           bool bSuccess = false;

           // Stop when there is no more data. 
            for (; ; )
            {
                // Read output from the child process's pipe for STDERR               
                bSuccess = ReadFile(g_hChildStd_ERR_Rd, chBuf, BUFSIZE, out dwRead, IntPtr.Zero);
                
                if (!bSuccess || dwRead == 0) break;
                else
                {
                    var str = Encoding.UTF8.GetString(chBuf, 0, (int)dwRead);
                    Console.WriteLine(str);
                       
                }
                        
            }

        } 

        #endregion
    }
}
