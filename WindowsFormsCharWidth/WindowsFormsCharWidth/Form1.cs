using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text;

namespace WindowsFormsCharWidth
{
    public partial class Form1 : Form
    {
        // http://www.pinvoke.net/default.aspx/gdi32.getcharabcwidths
        // http://www.gamedev.net/topic/390305-c-how-to-use-getcharabcwidths--please-help/
        // https://msdn.microsoft.com/en-us/library/windows/desktop/dd144857(v=vs.85).aspx  <== GetCharABCWidths()
        // https://www.microsoft.com/en-us/Typography/default.aspx

        [StructLayout(LayoutKind.Sequential)]
        public struct ABC
        {
            public int abcA;
            public uint abcB;
            public int abcC;

            public override string ToString()
            {
                return string.Format("A={0}, B={1}, C={2}", abcA, abcB, abcC);
            }
        }

        [DllImport("gdi32.dll", ExactSpelling = true)]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObj);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern int DeleteObject(IntPtr hObj);

        [DllImport("gdi32.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool GetCharABCWidthsW(IntPtr hdc, uint uFirstChar, uint uLastChar, [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct, SizeConst = 1)] ABC[] lpabc);


        public Form1()
        {
            InitializeComponent();
                        
         
        }

        public ABC GetCharWidthABC(char ch, Font font, Graphics gr)
        {
            ABC[] _temp = new ABC[1];
            IntPtr hDC = gr.GetHdc();
            Font ft = (Font)font.Clone();
            IntPtr hFt = ft.ToHfont();
            SelectObject(hDC, hFt);
            GetCharABCWidthsW(hDC, ch, ch, _temp);
            DeleteObject(hFt);
            gr.ReleaseHdc();
            return _temp[0];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Graphics g = CreateGraphics();
            StringBuilder bld = new StringBuilder();

            Font font = new Font("Arial", 16, GraphicsUnit.Pixel);
            var s = GetCharWidthABC('I', font, g).ToString();
            bld.AppendLine(s);

            Font font2 = new Font("Arial", 17, GraphicsUnit.Pixel);
            var s2 = GetCharWidthABC('I', font2, g).ToString();
            bld.AppendLine(s2);


            Font font3 = new Font("Arial", 18, GraphicsUnit.Pixel);
            var s3 = GetCharWidthABC('I', font3, g).ToString();
            bld.AppendLine(s3);

            textBox1.Text = bld.ToString();

            g.Dispose();
        }

    }
}
