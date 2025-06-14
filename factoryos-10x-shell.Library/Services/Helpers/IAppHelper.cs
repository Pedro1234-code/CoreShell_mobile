using factoryos_10x_shell.Library.Events;
using factoryos_10x_shell.Library.Models.InternalData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Management.Deployment;

namespace factoryos_10x_shell.Library.Services.Helpers
{
    /// <summary>
    /// A service that provides data about the UWP apps on the system.
    /// </summary>
    public interface IAppHelper
    {
        ObservableCollection<StartIconModel> StartIcons { get; set; }
        ObservableCollection<StartIconModel> TaskbarIcons { get; set; }
        Task LoadAppsAsync();

        // void SaveTaskbarPinnedApps(List<AppInfo> apps);

        void SaveTaskbarPinnedApps(List<StartIconModel> startIconModels);

        Package PackageFromAumid(string aumid);
        Task<AppListEntry> GetAppListEntryFromAumidAsync(string aumid);
        Task<AppListEntry> GetAppListEntryFromModelAsync(StartIconModel model);
    }
}
