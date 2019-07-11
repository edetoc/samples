using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MyRuntimeComponent
{
    public sealed class Class1
    {
        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentProcessId();

        public uint CurrentThreadId
        {
            get { return GetCurrentThreadId(); }
        }

        private uint CurrentProcessId
        {
            get { return GetCurrentProcessId(); }
        }


        public static string DoWork()
        {

            StringBuilder str = new StringBuilder();
            str.AppendLine("pid is:" + GetCurrentProcessId().ToString());
            str.AppendLine("tid is:" + GetCurrentThreadId().ToString());

            return str.ToString();
        }
    }
}
