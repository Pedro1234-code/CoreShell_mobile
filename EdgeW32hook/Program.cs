using System.Diagnostics;

namespace EdgeW32hook
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
            Process.Start(new ProcessStartInfo
            {
                FileName = "microsoft-edge://",
                UseShellExecute = true
            });
        }
    }
}