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

            //StringBuilder str = new StringBuilder();
            //str.AppendLine("pid is:" + GetCurrentProcessId().ToString());
            //str.AppendLine("tid is:" + GetCurrentThreadId().ToString());
            //return str.ToString();

            try
            {
                string path = @"c:\tmp\MyTest.txt";
                if (!File.Exists(path))
                {
                    // Create a file to write to.
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine("Hello");
                        sw.WriteLine("And");
                        sw.WriteLine("Welcome");
                    }
                }
                else return "file already exists";

                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

    }
}
