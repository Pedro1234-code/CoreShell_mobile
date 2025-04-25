using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Management.Deployment;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MobileOS_Update
{
    public sealed partial class MainPage : Page
    {
        private const string CurrentVersion = "20844"; 
        private const string VersionFileUrl = "https://github.com/Pedro1234-code/MOS_updatefiles/releases/download/static/version.txt"; // URL to the version file
        private const string PackageUrl = "https://github.com/Pedro1234-code/MOS_updatefiles/releases/download/static/latest.zip"; // URL to the update package

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
                        DownloadProgressBar.Visibility = Visibility.Visible;
                        UpdateVerify.Visibility = Visibility.Collapsed;
                        UpdateCert.Visibility = Visibility.Collapsed;
                        InstallMOS.Visibility = Visibility.Collapsed;
                        await DownloadAndSaveUpdateAsync();
                        DownloadProgressBar.Visibility = Visibility.Collapsed;
                        UpdateVerify.Visibility = Visibility.Visible;
                        UpdateCert.Visibility = Visibility.Visible;
                        InstallMOS.Visibility = Visibility.Visible;

                    }
                    else
                    {
                        await new MessageDialog("No updates available", "No updates. Check later.").ShowAsync();
                    }
                }
                else
                {

                }
            }
            else
            {

            }
        }

        private async void UpdateCert_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("updatecert:"));
            await new MessageDialog("Updated certs", "MobileOS Certificates have been updated").ShowAsync();
        }

        private async Task DownloadAndSaveUpdateAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(PackageUrl, HttpCompletionOption.ResponseHeadersRead);
                    if (response.IsSuccessStatusCode)
                    {
                        // Save the package to a temporary location
                        StorageFile tempFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync("AppUpdate.zip", CreationCollisionOption.ReplaceExisting);

                        using (var fileStream = await tempFile.OpenStreamForWriteAsync())
                        using (var contentStream = await response.Content.ReadAsStreamAsync())
                        {
                            long totalBytes = response.Content.Headers.ContentLength ?? 0;
                            long totalBytesRead = 0;
                            byte[] buffer = new byte[8192];
                            int bytesRead;

                            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;

                                if (totalBytes > 0)
                                {
                                    double progress = (double)totalBytesRead / totalBytes * 100;
                                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                    {
                                        DownloadProgressBar.Value = progress;
                                    });
                                }
                            }
                        }

                        // Use FolderPicker to let the user choose the save location
                        FolderPicker folderPicker = new FolderPicker();
                        folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
                        folderPicker.FileTypeFilter.Add("*");
                        StorageFolder pickedFolder = await folderPicker.PickSingleFolderAsync();

                        if (pickedFolder != null)
                        {
                            StorageFile destinationFile = await tempFile.CopyAsync(pickedFolder, "UpdatePackage_mobileos.zip", NameCollisionOption.ReplaceExisting);
                            await ExtractAndInstallPackagesAsync(destinationFile, pickedFolder);
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
                await new MessageDialog("Connection not found.", "Please check your internet connection.").ShowAsync();

            }
        }

        private async Task ExtractAndInstallPackagesAsync(StorageFile zipFile, StorageFolder destinationFolder)
        {
            List<StorageFile> nonMobileOSPackages = new List<StorageFile>();
            StorageFile mobileOSPackage = null;

            try
            {
                using (Stream zipStream = await zipFile.OpenStreamForReadAsync())
                using (ZipFile zip = new ZipFile(zipStream))
                {
                    foreach (ZipEntry entry in zip)
                    {
                        if (!entry.IsFile) continue;

                        string entryFileName = entry.Name;
                        Stream zipEntryStream = zip.GetInputStream(entry);

                        // Create full output path
                        StorageFile outputFile = await destinationFolder.CreateFileAsync(entryFileName, CreationCollisionOption.ReplaceExisting);

                        using (Stream outputStream = await outputFile.OpenStreamForWriteAsync())
                        {
                            byte[] buffer = new byte[4096];
                            StreamUtils.Copy(zipEntryStream, outputStream, buffer);
                        }

                        // Check if the entry is an appx or msix package
                        if (entryFileName.EndsWith(".msix", StringComparison.OrdinalIgnoreCase) ||
                            entryFileName.EndsWith(".msixbundle", StringComparison.OrdinalIgnoreCase))
                        {
                            if (entryFileName.StartsWith("MobileOS", StringComparison.OrdinalIgnoreCase))
                            {
                                mobileOSPackage = outputFile;
                            }
                            else
                            {
                                nonMobileOSPackages.Add(outputFile);
                            }
                        }
                    }
                }

                // Install non-MobileOS packages first
                foreach (var package in nonMobileOSPackages)
                {
                    await InstallPackageAsync(package);
                }

                // Install MobileOS package last
                if (mobileOSPackage != null)
                {
                    await InstallPackageAsync(mobileOSPackage);
                }
            }
            catch (Exception ex)
            {
                // Handle extraction or installation errors
            }
        }

        private async Task InstallPackageAsync(StorageFile packageFile)
        {
            try
            {
                var packageManager = new PackageManager();
                var deploymentResult = await packageManager.AddPackageAsync(new Uri(packageFile.Path), null, DeploymentOptions.ForceApplicationShutdown);

                // Check the deployment result
                if (deploymentResult != null && deploymentResult.ExtendedErrorCode != null)
                {
                    // Handle errors (e.g., log the error message)
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog("Installation has been partially completed.", "Installation was partially completed. If you get any issues, please report.").ShowAsync();

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


        private async void InstallMOS_Click(object sender, RoutedEventArgs e)
        {
            DownloadProgressBar.Visibility = Visibility.Visible;
            UpdateVerify.IsEnabled = false;
            InstallMOS.IsEnabled = false; 

            // Set the download link for the zip file here
            string zipFileUrl = "https://github.com/Pedro1234-code/MOS_updatefiles/releases/download/static/base.zip";

            // Use FolderPicker to let the user choose the save location
            FolderPicker folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.Downloads
            };
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder pickedFolder = await folderPicker.PickSingleFolderAsync();

            if (pickedFolder != null)
            {
                await DownloadAndInstallFromZipAsync(zipFileUrl, pickedFolder);
            }
            else
            {
                // User canceled the folder picking
                await new MessageDialog("Operation canceled", "No folder selected for download.").ShowAsync();
            }

            DownloadProgressBar.Visibility = Visibility.Collapsed;
            InstallMOS.IsEnabled = true;
            InstallMOS.IsEnabled = true; // Re-enable button after installation
        }

        private async Task DownloadAndInstallFromZipAsync(string url, StorageFolder destinationFolder)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    if (response.IsSuccessStatusCode)
                    {
                        // Save the package to the selected location
                        StorageFile tempFile = await destinationFolder.CreateFileAsync("InstallUpdate.zip", CreationCollisionOption.ReplaceExisting);

                        using (var fileStream = await tempFile.OpenStreamForWriteAsync())
                        using (var contentStream = await response.Content.ReadAsStreamAsync())
                        {
                            long totalBytes = response.Content.Headers.ContentLength ?? 0;
                            long totalBytesRead = 0;
                            byte[] buffer = new byte[8192];
                            int bytesRead;

                            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;

                                if (totalBytes > 0)
                                {
                                    double progress = (double)totalBytesRead / totalBytes * 100;
                                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                    {
                                        DownloadProgressBar.Value = progress;
                                    });
                                }
                            }
                        }

                        // Extract and install packages
                        await ExtractAndInstallPackagesAsync(tempFile, destinationFolder);
                    }
                    else
                    {
                        await new MessageDialog("Download failed", "Please check the URL or your internet connection.").ShowAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog("An error occurred", "Please check your internet connection.").ShowAsync();
            }
        }
    }
}
