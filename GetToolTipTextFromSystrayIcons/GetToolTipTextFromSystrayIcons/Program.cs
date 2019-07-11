using System;
using System.Runtime.InteropServices;

// Sample based on below URLs:
// http://stackoverflow.com/questions/6366505/get-tooltip-text-from-icon-in-system-tray
// http://www.codeproject.com/Articles/10497/A-tool-to-order-the-window-buttons-in-your-taskbar

namespace GetToolTipTextFromSystrayIcons
{
   
    class Program
    {

        static void Main(string[] args)
        {

            // Get Visible icons in systray

            Console.WriteLine("Visible icons:");

            IntPtr _ToolbarWindowHandle = GetSystemTrayHandle();

            // iterate through buttons and get data
            UInt32 count = User32.SendMessage(_ToolbarWindowHandle, TB.BUTTONCOUNT, 0 , 0 );
           
            for (int i = 0; i < count; i++)
            {
                TBBUTTON tbButton = new TBBUTTON();
                string text = String.Empty;
                IntPtr ipWindowHandle = IntPtr.Zero;

                bool b = GetTBButton(_ToolbarWindowHandle, i, ref tbButton, ref text, ref ipWindowHandle);
                if(!string.IsNullOrEmpty (text))
                {
                    Console.WriteLine(text);
                }

            }


            Console.WriteLine();
            Console.WriteLine();

            // Get non visible icons in systray

            Console.WriteLine("Non visible icons:");

            _ToolbarWindowHandle = GetSystemTrayHandle2();

            // iterate through buttons and get data
            count = User32.SendMessage(_ToolbarWindowHandle, TB.BUTTONCOUNT, 0, 0);

            for (int i = 0; i < count; i++)
            {
                TBBUTTON tbButton = new TBBUTTON();
                string text = String.Empty;
                IntPtr ipWindowHandle = IntPtr.Zero;

                bool b = GetTBButton(_ToolbarWindowHandle, i, ref tbButton, ref text, ref ipWindowHandle);
                if (!string.IsNullOrEmpty(text))
                {
                    Console.WriteLine(text);
                }

            }

        }

        //find systray window handle (visible icons)
        static IntPtr GetSystemTrayHandle()
        {
            IntPtr hWndTray = User32.FindWindow("Shell_TrayWnd", null);
            if (hWndTray != IntPtr.Zero)
            {
                hWndTray = User32.FindWindowEx(hWndTray, IntPtr.Zero, "TrayNotifyWnd", null);
                if (hWndTray != IntPtr.Zero)
                {
                    hWndTray = User32.FindWindowEx(hWndTray, IntPtr.Zero, "SysPager", null);
                    if (hWndTray != IntPtr.Zero)
                    {
                        hWndTray = User32.FindWindowEx(hWndTray, IntPtr.Zero, "ToolbarWindow32", null);
                        return hWndTray;
                    }
                }
            }

            return IntPtr.Zero;
        }

         //find systray window handle (non visible icons)
        static IntPtr GetSystemTrayHandle2()
        {
            IntPtr hWndTray = User32.FindWindow("NotifyIconOverflowWindow", null);
            if (hWndTray != IntPtr.Zero)
            {
                hWndTray = User32.FindWindowEx(hWndTray, IntPtr.Zero, "ToolbarWindow32", null);
                if (hWndTray != IntPtr.Zero)
                {
                    return hWndTray;
                   
                }
            }

            return IntPtr.Zero;
        }


        // Systray window is a toolbar class, you need to get information for a single icon        
        private static unsafe bool GetTBButton(IntPtr hToolbar, int i, ref TBBUTTON tbButton, ref string text, ref IntPtr ipWindowHandle)
        {
            // One page
            const int BUFFER_SIZE = 0x1000;

            byte[] localBuffer = new byte[BUFFER_SIZE];

            UInt32 processId = 0;
            UInt32 threadId = User32.GetWindowThreadProcessId(hToolbar, out processId);

            IntPtr hProcess = Kernel32.OpenProcess(ProcessRights.ALL_ACCESS, false, processId);
            if (hProcess == IntPtr.Zero) { 
               // Debug.Assert(false); 
                  return false; 
            }

            IntPtr ipRemoteBuffer = Kernel32.VirtualAllocEx(
                hProcess,
                IntPtr.Zero,
                new UIntPtr(BUFFER_SIZE),
                MemAllocationType.COMMIT,
                MemoryProtection.PAGE_READWRITE);

            if (ipRemoteBuffer == IntPtr.Zero) {
                //Debug.Assert(false); 
                return false; 
            }

            // TBButton
            fixed (TBBUTTON* pTBButton = &tbButton)
            {
                IntPtr ipTBButton = new IntPtr(pTBButton);

                int b = (int)User32.SendMessage(hToolbar, TB.GETBUTTON, (IntPtr)i, ipRemoteBuffer);
                if (b == 0) { 
                    //Debug.Assert(false); 
                    return false;
                }

                // this is fixed
                Int32 dwBytesRead = 0;
                IntPtr ipBytesRead = new IntPtr(&dwBytesRead);

                bool b2 = Kernel32.ReadProcessMemory(
                    hProcess,
                    ipRemoteBuffer,
                    ipTBButton,
                    new UIntPtr((uint)sizeof(TBBUTTON)),
                    ipBytesRead);

                if (!b2) { 
                    //Debug.Assert(false); 
                    return false; 
                }
            }

            // button text
            fixed (byte* pLocalBuffer = localBuffer)
            {
                IntPtr ipLocalBuffer = new IntPtr(pLocalBuffer);

                int chars = (int)User32.SendMessage(hToolbar, TB.GETBUTTONTEXTW, (IntPtr)tbButton.idCommand, ipRemoteBuffer);
                if (chars == -1) {
                    // Debug.Assert(false); 
                    return false; 
                }

                // this is fixed
                Int32 dwBytesRead = 0;
                IntPtr ipBytesRead = new IntPtr(&dwBytesRead);

                bool b4 = Kernel32.ReadProcessMemory(
                    hProcess,
                    ipRemoteBuffer,
                    ipLocalBuffer,
                    new UIntPtr(BUFFER_SIZE),
                    ipBytesRead);

                if (!b4) { 
                    //Debug.Assert(false);
                    return false; 
                }

                text = Marshal.PtrToStringUni(ipLocalBuffer, chars);

                if (text == " ") text = String.Empty;
            }

            Kernel32.VirtualFreeEx(
                hProcess,
                ipRemoteBuffer,
                UIntPtr.Zero,
                MemAllocationType.RELEASE);

            Kernel32.CloseHandle(hProcess);

            return true;
        }
        
   
    }
}
