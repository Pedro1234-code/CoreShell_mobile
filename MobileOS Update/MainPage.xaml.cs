using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MobileOS_Update
{
    public sealed partial class MainPage : Page
    {
        private const string CurrentVersion = "20724"; // Set your current version here
        private const string VersionFileUrl = "https://github.com/Pedro1234-code/MOS_updatefiles/releases/download/static/version.txt"; // URL to the version file
        private const string PackageUrl = "https://github.com/Pedro1234-code/MOS_updatefiles/releases/download/static/Appx.Packer.v3.0.zip"; // URL to the update package

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void UpdateVerify_Click(object sender, RoutedEventArgs e)
        {
            // Get the current installed version
            string installedVersion = CurrentVersion;

            // Download the version info from the server
            string serverVersionInfo = await DownloadVersionInfoAsync(VersionFileUrl);

            if (!string.IsNullOrEmpty(serverVersionInfo))
            {
                // Parse the version from the server info
                if (int.TryParse(serverVersionInfo, out int serverVersion))
                {
                    if (serverVersion > int.Parse(installedVersion))
                    {
                        // Server version is higher, proceed with download
                        await DownloadAndSaveUpdateAsync();
                    }
                    else
                    {
                       // no update
                    }
                }
                else
                {
                    // Invalid version format in the server info
                    // Handle this case (e.g., log an error)
                }
            }
            else
            {
                // Error downloading version info
                // Handle this case (e.g., show an error message)
            }
        }

        private async Task DownloadAndSaveUpdateAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(PackageUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        // Save the package to a temporary location
                        StorageFile tempFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync("AppUpdate.zip", CreationCollisionOption.ReplaceExisting);
                        await FileIO.WriteBytesAsync(tempFile, await response.Content.ReadAsByteArrayAsync());

                        // Use FolderPicker to let the user choose the save location
                        FolderPicker folderPicker = new FolderPicker();
                        folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
                        folderPicker.FileTypeFilter.Add(".zip");
                        StorageFolder pickedFolder = await folderPicker.PickSingleFolderAsync();

                        if (pickedFolder != null)
                        {
                            await tempFile.MoveAsync(pickedFolder, "UpdatePackage_mobileos.zip", NameCollisionOption.GenerateUniqueName);

                            // Inform the user about the update (You can use a dialog or any other UI element)
                        }
                        else
                        {
                            // Handle user cancellation
                        }
                    }
                    else
                    {
                        // Handle unsuccessful response (e.g., server error, file not found)
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., network issues)
            }
        }

        public async Task<string> DownloadVersionInfoAsync(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(VersionFileUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        // Handle unsuccessful response (e.g., server error, file not found)
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., network issues)
                return null;
            }
        }
    }
}
