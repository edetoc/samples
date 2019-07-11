using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HelloWorldBRT
{
    public sealed class Class
    {
        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentProcessId();

        public uint CurrentThreadId
        {
            get { return GetCurrentThreadId(); }
        }

        public uint CurrentProcessId
        {
            get { return GetCurrentProcessId(); }
        }

        public static string DoWork()
        {

            StringBuilder str = new StringBuilder();
            str.AppendLine("This code is executed from process with pid:" + GetCurrentProcessId().ToString() + "<br>");
            
            try
            {
                string filepath = @"c:\tmp\MyTest.txt";
                if (!File.Exists(filepath))
                {
                    // Create a file to write to.
                    using (StreamWriter sw = File.CreateText(filepath))
                    {
                        sw.WriteLine("This text has been written from the Windows plugin");
                        
                    }
                    str.AppendLine(filepath + " created with success");
                }
                else
                {
                    str.AppendLine (filepath + " already exists");
                                  
                }
                
            }
            catch (Exception ex)
            {
                str.AppendLine(ex.Message);
                
            }

            return str.ToString();
            

        }

    }
}
