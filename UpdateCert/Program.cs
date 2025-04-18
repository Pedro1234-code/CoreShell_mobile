using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

class Program
{
    static async System.Threading.Tasks.Task Main(string[] args)
    {
        string certUrl = "https://github.com/Pedro1234-code/MOS_updatefiles/releases/download/static/MOS.cer";
        string tempPath = Path.Combine(Path.GetTempPath(), "MOS.cer");

        try
        {
            // Download certificate
            using (HttpClient client = new HttpClient())
            {
                Console.WriteLine("Downloading certificate...");
                byte[] certBytes = await client.GetByteArrayAsync(certUrl);
                await File.WriteAllBytesAsync(tempPath, certBytes);
                Console.WriteLine("Downloaded to: " + tempPath);
            }

            // Load certificate
            Console.WriteLine("Loading certificate...");
            X509Certificate2 cert = new X509Certificate2(tempPath);

            // Open store and install
            Console.WriteLine("Installing certificate...");
            using (X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadWrite);
                store.Add(cert);
                store.Close();
            }

            Console.WriteLine("Certificate successfully installed to Trusted Root (Local Machine).");

            // Clean up
            File.Delete(tempPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
