using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.System;

namespace factoryos_10x_shell
{
    public sealed class BackgroundTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            // Your background task logic here
            CheckStartMenuState().Wait();
            // Detect Start Menu open and launch protocol
            deferral.Complete();
        }

        private async Task CheckStartMenuState()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "Win32Helpers.exe",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string result = await process.StandardOutput.ReadToEndAsync();
            process.WaitForExit();

            if (result.Contains("Start Menu is open"))
            {
                await LaunchProtocolAsync();
            }
        }

        private async Task LaunchProtocolAsync()
        {
            var uri = new Uri("coreshell://");
            await Launcher.LaunchUriAsync(uri);
        }
    }
}
