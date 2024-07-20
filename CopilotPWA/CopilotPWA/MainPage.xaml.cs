using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x416

namespace CopilotPWA
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            StartWB();
        }


        private async void StartWB()
        {
            await CopilotLoader.EnsureCoreWebView2Async();
            CopilotLoader.CoreWebView2.Settings.UserAgent = "Mozilla/5.0 (Linux; Android 12; moto g(60) Build/S2RI32.32-20-9-9-2; ) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Mobile Safari/537.36 EdgA/121.0.2277.138";
            CopilotLoader.CoreWebView2.Navigate("https://bing.com/chat");
        }
        private void MainPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (CopilotLoader.CanGoBack)
            {
                CopilotLoader.GoBack();
                e.Handled = true;
            }
        }
    }
}
