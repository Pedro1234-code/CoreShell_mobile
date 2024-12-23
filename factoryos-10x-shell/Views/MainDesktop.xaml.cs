﻿using factoryos_10x_shell.Library.Services.Managers;
using factoryos_10x_shell.Library.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Microsoft.Win32;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.System;
using Windows.UI.Input.Preview.Injection;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using System.Runtime.InteropServices;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.System.UserProfile;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.Threading.Tasks;
using factoryos_10x_shell.Services.Helpers;

namespace factoryos_10x_shell.Views
{
    public sealed partial class MainDesktop : Page
    {
        private Storyboard OpenStartStoryboard { get; set; }
        private Storyboard CloseStartStoryboard { get; set; }

        private Storyboard OpenActionStoryboard { get; set; }
        private Storyboard CloseActionStoryboard { get; set; }


        private readonly IStartManagerService m_startManager;
        private readonly IActionCenterManagerService m_actionManager;

        public MainDesktop()
        {
            this.InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<MainDesktopViewModel>();

            m_startManager = App.ServiceProvider.GetRequiredService<IStartManagerService>();
            m_startManager.StartVisibilityChanged += StartManager_StartVisibilityChanged;

            m_actionManager = App.ServiceProvider.GetRequiredService<IActionCenterManagerService>();
            m_actionManager.ActionVisibilityChanged += ActionCenterManager_ActionVisibilityChanged;

            LoadBackgroundImage();

            this.PointerPressed += OnPointerPressed;

            TaskbarFrame.Navigate(typeof(Default10xBar));
            StartMenuFrame.Navigate(typeof(StartMenu));

            InitStartOpen();
            InitStartClose();

            InitActionOpen();
            InitActionClose();


            AppState.Instance.OnBgChangeButtonVisibilityChanged += UpdateBgChangeButtonVisibility;
            UpdateBgChangeButtonVisibility(AppState.Instance.IsBgChangeButtonVisible);
        }

        private void UpdateBgChangeButtonVisibility(bool isVisible)
        {
            BgChangebutton.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;

        }

            private async void LoadBackgroundImage()
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("backgroundImagePath"))
            {
                string imagePath = localSettings.Values["backgroundImagePath"].ToString();
                if (!string.IsNullOrEmpty(imagePath))
                {
                    var file = await StorageFile.GetFileFromPathAsync(imagePath);
                    if (file != null)
                    {
                        var bitmapImage = new BitmapImage();
                        using (var stream = await file.OpenAsync(FileAccessMode.Read))
                        {
                            await bitmapImage.SetSourceAsync(stream);
                        }
                        BackgroundWallpaper.Source = bitmapImage;
                    }
                }
            }
        }

        public Image BackgroundWallpaperControl => BackgroundWallpaper;

        private void OnPointerPressed(object sender, PointerRoutedEventArgs args)
        {
            PointerPoint point = args.GetCurrentPoint(BackgroundWallpaper);

            if (m_actionManager.IsActionCenterOpen)
            {
                Rect actionCenterBounds = ActionCenterFrame.TransformToVisual(null).TransformBounds(new Rect(0, 0, ActionCenterFrame.ActualWidth, ActionCenterFrame.ActualHeight));

                if (!actionCenterBounds.Contains(point.Position))
                {
                    m_actionManager.RequestActionVisibilityChange(false);
                }
            }

            if (m_startManager.IsStartOpen)
            {
                Rect startMenuBounds = StartMenuFrame.TransformToVisual(null).TransformBounds(new Rect(0, 0, StartMenuFrame.ActualWidth, StartMenuFrame.ActualHeight));

                if (!startMenuBounds.Contains(point.Position))
                {
                    m_startManager.RequestStartVisibilityChange(false);
                }
            }
        }



        private async void BgChangebutton_Click(object sender, RoutedEventArgs e)
        {
            // Create a file picker
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            // Let the user pick a file
            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                // Save the file path to local settings
                Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values["backgroundImagePath"] = file.Path;

                // Set the selected image as the source of the BackgroundWallpaper image
                var bitmapImage = new BitmapImage();
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    await bitmapImage.SetSourceAsync(stream);
                }
                BackgroundWallpaper.Source = bitmapImage;
            }
        }


        public MainDesktopViewModel ViewModel => (MainDesktopViewModel)this.DataContext;


        private void InitActionOpen()
        {
            DoubleAnimation slideInAnimation = new DoubleAnimation
            {
                From = 800,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(slideInAnimation, ActionCenterTransform);
            Storyboard.SetTargetProperty(slideInAnimation, "Y");

            OpenActionStoryboard = new Storyboard();
            OpenActionStoryboard.Children.Add(slideInAnimation);
        }
        private void InitActionClose()
        {
            DoubleAnimation slideOutAnimation = new DoubleAnimation
            {
                From = 0,
                To = 800,
                Duration = new Duration(TimeSpan.FromSeconds(0.20)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(slideOutAnimation, ActionCenterTransform);
            Storyboard.SetTargetProperty(slideOutAnimation, "Y");

            CloseActionStoryboard = new Storyboard();
            CloseActionStoryboard.Children.Add(slideOutAnimation);
        }

        private void InitStartOpen()
        {
            DoubleAnimation slideInAnimation = new DoubleAnimation
            {
                From = 800,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(slideInAnimation, StartMenuTransform);
            Storyboard.SetTargetProperty(slideInAnimation, "Y");

            OpenStartStoryboard = new Storyboard();
            OpenStartStoryboard.Children.Add(slideInAnimation);
        }
        private void InitStartClose()
        {
            DoubleAnimation slideOutAnimation = new DoubleAnimation
            {
                From = 0,
                To = 800,
                Duration = new Duration(TimeSpan.FromSeconds(0.20)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(slideOutAnimation, StartMenuTransform);
            Storyboard.SetTargetProperty(slideOutAnimation, "Y");

            CloseStartStoryboard = new Storyboard();
            CloseStartStoryboard.Children.Add(slideOutAnimation);
        }


        private void StartManager_StartVisibilityChanged(object sender, Library.Events.StartVisibilityChangedEventArgs e)
        {
            if (e.CurrentVisibility) { OpenStartStoryboard.Begin(); }
            else { CloseStartStoryboard.Begin(); }
        }

        private void ActionCenterManager_ActionVisibilityChanged(object sender, Library.Events.ActionCenterVisibilityChangedEventArgs e)
        {
            if (e.CurrentVisibility) { OpenActionStoryboard.Begin(); }
            else { CloseActionStoryboard.Begin(); }
        }
    }
}
