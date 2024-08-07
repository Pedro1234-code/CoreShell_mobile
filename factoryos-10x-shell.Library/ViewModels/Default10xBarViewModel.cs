﻿using CommunityToolkit.Mvvm.ComponentModel;
using factoryos_10x_shell.Library.Constants;
using factoryos_10x_shell.Library.Models.Hardware;
using factoryos_10x_shell.Library.Services.Environment;
using factoryos_10x_shell.Library.Services.Hardware;
using factoryos_10x_shell.Library.Services.Managers;
using Windows.UI.Xaml.Controls;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Input.Preview.Injection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI;
using CommunityToolkit.Mvvm.Input;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using factoryos_10x_shell.Library.Services.Navigation;


namespace factoryos_10x_shell.Library.ViewModels
{
    public partial class Default10xBarViewModel : ObservableObject
    {
        private readonly ITimeService m_timeService;
        private readonly IDispatcherService m_dispatcherService;
        private readonly IThemeService m_themeService;

        private readonly IBatteryService m_powerService;
        private readonly INetworkService m_netService;

        private readonly INotificationManager m_notifManager;
        private readonly IStartManagerService m_startManager;
        private readonly IActionCenterManagerService m_actionManager;

        public Default10xBarViewModel(
            ITimeService timeService,
            IDispatcherService dispatcherService,
            IBatteryService powerService,
            INetworkService netService,
            INotificationManager notifManager,
            IThemeService themeService,
            IStartManagerService startManager,
            IActionCenterManagerService actionManager)
        {
            m_timeService = timeService;
            m_dispatcherService = dispatcherService;
            m_powerService = powerService;
            m_netService = netService;
            m_notifManager = notifManager;
            m_themeService = themeService;
            m_startManager = startManager;
            m_actionManager = actionManager;


            m_timeService.UpdateClockBinding += TimeService_UpdateClockBinding;

            m_powerService.BatteryStatusChanged += BatteryService_BatteryStatusChanged;
            DetectBatteryPresence();

            m_netService.InternetStatusChanged += NetworkService_InternetStatusChanged;
            UpdateNetworkStatus();


            m_themeService.GlobalThemeChanged += ThemeService_GlobalThemeChanged;

            StartColorOpacity = 0;
            StartNormalOpacity = 1;
            m_startManager.StartVisibilityChanged += StartManager_StartVisibilityChanged;

        }


        public Thickness NetworkStatusMargin
        {
            get
            {
                switch (m_netService.InternetType)
                {
                    case InternetConnection.Wired:
                        return new Thickness(0, 0, 10, 6);
                    default:
                        return new Thickness(0, 0, 10, 8);
                }
            }
        }


        [ObservableProperty]
        private string timeText;

        private void TimeService_UpdateClockBinding(object sender, EventArgs e)
        {
            m_dispatcherService.DispatcherQueue.TryEnqueue(() =>
            {
                TimeText = DateTime.Now.ToString(m_timeService.ClockFormat);
            });
        }


        [ObservableProperty]
        private string batteryStatusCharacter;

        [ObservableProperty]
        private Visibility battStatusVisibility;

        private void BatteryService_BatteryStatusChanged(object sender, EventArgs e)
        {
            m_dispatcherService.DispatcherQueue.TryEnqueue(() =>
            {
                AggregateBattery();
            });
        }
        private void DetectBatteryPresence()
        {
            BatteryReport report = m_powerService.GetStatusReport();
            BatteryPowerStatus status = report.PowerStatus;

            if (status == BatteryPowerStatus.NotInstalled)
            {
                BattStatusVisibility = Visibility.Collapsed;
            }
            else
            {
                BattStatusVisibility = Visibility.Visible;
                AggregateBattery();
            }
        }
        private void AggregateBattery()
        {
            BatteryReport report = m_powerService.GetStatusReport();
            double battLevel = report.ChargePercentage;
            BatteryPowerStatus status = report.PowerStatus;
            if (status == BatteryPowerStatus.Charging || status == BatteryPowerStatus.Idle)
            {
                int indexCharge = (int)Math.Floor(battLevel / 10);
                BatteryStatusCharacter = IconConstants.BatteryIconsCharge[indexCharge];
            }
            else
            {
                int indexDischarge = (int)Math.Floor(battLevel / 10);
                BatteryStatusCharacter = IconConstants.BatteryIcons[indexDischarge];
            }
        }


        [ObservableProperty]
        private string networkStatusCharacter;

        [ObservableProperty]
        private string networkStatusBackground;

        private void NetworkService_InternetStatusChanged(object sender, EventArgs e)
        {
            UpdateNetworkStatus();
        }
        private void UpdateNetworkStatus()
        {
            string statusTextBuf = "\uE774";
            string statusBackgroundBuf = "\uE774";
            if (m_netService.IsInternetAvailable)
            {
                switch (m_netService.InternetType)
                {
                    case InternetConnection.Wired:
                        statusBackgroundBuf = "\uE839";
                        statusTextBuf = "\uE839";
                        break;
                    case InternetConnection.Wireless:
                        if (m_netService.SignalStrength < IconConstants.WiFiIcons.Length)
                        {
                            statusBackgroundBuf = "\uE701";
                            statusTextBuf = IconConstants.WiFiIcons[m_netService.SignalStrength];
                        }
                        break;
                    case InternetConnection.Data:
                        if (m_netService.SignalStrength < IconConstants.DataIcons.Length)
                        {
                            statusBackgroundBuf = "\uEC3B";
                            statusTextBuf = IconConstants.DataIcons[m_netService.SignalStrength];
                        }
                        break;
                    case InternetConnection.Unknown:
                    default:
                        statusBackgroundBuf = "\uE774";
                        statusTextBuf = "\uE774";
                        break;
                }
            }
            else
            {
                statusTextBuf = "\uEB55";
                statusBackgroundBuf = "\uEB55";
            }
            m_dispatcherService.DispatcherQueue.TryEnqueue(() =>
            {
                NetworkStatusBackground = statusBackgroundBuf;
                NetworkStatusCharacter = statusTextBuf;
            });
        }




        public SolidColorBrush ThemedIconBrush => m_themeService.CurrentTheme switch
        {
            ApplicationTheme.Light => new SolidColorBrush(Color.FromArgb(255, 99, 99, 98)),
            ApplicationTheme.Dark => new SolidColorBrush(Color.FromArgb(255, 166, 166, 166)),
            _ => new SolidColorBrush(Color.FromArgb(255, 166, 166, 166))
        };

        private void ThemeService_GlobalThemeChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(ThemedIconBrush));
        }


        [ObservableProperty]
        double startColorOpacity;

        [ObservableProperty]
        double startNormalOpacity;

        private void StartManager_StartVisibilityChanged(object sender, Events.StartVisibilityChangedEventArgs e)
        {
            if (e.CurrentVisibility)
            {
                StartColorOpacity = 1;
                StartNormalOpacity = 0;
            }
            else
            {
                StartColorOpacity = 0;
                StartNormalOpacity = 1;
            }
        }

        [RelayCommand]
        private void StartButtonClicked()
        {
            m_startManager.RequestStartVisibilityChange(!m_startManager.IsStartOpen);
            if (m_actionManager.IsActionCenterOpen)
                m_actionManager.RequestActionVisibilityChange(false);
        }


        [RelayCommand]
        private void ActionCenterButtonClicked()
        {
            // Create an instance of InputInjector
            InputInjector inputInjector = InputInjector.TryCreate();

            // Create an instance for the 'A' key
            InjectedInputKeyboardInfo tabKey = new InjectedInputKeyboardInfo();
            tabKey.VirtualKey = (ushort)(VirtualKey.A);
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
        }

        [RelayCommand]
        private void TaskViewButtonClicked()
        {
            // Create an instance of InputInjector
            InputInjector inputInjector = InputInjector.TryCreate();

            // Create an instance for the 'Tab' key
            InjectedInputKeyboardInfo tabKey = new InjectedInputKeyboardInfo();
            tabKey.VirtualKey = (ushort)(VirtualKey.Tab);
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

        }

        [RelayCommand]
        private void SearchButtonClicked()
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