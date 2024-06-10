using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;

namespace factoryos_10x_shell.Library.ViewModels
{
    public partial class PowerDialogScreenViewModel : ObservableObject
    {
        public PowerDialogScreenViewModel()
        {
        }

        [RelayCommand]
        private async void ShutDownButton_Click()
        {
            await Launcher.LaunchUriAsync(new Uri("shutdowncomponent:"));
        }


    }
}
