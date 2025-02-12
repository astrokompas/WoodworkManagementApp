using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoodworkManagementApp.Models;

namespace WoodworkManagementApp.Services
{
    public interface ISettingsService
    {
        Task<AppSettings> LoadSettingsAsync();
        Task SaveSettingsAsync(AppSettings settings);
        Task ValidateSettingsAsync(AppSettings settings);
        bool IsOrdersFolderAvailable();
        void EnsureDirectoriesExist();
    }
}
