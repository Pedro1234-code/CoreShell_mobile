using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace factoryos_10x_shell.Views
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class PowerDialogScreen : Page
    {
        public PowerDialogScreen()
        {
            this.InitializeComponent();
            string wallpaperPath = WallpaperHelper.GetDesktopWallpaper();
            BitmapImage bitmapImage = new BitmapImage(new Uri(wallpaperPath));
            BG_powerdiag.Source = bitmapImage;
        }

        class WallpaperHelper
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

            private const int SPI_GETDESKWALLPAPER = 0x0073;

            public static string GetDesktopWallpaper()
            {
                string wallpaper = new string('\0', 256);
                SystemParametersInfo(SPI_GETDESKWALLPAPER, wallpaper.Length, wallpaper, 0);
                return wallpaper.Substring(0, wallpaper.IndexOf('\0'));
            }
        }

        private async Task ShutdownButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("shutdowncomponent:"));

        }

        private void ShutdownButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
