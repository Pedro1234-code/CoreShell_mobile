using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
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

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x416

namespace Xbox_PWA
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            try
            {
                // Define a pasta de dados do usuário para o WebView2 usando o local da pasta do app
                string userDataFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;

                var options = new CoreWebView2EnvironmentOptions { AdditionalBrowserArguments = "--no-sandbox" };

                // Cria o ambiente WebView2
                var environment = await CoreWebView2Environment.CreateWithOptionsAsync(null, userDataFolder, options);

                // Inicializa o WebView2 com o ambiente configurado
                WebView.CoreWebView2InitializationCompleted += (sender, args) =>
                {
                    if (args.IsSuccess)
                    {
                        WebView.CoreWebView2.Navigate("https://xbox.com/play");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Erro ao inicializar o WebView2: " + args.InitializationException);
                    }
                };

                // Inicia o WebView2 (sem passar o ambiente diretamente)
                await WebView.EnsureCoreWebView2Async();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro ao inicializar o WebView2: " + ex.Message);
            }
        }
    }
}
