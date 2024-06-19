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
            try
            {
                string serverVersion = await GetServerVersionAsync(VersionFileUrl);

                if (IsNewVersionAvailable(CurrentVersion, serverVersion))
                {
                    await DownloadUpdatePackageAsync(PackageUrl);
                    // Notify the user that the update package has been downloaded
                }
                else
                {
                    // Notify the user that they already have the latest version
                }
            }
            catch (Exception ex)
            {
                // Handle errors (e.g., network issues, parsing errors)
            }
        }

        private async Task<string> GetServerVersionAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetStringAsync(url);
            }
        }

        private bool IsNewVersionAvailable(string currentVersion, string serverVersion)
        {
            Version current = new Version(currentVersion);
            Version server = new Version(serverVersion);

            return server.CompareTo(current) > 0;
        }

        private async Task DownloadUpdatePackageAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                byte[] packageBytes = await response.Content.ReadAsByteArrayAsync();

                StorageFolder downloadsFolder = await GetDownloadsFolderAsync();
                if (downloadsFolder != null)
                {
                    StorageFile packageFile = await downloadsFolder.CreateFileAsync("update_package.zip", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteBytesAsync(packageFile, packageBytes);
                }
            }
        }

        private async Task<StorageFolder> GetDownloadsFolderAsync()
        {
            StorageFolder folder = null;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                FolderPicker folderPicker = new FolderPicker();
                folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
                folderPicker.FileTypeFilter.Add("*");

                folder = await folderPicker.PickSingleFolderAsync();
            });
            return folder;
        }
    }
}
