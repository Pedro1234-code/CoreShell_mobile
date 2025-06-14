using Windows.UI.Xaml.Controls;
using System;

namespace factoryos_10x_shell.Views
{
    public sealed partial class FallbackErrorPage : Page
    {
        public FallbackErrorPage(Exception ex)
        {
            this.InitializeComponent();

            ExceptionMessage.Text = $"Tipo: {ex.GetType().Name}\nMensagem: {ex.Message}";
            StackTrace.Text = $"StackTrace:\n{ex.StackTrace}";
        }
    }
}
