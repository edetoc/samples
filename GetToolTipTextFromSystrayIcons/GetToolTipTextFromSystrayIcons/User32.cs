using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GetToolTipTextFromSystrayIcons
{

    internal class WM
    {
        public const UInt32 CLOSE = 0x0010;
        public const UInt32 GETICON = 0x007F;
        public const UInt32 KEYDOWN = 0x0100;
        public const UInt32 COMMAND = 0x0111;
        public const UInt32 USER = 0x0400; // 0x0400 - 0x7FFF
        public const UInt32 APP = 0x8000; // 0x8000 - 0xBFFF
    }

    internal class TB
    {
        public const UInt32 GETBUTTON = WM.USER + 23;
        public const UInt32 BUTTONCOUNT = WM.USER + 24;
        public const UInt32 CUSTOMIZE = WM.USER + 27;
        public const UInt32 GETBUTTONTEXTA = WM.USER + 45;
        public const UInt32 GETBUTTONTEXTW = WM.USER + 75;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TBBUTTON
    {
        public Int32 iBitmap;
        public Int32 idCommand;
        public byte fsState;
        public byte fsStyle;
        //		[ MarshalAs( UnmanagedType.ByValArray, SizeConst=2 ) ]
        //		public byte[] bReserved;
        public byte bReserved1;
        public byte bReserved2;
        public UInt32 dwData;
        public IntPtr iString;
    };


    internal class User32
    {

       // private User32() { }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern UInt32 SendMessage(
            IntPtr hWnd,
            UInt32 msg,
            UInt32 wParam,
            UInt32 lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(
            IntPtr hWnd,
            UInt32 msg,
            IntPtr wParam,
            IntPtr lParam);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern UInt32 GetWindowThreadProcessId
        (
            IntPtr hWnd,
            //			[ MarshalAs( UnmanagedType.
            out UInt32 lpdwProcessId
        );

    }
}
