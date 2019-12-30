using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            // determine the package root, based on own location
            string result = Assembly.GetExecutingAssembly().Location;
            int index = result.LastIndexOf("\\");
            string rootPath = $"{result.Substring(0, index)}\\..\\";

            // process object to keep track of your child process
            Process newProcess = null;

            //if ((bool)ApplicationData.Current.LocalSettings.Values["App1"])
            //{
            //    Process[] remoteAll = Process.GetProcesses("Recorder");
            //    foreach (var i in remoteAll)
            //    {
            //        i.Kill();
            //    }
            //}

            Debug.WriteLine(string.Format("rootpath: {0}", rootPath));

            if (args.Length > 2)
            {
                // launch process based on parameter
                switch (args[2])
                {
                    case "/sum":
                        newProcess = Process.Start(rootPath + @"SumApp\sumapp.exe");
                        Debug.WriteLine("sumapp pid: " + newProcess.Id.ToString());
                        break;
                    case "/mul":
                        newProcess = Process.Start(rootPath + @"MulApp\mulapp.exe");
                        break;
                        //case "/parameters":
                        //    string parameters = ApplicationData.Current.LocalSettings.Values["parameters"] as string;
                        //    newProcess = Process.Start(rootPath + @"Recorder\Recorder.exe", parameters);
                        //    break;

                }
            }
        }
    }
}
