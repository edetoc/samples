using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace MulApp
{
    class Program
    {
        static AppServiceConnection connection = null;

        static void Main(string[] args)
        {
            Thread appServiceThread = new Thread(new ThreadStart(ThreadProc));
            appServiceThread.Start();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("*****************************");
            Console.WriteLine("**** Classic desktop app ****");
            Console.WriteLine("*****************************");
            Console.ReadLine();

        }
        /// <summary>
        /// Creates the app service connection
        /// </summary>
        static async void ThreadProc()
        {

            Debug.WriteLine("Begin MulApp ThreadProc");

            connection = new AppServiceConnection();
            connection.AppServiceName = "MulAppService";
            connection.PackageFamilyName = Windows.ApplicationModel.Package.Current.Id.FamilyName;
            connection.RequestReceived += Connection_RequestReceived;

            AppServiceConnectionStatus status = await connection.OpenAsync();
            switch (status)
            {
                case AppServiceConnectionStatus.Success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Connection established - waiting for requests");
                    Console.WriteLine();
                    break;
                case AppServiceConnectionStatus.AppNotInstalled:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("The app AppServicesProvider is not installed.");
                    return;
                case AppServiceConnectionStatus.AppUnavailable:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("The app AppServicesProvider is not available.");
                    return;
                case AppServiceConnectionStatus.AppServiceUnavailable:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(string.Format("The app AppServicesProvider is installed but it does not provide the app service {0}.", connection.AppServiceName));
                    return;
                case AppServiceConnectionStatus.Unknown:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(string.Format("An unkown error occurred while we were trying to open an AppServiceConnection."));
                    return;
            }
        }

        /// <summary>
        /// Receives message from UWP app and sends a response back
        /// </summary>
        private static void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            string key = args.Request.Message.First().Key;
            string value = args.Request.Message.First().Value.ToString();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(string.Format("Received message '{0}' with value '{1}'", key, value));

            if (key == "mul")
            {
                // Get number 1 and 2 from received value
                int num1, num2;
                string[] numbers = value.Split(';');
                string result;

                // do the Sum of numbers               
                Int32.TryParse(numbers[0], out num1);
                Int32.TryParse(numbers[1], out num2);

                result = (num1 * num2).ToString();

                ValueSet valueSet = new ValueSet();
                valueSet.Add("response", result);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(string.Format("Sending response: '{0}'", result));
                Console.WriteLine();
                args.Request.SendResponseAsync(valueSet).Completed += delegate { };
            }

            if (key == "killMulApp")
            {
                ValueSet valueSet = new ValueSet();
                valueSet.Add("response", "OK MulApp.exe kills itself in 2 secs");

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(string.Format("Sending response: '{0}'", "OK will kill myself"));
                Console.WriteLine();
                args.Request.SendResponseAsync(valueSet).Completed += delegate { };

                if (connection != null)
                    connection.Dispose();

                Thread.Sleep(2000);

                Environment.Exit(-1);

            }
        }
    }
}
