﻿using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Management.Deployment;
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
                        await DownloadAndSaveUpdateAsync();
                        DownloadProgressBar.Visibility = Visibility.Collapsed;
                        UpdateVerify.Visibility = Visibility.Visible;
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
            }
        }

        private async Task ExtractAndInstallPackagesAsync(StorageFile zipFile, StorageFolder destinationFolder)
        {
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

                        if (entryFileName.EndsWith(".msix", StringComparison.OrdinalIgnoreCase) ||
                            entryFileName.EndsWith(".msixbundle", StringComparison.OrdinalIgnoreCase))
                        {
                            await InstallPackageAsync(outputFile);
                        }
                    }
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
                // Handle installation errors
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
