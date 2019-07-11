using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LaunchNotepadSysNative
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            StringBuilder SB = new StringBuilder();
            SB.Append("Welcome! (this process runs as ");
            if (System.Environment.Is64BitProcess)
                SB.Append("64 bits)");
            else
                SB.Append("32 bits)");

            this.Text = SB.ToString();
            
        }

        // launch Notepad x86
        private void button1_Click(object sender, EventArgs e)
        {
            ProcessStartInfo PSI = new ProcessStartInfo("notepad.exe");
            Process p = Process.Start(PSI);

        }

        // launch Notepad x64 
        private void button2_Click(object sender, EventArgs e)
        {
            /*
            942589	A 32-bit application cannot access the system32 folder on a computer that is running a 64-bit version of Windows Server 2003 or of Windows XP
            http://support.microsoft.com/kb/942589/en-US
            */

            var pathToExe = Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.Windows),  @"sysnative\notepad.exe");

            ProcessStartInfo PSI = new ProcessStartInfo(pathToExe);          
            Process p = Process.Start(PSI);
        }
    }
}
