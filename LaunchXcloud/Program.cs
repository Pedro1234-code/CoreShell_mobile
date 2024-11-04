using System;
using System.Diagnostics;
using System.IO;

namespace ExecuteExeWithArgs
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Get the directory of the current executable
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // Path to the executable you want to run
                string exeName = "XboxCloudGamingPWA.exe"; // Replace with the actual EXE file name
                string exePath = Path.Combine(currentDirectory, exeName);

                // Check if the EXE file exists
                if (File.Exists(exePath))
                {
                    // Start the process with arguments
                    Process process = new Process();
                    process.StartInfo.FileName = exePath;
                    process.StartInfo.WorkingDirectory = currentDirectory;
                    process.StartInfo.Arguments = "--no-sandbox"; // Add the parameter here
                    process.Start();

                    // Optionally wait for the process to exit
                    process.WaitForExit();

                    Console.WriteLine($"{exeName} executed with '--no-sandbox' successfully.");
                }
                else
                {
                    Console.WriteLine($"Executable {exeName} not found in the folder.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
