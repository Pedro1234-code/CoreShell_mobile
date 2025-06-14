using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Management.Deployment;
using Windows.Networking.Proximity;
using Windows.UI.Xaml;
using System.Runtime.Serialization.Json;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.System.UserProfile;
using Windows.Storage;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media.Imaging;

using static factoryos_10x_shell.Helpers.VisualHelper;
using Windows.Storage.Streams;
using static System.Net.Mime.MediaTypeNames;
using Windows.System;
using Microsoft.Extensions.DependencyInjection;
using factoryos_10x_shell.Library.ViewModels;
using factoryos_10x_shell.Library.Models.InternalData;
using Windows.ApplicationModel.Core;
using factoryos_10x_shell.Library.Services.Helpers;
using System.Threading.Tasks;
using Windows.UI.Input.Preview.Injection;
using factoryos_10x_shell.Services.Helpers;
using factoryos_10x_shell.Library.Services.Managers;

namespace factoryos_10x_shell.Views
{
    public sealed partial class StartMenu : Page
    {

        private readonly IAppHelper appHelper;

        public StartMenu()
        {
            this.InitializeComponent();
            appHelper = App.ServiceProvider.GetRequiredService<IAppHelper>();
            DataContext = App.ServiceProvider.GetRequiredService<StartMenuViewModel>();

        }

        public StartMenuViewModel ViewModel => (StartMenuViewModel)this.DataContext;


        private async void AppButton_Click(object sender, RoutedEventArgs e)
        {
            StartIconModel model = ((FrameworkElement)sender).DataContext as StartIconModel;
            if (model != null)
            {
                // suspend app
                InputInjector inputInjector = InputInjector.TryCreate();

                // Create an instance for the 'Down' key
                InjectedInputKeyboardInfo tabKey = new InjectedInputKeyboardInfo();
                tabKey.VirtualKey = (ushort)(VirtualKey.Down);
                tabKey.KeyOptions = InjectedInputKeyOptions.None;

                // Create an instance for the 'Windows' key
                InjectedInputKeyboardInfo winKey = new InjectedInputKeyboardInfo();
                winKey.VirtualKey = (ushort)(VirtualKey.LeftWindows);
                winKey.KeyOptions = InjectedInputKeyOptions.None;

                // Inject the 'Windows' key down
                inputInjector.InjectKeyboardInput(new[] { winKey });

                // Inject the 'Tab' key down and up
                inputInjector.InjectKeyboardInput(new[] { tabKey });
                tabKey.KeyOptions = InjectedInputKeyOptions.KeyUp;
                inputInjector.InjectKeyboardInput(new[] { tabKey });

                // Inject the 'Windows' key up
                winKey.KeyOptions = InjectedInputKeyOptions.KeyUp;
                inputInjector.InjectKeyboardInput(new[] { winKey });

                // launch app
                await (model.Data as AppListEntry).LaunchAsync();

            }
        }

        private void PinToTaskbar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item &&
                item.DataContext is StartIconModel app)
            {
                if (!appHelper.TaskbarIcons.Any(i => i.AppId == app.AppId))
                {
                    appHelper.TaskbarIcons.Add(app);
                    appHelper.SaveTaskbarPinnedApps(appHelper.TaskbarIcons.ToList());
                    Debug.WriteLine($"Salvando: AppId={app.AppId} / Aumid={app.Aumid ?? "NULL"} / Nome={app.IconName}");
                }
            }
        }


        private async void PowerButton_click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("powerdialogcomponent:"));
        }

        private void SettingsButton_click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Settings), null);
        }

    }
}