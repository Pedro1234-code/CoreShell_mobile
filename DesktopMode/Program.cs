using System.Diagnostics;

namespace DesktopMode
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            KillProcess("factoryos-10x-shell");
            KillProcess("MobileShellPlus");
            KillProcess("explorer");

            // Restart explorer.exe
            StartProcess("explorer.exe");
        }

        static void KillProcess(string processName)
        {
            foreach (var process in Process.GetProcessesByName(processName))
            {
                try
                {
                    process.Kill();
                    process.WaitForExit();
                    Console.WriteLine($"{processName} has been killed.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error killing {processName}: {ex.Message}");
                }
            }
        }

        static void StartProcess(string processName)
        {
            try
            {
                Process.Start(processName);
                Console.WriteLine($"{processName} has been started.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting {processName}: {ex.Message}");
            }
        }
    }
}