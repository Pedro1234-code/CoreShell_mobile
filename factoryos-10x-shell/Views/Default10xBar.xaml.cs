using factoryos_10x_shell.Library.Models.InternalData;
using factoryos_10x_shell.Library.Services.Navigation;
using factoryos_10x_shell.Library.ViewModels;
using factoryos_10x_shell.Services.Helpers;
using factoryos_10x_shell.Services.Navigation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Core;
using Windows.Devices.Power;
using Windows.Foundation.Collections;
using Windows.Management.Deployment;
using Windows.System;
using Windows.UI;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Input.Preview.Injection;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;


namespace factoryos_10x_shell.Views
{
    public sealed partial class Default10xBar : Page
    {
        private readonly AppHelper _appHelper;


        public Default10xBar()
        {
            this.InitializeComponent();

            DataContext = App.ServiceProvider.GetRequiredService<Default10xBarViewModel>();
            _appHelper = App.ServiceProvider.GetRequiredService<AppHelper>();

            AppState.Instance.OnSearchButtonVisibilityChanged += UpdateSearchButtonVisibility;
            AppState.Instance.OnCopilotButtonVisibilityChanged += UpdateCopilotButtonVisibility;

            UpdateSearchButtonVisibility(AppState.Instance.IsSearchButtonVisible);
            UpdateCopilotButtonVisibility(AppState.Instance.IsCopilotButtonVisible);

            Loaded += Default10xBar_Loaded; // Chamada após a view estar carregada
        }

        private async void Default10xBar_Loaded(object sender, RoutedEventArgs e)
        {
            await _appHelper.LoadPinnedTaskbarAppsAsync();
            var loadedApps = _appHelper.LoadTaskbarPinnedApps();
            Debug.WriteLine($"Loaded pinned apps count: {loadedApps?.Count}");
            if (loadedApps != null)
            {
                foreach (var app in loadedApps)
                {
                    var fullApp = await _appHelper.GetAppListEntryFromModelAsync(app);
                    if (fullApp != null)
                    {
                        app.Data = fullApp;

                        var logo = fullApp.DisplayInfo.GetLogo(new Windows.Foundation.Size(48, 48));
                        if (logo != null)
                        {
                            var stream = await logo.OpenReadAsync();
                            var decoder = await BitmapDecoder.CreateAsync(stream);

                            using (var memStream = new InMemoryRandomAccessStream())
                            {
                                var encoder = await BitmapEncoder.CreateForTranscodingAsync(memStream, decoder);
                                encoder.BitmapTransform.ScaledWidth = 32;
                                encoder.BitmapTransform.ScaledHeight = 32;
                                encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;

                                await encoder.FlushAsync();

                                var bitmap = new BitmapImage();
                                memStream.Seek(0);
                                await bitmap.SetSourceAsync(memStream);
                                app.IconSource = bitmap;
                            }
                        }

                        _appHelper.TaskbarIcons.Add(app);
                    }
                    else
                    {
                        Debug.WriteLine($"App inválido: {app.AppId} / {app.Aumid}");
                    }
                }

            }

            RefreshPinnedApps();
            _appHelper.TaskbarIcons.CollectionChanged += (s, args) => RefreshPinnedApps();
        }


        public Default10xBarViewModel ViewModel => (Default10xBarViewModel)this.DataContext;



        private void ActionCenterClick(object sender, RoutedEventArgs e)
        {

        }

        private void TaskViewButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RefreshPinnedApps()
        {
            PinnedAppsPanel.Children.Clear();


            foreach (var app in _appHelper.TaskbarIcons)
            {
                var button = new Button
                {
                    Width = 48,
                    Height = 48,
                    Margin = new Thickness(2),
                    Style = (Style)Application.Current.Resources["AppIconBackgroundStyle"],
                    Tag = app
                };

                var stack = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var icon = new Image
                {
                    Width = 32,
                    Height = 32,
                    Source = app.IconSource
                };


                stack.Children.Add(icon);
                button.Content = stack;

                button.Click += async (s, e) =>
                {
                    var model = (s as Button)?.Tag as StartIconModel;
                    if (model != null)
                    {
                        AppListEntry entry = null;

                        // Se já estiver armazenado
                        if (model.Data is AppListEntry storedEntry)
                        {
                            entry = storedEntry;
                        }
                        else
                        {
                            // Busca dinamicamente com base em AppId e AUMID
                            entry = await _appHelper.GetAppListEntryFromModelAsync(model);
                        }

                        if (entry != null)
                        {
                            await entry.LaunchAsync();
                        }
                        else
                        {
                            // Debug opcional
                            System.Diagnostics.Debug.WriteLine($"App não encontrado: {model.AppId} / {model.Aumid}");
                        }
                    }
                };

                button.RightTapped += (s, e) =>
                {
                    var flyout = new MenuFlyout();

                    var unpinItem = new MenuFlyoutItem { Text = "Unpin App" };
                    unpinItem.Click += (sender2, e2) =>
                    {
                        var model = (s as Button)?.Tag as StartIconModel;
                        if (model != null)
                        {
                            _appHelper.TaskbarIcons.Remove(model);
                            _appHelper.SaveTaskbarPinnedApps(_appHelper.TaskbarIcons.ToList());
                            RefreshPinnedApps(); 
                        }
                    };

                    flyout.Items.Add(unpinItem);
                    flyout.ShowAt(button, e.GetPosition(button));
                };


                PinnedAppsPanel.Children.Add(button);
            }

        }
        private void UpdateSearchButtonVisibility(bool isVisible)
        {
            SearchButton.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateCopilotButtonVisibility(bool isVisible)
        {
            CopilotButton.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void CopilotButton_Click(object sender, RoutedEventArgs e)
        {
            var packageFamilyName = "MobileOSdev.CopilotPWA_d7x680j9yw8bm";
            var pm = new Windows.Management.Deployment.PackageManager();
            var packages = pm.FindPackagesForUser(string.Empty, packageFamilyName);

            var foundPackage = packages.FirstOrDefault();
            if (foundPackage != null)
            {
                var appListEntries = await foundPackage.GetAppListEntriesAsync();
                var entry = appListEntries.FirstOrDefault();
                if (entry != null)
                {
                    bool success = await entry.LaunchAsync();
                }
            }
        }
    }
}
