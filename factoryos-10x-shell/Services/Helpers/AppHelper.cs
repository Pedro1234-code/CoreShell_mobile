using factoryos_10x_shell.Library.Events;
using factoryos_10x_shell.Library.Models.InternalData;
using factoryos_10x_shell.Library.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Management.Deployment;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Newtonsoft.Json;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;


namespace factoryos_10x_shell.Services.Helpers
{
    public class AppHelper : IAppHelper
    {
        private DispatcherQueue m_dispatcherQueue;

        private Size _logoSize;
        public ObservableCollection<StartIconModel> StartIcons { get; set; }
        public ObservableCollection<StartIconModel> TaskbarIcons { get; set; } = new ObservableCollection<StartIconModel>();

        private List<StartIconModel> _iconCache { get; set; }

        public PackageManager PackageManager { get; set; }

        private Dictionary<string, string> m_pkgFamilyMap;

        public AppHelper() 
        {
            m_dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            PackageManager = new PackageManager();
            m_pkgFamilyMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            _logoSize = new Size(176, 176);
            _iconCache = new List<StartIconModel>();
        }

        public List<StartIconModel> LoadTaskbarPinnedApps()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            var list = new List<StartIconModel>();

            var pinned = localSettings.Values
                .Where(kv => kv.Key.StartsWith("PinnedApp_"))
                .OrderBy(kv => kv.Key) // garante ordem correta
                .Select(kv => kv.Value as ApplicationDataCompositeValue);
            
            foreach (var composite in pinned)
            {
                var model = new StartIconModel
                {
                    AppId = composite["AppId"] as string,
                    Aumid = composite["Aumid"] as string,
                    IconName = composite["IconName"] as string,
                    AppUri = composite["AppUri"] as string
                };

                list.Add(model);
            }

            return list;
        }


        public void SaveTaskbarPinnedApps(List<StartIconModel> apps)
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            // Limpa apps antigos
            var keysToRemove = localSettings.Values.Keys.Where(k => k.StartsWith("PinnedApp_")).ToList();
            foreach (var key in keysToRemove)
            {
                localSettings.Values.Remove(key);
            }

            // Salva os novos
            for (int i = 0; i < apps.Count; i++)
            {
                var app = apps[i];
                var composite = new ApplicationDataCompositeValue
                {
                    ["AppId"] = app.AppId,
                    ["Aumid"] = app.Aumid,
                    ["IconName"] = app.IconName,
                    ["AppUri"] = app.AppUri
                };

                localSettings.Values[$"PinnedApp_{i}"] = composite;
            }
        }

        public async Task<StartIconModel> CreateStartIconModelFromEntryAsync(AppListEntry entry)
        {
            var displayInfo = entry.DisplayInfo;
            var logoRef = displayInfo.GetLogo(new Windows.Foundation.Size(64, 64));
            var stream = await logoRef.OpenReadAsync();

            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream);

            string appUserModelId = entry.AppUserModelId;
            string aumid = null;

            try
            {
                // Extrair AUMID real
                string[] parts = appUserModelId.Split('!');
                if (parts.Length == 2)
                {
                    string packageFamilyName = parts[0];
                    string appId = parts[1];
                    aumid = $"{packageFamilyName}!{appId}";
                }
                else
                {
                    Debug.WriteLine($"[ERRO] AppUserModelId inválido: {appUserModelId}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERRO] Falha ao construir AUMID: {ex.Message}");
            }

            return new StartIconModel
            {
                AppId = appUserModelId,
                Aumid = aumid,
                IconName = displayInfo.DisplayName,
                IconSource = bitmap,
                Data = entry
            };
        }

        public async Task LoadPinnedTaskbarAppsAsync()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            var loaded = new List<StartIconModel>();

            foreach (var key in localSettings.Values.Keys)
            {
                if (!key.StartsWith("PinnedApp_")) continue;

                var composite = localSettings.Values[key] as ApplicationDataCompositeValue;
                if (composite == null) continue;

                var appId = composite["AppId"] as string;
                var aumid = composite["Aumid"] as string;
                var iconName = composite["IconName"] as string;
                var appUri = composite["AppUri"] as string;

                // Tenta obter a AppListEntry correspondente
                var entry = await GetAppListEntryFromAumidAsync(aumid);
                if (entry == null)
                {
                    Debug.WriteLine($"[ERRO] Não foi possível recuperar AppListEntry para {aumid}");
                    continue;
                }

                var model = await CreateStartIconModelFromEntryAsync(entry);

                // Restaurar dados extras
                model.IconName = iconName;
                model.AppUri = appUri;

                loaded.Add(model);
            }

            var dispatcher = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher;

            await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                TaskbarIcons.Clear();
                foreach (var item in loaded)
                    TaskbarIcons.Add(item);
            });

            Debug.WriteLine($"[INFO] Loaded pinned apps count: {loaded.Count}");
        }



        public async Task LoadAppsAsync()
        {
            try
            {
                _iconCache.Clear(); // Clear the cache before reloading
                PackageManager packageManager = new PackageManager();
                IEnumerable<Package> packages = packageManager.FindPackagesForUser("");

                RandomAccessStreamReference logoData;

                foreach (Package package in packages)
                {
                    if (!package.IsFramework && !package.IsResourcePackage && !package.IsStub && package.GetAppListEntries().FirstOrDefault() != null)
                    {
                        m_pkgFamilyMap[package.Id.FamilyName] = package.Id.FullName;
                        try
                        {
                            IReadOnlyList<AppListEntry> entries = package.GetAppListEntries();
                            foreach (AppListEntry entry in entries)
                            {
                                logoData = package.GetLogoAsRandomAccessStreamReference(_logoSize);
                                IRandomAccessStreamWithContentType stream = await logoData.OpenReadAsync();
                                BitmapImage bitmapImage = new BitmapImage();
                                await bitmapImage.SetSourceAsync(stream);
                                _iconCache.Add(new StartIconModel { IconName = entry.DisplayInfo.DisplayName, AppId = entry.AppUserModelId, IconSource = bitmapImage, Data = entry });
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error accessing logo for package {package.Id.FullName}: {ex.Message}");
                        }
                    }
                }

                StartIcons = new ObservableCollection<StartIconModel>(_iconCache.OrderBy(icon => icon.IconName));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LoadApps => Get: " + ex.Message);
            }
        }
        public Package PackageFromAumid(string aumid)
        {
            string[] aumidParts = aumid.Split('!');
            string packageFamilyName = aumidParts[0];

            if (m_pkgFamilyMap.TryGetValue(packageFamilyName, out string packageFullName))
            {
                return PackageManager.FindPackageForUser(string.Empty, packageFullName);
            }
            return null;
        }

        public async Task<AppListEntry> GetAppListEntryFromAumidAsync(string aumid)
        {
            var pm = new PackageManager();
            var packages = pm.FindPackagesForUserWithPackageTypes("", PackageTypes.Main);

            foreach (var package in packages)
            {
                try
                {
                    var entries = await package.GetAppListEntriesAsync();
                    foreach (var entry in entries)
                    {
                        if (entry.AppUserModelId == aumid)
                        {
                            return entry;
                        }
                    }
                }
                catch
                {
                    Debug.WriteLine($"[AppDisplayInfo] Failed to convert AUMID");
                }
            }

            return null;
        }

        public async Task<AppListEntry> GetAppListEntryFromModelAsync(StartIconModel model)
        {
            if (string.IsNullOrEmpty(model.AppId))
                return null;

            var pm = new PackageManager();
            var allPackages = pm.FindPackagesForUser(string.Empty);

            foreach (var pkg in allPackages)
            {
                IReadOnlyList<AppListEntry> entries = null;

                try
                {
                    entries = await pkg.GetAppListEntriesAsync();
                }
                catch
                {
                    continue;
                }

                foreach (var entry in entries)
                {
                    if (entry.AppUserModelId == model.AppId || entry.DisplayInfo.DisplayName == model.Name)
                    {
                        return entry;
                    }
                }
            }

            return null;
        }
    }
}
