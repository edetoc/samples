using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.Threading;

namespace EventLogWatcher
{
    class Program
    {

        static int count;

        static void Main(string[] args)
        {
            ManagementEventWatcher eventLogChangesWatcher = null;

            eventLogChangesWatcher = new ManagementEventWatcher(
                                 new EventQuery("SELECT * FROM __InstanceCreationEvent WHERE TargetInstance ISA  'Win32_NTLogEvent' and TargetInstance.LogFile = 'Security' and TargetInstance.EventCode='4624'")
                                                               );

            eventLogChangesWatcher.EventArrived += eventLogChangesWatcher_EventArrived;

            count = 0;
            eventLogChangesWatcher.Start();

            do            
            {
                Console.WriteLine("waiting...");
                Thread.Sleep(1000);

            } while (count < 10);

            eventLogChangesWatcher.Stop();
            
        }

        static void eventLogChangesWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            Console.WriteLine("Event arrived!");

            try 
            {
            
             // Get the Event object and display it
                PropertyData property;
                if ((property = e.NewEvent.Properties["TargetInstance"]) != null)
                {

                    ManagementBaseObject managementObject = property.Value as ManagementBaseObject;

                    StringBuilder sb = new StringBuilder();

                    // See Win32_NTLogEvent class. https://msdn.microsoft.com/en-us/library/aa394226(v=vs.85).aspx

                    sb.AppendLine (managementObject.Properties["SourceName"].Value.ToString());
                    sb.AppendLine (managementObject.Properties["Type"].Value.ToString());

                    sb.AppendLine (managementObject.Properties["EventCode"].Value.ToString());

                    if (managementObject.Properties["Message"].Value != null)
                    {
                    
                        sb.AppendLine (managementObject.Properties["Message"].Value.ToString());
                    }
                    Console.WriteLine(sb.ToString());
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            
            }

            count++;
        }
    }
}
