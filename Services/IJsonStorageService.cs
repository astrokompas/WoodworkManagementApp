using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace WoodworkManagementApp.Services
{
    public interface IJsonStorageService
    {
        Task SaveAsync<T>(T data, string fileName = "data.json");
        Task<T> LoadAsync<T>(string fileName = "data.json") where T : new();
    }
}
