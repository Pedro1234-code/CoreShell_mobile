using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using factoryos_10x_shell.Library.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Input.Preview.Injection;
using Windows.UI.ViewManagement;
using Windows.System;
using System;
using System.Runtime.InteropServices;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using System.Threading.Tasks;
using Windows.Devices.Power;
using Windows.UI.Core;
using System.Linq;
using factoryos_10x_shell.Services.Navigation;
using factoryos_10x_shell.Library.Services.Navigation;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using factoryos_10x_shell.Services.Helpers;
using System.Diagnostics;


namespace factoryos_10x_shell.Views
{
    public sealed partial class Default10xBar : Page
    {
        public Default10xBar()
        {
            this.InitializeComponent();

            DataContext = App.ServiceProvider.GetRequiredService<Default10xBarViewModel>();
            AppState.Instance.OnSearchButtonVisibilityChanged += UpdateSearchButtonVisibility;
            AppState.Instance.OnCopilotButtonVisibilityChanged += UpdateCopilotButtonVisibility; UpdateSearchButtonVisibility(AppState.Instance.IsSearchButtonVisible);


            UpdateSearchButtonVisibility(AppState.Instance.IsSearchButtonVisible);
            UpdateCopilotButtonVisibility(AppState.Instance.IsCopilotButtonVisible);
        }


        public Default10xBarViewModel ViewModel => (Default10xBarViewModel)this.DataContext;

        private void ActionCenterClick(object sender, RoutedEventArgs e)
        {

        }

        private void TaskViewButton_Click(object sender, RoutedEventArgs e)
        {

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
