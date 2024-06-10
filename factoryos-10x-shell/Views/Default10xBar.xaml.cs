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


namespace factoryos_10x_shell.Views
{
    public sealed partial class Default10xBar : Page
    {
        public Default10xBar()
        {
            this.InitializeComponent();

            DataContext = App.ServiceProvider.GetRequiredService<Default10xBarViewModel>();
        }


        public Default10xBarViewModel ViewModel => (Default10xBarViewModel)this.DataContext;

        private void ActionCenterClick(object sender, RoutedEventArgs e)
        {

        }

        private void TaskViewButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PowerButton_Click(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("powerdialogcomponent:"));
        }
    }
}
