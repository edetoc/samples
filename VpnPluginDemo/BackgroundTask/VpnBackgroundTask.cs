using System;
using System.Diagnostics;
using Windows.ApplicationModel.Background;
using Windows.Networking.Vpn;
using Windows.System.Diagnostics;

namespace BackgroundTask
{
    /// inheritdoc
    public sealed class VpnBackgroundTask : IBackgroundTask
    {
        private static SslVpnPlugin vpnPlugin;

        private BackgroundTaskDeferral serviceDeferral;        

        /// <inheritdoc />
        public void Run(IBackgroundTaskInstance taskInstance)
        {

            // Debug.WriteLine("VPNDEMO: Entering VpnBackgroundTask.Run");
            
            //Take a deferral 
            serviceDeferral = taskInstance.GetDeferral();

            try
            {
              
                if (VpnBackgroundTask.vpnPlugin == null)
                {
                    Debug.WriteLine("VPNDEMO: creating vpn plugin... ");
                    VpnBackgroundTask.vpnPlugin = new SslVpnPlugin();

                    taskInstance.Canceled += OnTaskCanceled;
                }

                Debug.WriteLine("VPNDEMO: Calling ProcessEventAsync...");
                VpnChannel.ProcessEventAsync(VpnBackgroundTask.vpnPlugin, taskInstance.TriggerDetails);

            }
            catch(Exception ex)
            {
                Debug.WriteLine($"VPNDEMO: unhandled exception in VpnBackgroundTask.Run: {ex.Message} ");
            }

            // we're done complete the deferral
            serviceDeferral.Complete();

            //Debug.WriteLine($"VPNDEMO: Exiting VpnBackgroundTask.Run");

           
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {

            Debug.WriteLine("VPNDEMO: Background " + sender.Task.Name + " Cancel Requested...");

            if (serviceDeferral != null)
            {
                //Complete the service deferral
                serviceDeferral.Complete();
                serviceDeferral = null;
            }
        }
    }
}