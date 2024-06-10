using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using factoryos_10x_shell.Library.Models.InternalData;
using factoryos_10x_shell.Library.Services.Helpers;
using factoryos_10x_shell.Library.Services.Managers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Input.Preview.Injection;
using Windows.UI.Xaml.Controls;

namespace factoryos_10x_shell.Library.ViewModels
{
    public partial class StartMenuViewModel : ObservableObject
    {
        private readonly IStartManagerService m_startManager;

        private readonly IAppHelper m_appHelper;


        public StartMenuViewModel(IStartManagerService startManager, IAppHelper appHelper) 
        {
            m_startManager = startManager;
            m_appHelper = appHelper;

            AppsListGridHeight = 310;
            AppsListToggleContent = "Show all";
        }


        public ObservableCollection<StartIconModel> StartIcons => m_appHelper.StartIcons;


        [ObservableProperty]
        private double appsListGridHeight;

        [ObservableProperty]
        private string appsListToggleContent;

        [RelayCommand]
        public void AppsListToggleClicked()
        {
            m_startManager.IsAppsListExpanded = !m_startManager.IsAppsListExpanded;
            bool expanded = m_startManager.IsAppsListExpanded;

            AppsListToggleContent = expanded ? "Show less" : "Show all";
            if (expanded)
            {
                AppsListGridHeight = Double.NaN;
            }
            else
            {
                AppsListGridHeight = 310;
            }
        }

        [RelayCommand]
        public async Task TextBoxSearchClickedAsync()
        {
            InputInjector inputInjector = InputInjector.TryCreate();

            // Create an instance for the 'Tab' key
            InjectedInputKeyboardInfo sKey = new InjectedInputKeyboardInfo();
            sKey.VirtualKey = (ushort)(VirtualKey.S);
            sKey.KeyOptions = InjectedInputKeyOptions.None;

            // Create an instance for the 'Windows' key
            InjectedInputKeyboardInfo winKey = new InjectedInputKeyboardInfo();
            winKey.VirtualKey = (ushort)(VirtualKey.LeftWindows);
            winKey.KeyOptions = InjectedInputKeyOptions.None;

            // Inject the 'Windows' key down
            inputInjector.InjectKeyboardInput(new[] { winKey });

            // Inject the 'Tab' key down and up
            inputInjector.InjectKeyboardInput(new[] { sKey });
            sKey.KeyOptions = InjectedInputKeyOptions.KeyUp;
            inputInjector.InjectKeyboardInput(new[] { sKey });

            // Inject the 'Windows' key up
            winKey.KeyOptions = InjectedInputKeyOptions.KeyUp;
            inputInjector.InjectKeyboardInput(new[] { winKey });

        }

    }
}
