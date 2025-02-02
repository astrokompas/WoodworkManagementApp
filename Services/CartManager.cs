using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoodworkManagementApp.Models;
using WoodworkManagementApp.Services;


namespace WoodworkManagementApp.Services
{
    public class CartManager : ICartManager
    {
        private readonly IJsonStorageService _jsonStorage;

        public CartManager(IJsonStorageService jsonStorage)
        {
            _jsonStorage = jsonStorage;
        }

        public async Task SaveCartStateAsync(ObservableCollection<CartItem> items)
        {
            await _jsonStorage.SaveAsync(items.ToList());
        }

        public async Task<ObservableCollection<CartItem>> LoadCartStateAsync()
        {
            var items = await _jsonStorage.LoadAsync<List<CartItem>>();
            return new ObservableCollection<CartItem>(items ?? new List<CartItem>());
        }

        public async Task ClearCartStateAsync()
        {
            await _jsonStorage.SaveAsync(new List<CartItem>());
        }
    }
}
