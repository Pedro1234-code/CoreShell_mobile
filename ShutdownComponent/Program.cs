using System.Diagnostics;

namespace ShutdownComponent
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
            ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe", "/c notepad.exe");
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;

            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();
        }
    }
}