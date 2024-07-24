using factoryos_10x_shell.Library.Constants;
using factoryos_10x_shell.Library.Models.Hardware;
using factoryos_10x_shell.Library.Services.Environment;
using factoryos_10x_shell.Library.Services.Hardware;
using factoryos_10x_shell.Library.Services.Helpers;
using factoryos_10x_shell.Library.Services.Managers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Management.Deployment;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Xaml.Navigation;
using Windows.UI;
using factoryos_10x_shell.Services.Helpers;

namespace factoryos_10x_shell
{
    sealed partial class App : Application
    {
        public static MediaPlayer MediaPlayer;

        [Obsolete]
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.Resuming += OnResuming;
            if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("IsSearchButtonVisible"))
            {
                AppState.Instance.IsSearchButtonVisible = (bool)Windows.Storage.ApplicationData.Current.LocalSettings.Values["IsSearchButtonVisible"];
            }
            if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("IsCopilotButtonVisible"))
            {
                AppState.Instance.IsCopilotButtonVisible = (bool)Windows.Storage.ApplicationData.Current.LocalSettings.Values["IsCopilotButtonVisible"];
            }
        }

        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            ConfigureServices();
            PreloadServices();
            IAppHelper appHelper = ServiceProvider.GetRequiredService<IAppHelper>();
            IBluetoothService btService = ServiceProvider.GetRequiredService<IBluetoothService>();

            await btService.InitializeAsync();
            await appHelper.LoadAppsAsync();

            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {

                }
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                Window.Current.Activate();
            }

            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
            var view = ApplicationView.GetForCurrentView();
            view.FullScreenSystemOverlayMode = FullScreenSystemOverlayMode.Minimal;
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.Protocol)
            {
                var protocolArgs = args as ProtocolActivatedEventArgs;

                // Check if the app is already running
                if (Window.Current.Content == null)
                {
                    // The app is not running, close the app and launch again

                    var packageFamilyName = "MobileOSDev.CoreShell_d7x680j9yw8bm";
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
                else
                {
                    // The app is running, bring it to the foreground
                    Window.Current.Activate();
                }

                Window.Current.Activate();
            }
        }


        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        private void OnResuming(object sender, object e)
        {
            // TODO: Restore application state and resume any background activity
        }


    }
}
