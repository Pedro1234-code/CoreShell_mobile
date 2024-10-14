using factoryos_10x_shell.Services.Helpers;
using factoryos_10x_shell.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace factoryos_10x_shell
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            this.InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(StartMenu), null);
        }
        private void CopilotSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.Name == "VisibilityToggleSwitch")
                {
                    AppState.Instance.IsSearchButtonVisible = toggleSwitch.IsOn;
                    Windows.Storage.ApplicationData.Current.LocalSettings.Values["IsSearchButtonVisible"] = toggleSwitch.IsOn;
                }
                else if (toggleSwitch.Name == "CopilotToggleSwitch")
                {
                    AppState.Instance.IsCopilotButtonVisible = toggleSwitch.IsOn;
                    Windows.Storage.ApplicationData.Current.LocalSettings.Values["IsCopilotButtonVisible"] = toggleSwitch.IsOn;
                }
            }
        }

        private void Search_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                AppState.Instance.IsSearchButtonVisible = toggleSwitch.IsOn;
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["IsSearchButtonVisible"] = toggleSwitch.IsOn;
            }
        }

        private void BG_Toggled(object sender, RoutedEventArgs e)
        {
            {
                ToggleSwitch toggleSwitch = sender as ToggleSwitch;
                if (toggleSwitch != null)
                {
                    AppState.Instance.IsBgChangeButtonVisible = toggleSwitch.IsOn;
                    Windows.Storage.ApplicationData.Current.LocalSettings.Values["IsBgChangeButtonVisible"] = toggleSwitch.IsOn;
                }
            }
        }
    }
}
