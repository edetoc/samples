using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CreateProcessSample
{
    class Program
    {
        static void Main(string[] args)
        {
           
            // Retrieve parameters

            if (args.Length == 3)
            {
                string domain = args[0];
                string user = args[1];
                string password = args[2];

                Console.WriteLine("domain: " + domain);
                Console.WriteLine("user: " + user);
                Console.WriteLine("password: " + password);
                Console.WriteLine();

                try
                {
                    //Console.WriteLine("Test with CreateProcessWithLogonW...");
                    //Win32.LaunchCommand1("c:\\tmp\\batch.cmd", "machine", "user", "password");

                    Console.WriteLine("This is a test with CreateProcessAsUser...");
                    Win32.LaunchCommand2("c:\\tmp\\batch.cmd", domain, user, password);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("LaunchCommand error: " + ex.Message);
                }
                //Console.WriteLine("Press any key to continue...");
                //Console.ReadKey();

            }
            else displayUsage(args.Length);
            
        }

        static void displayUsage(int argc)
        {
            Console.WriteLine (string.Format ("Bad number of args ({0})",argc.ToString() ));
            Console.WriteLine("CreateProcessSample domain user password");

        }
    }
}

