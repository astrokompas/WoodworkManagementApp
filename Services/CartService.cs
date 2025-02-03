using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Threading;
using System.Windows;
using WoodworkManagementApp.Models;
using WoodworkManagementApp.Services;


namespace WoodworkManagementApp.Services
{
    public class CartService : ICartService
    {
        private readonly IJsonStorageService _jsonStorage;
        private ObservableCollection<CartItem> _cartItems;
        private const string CART_FILE = "cart.json";
        private readonly Dispatcher _dispatcher;

        public CartService(IJsonStorageService jsonStorage)
        {
            _jsonStorage = jsonStorage;
            _dispatcher = Application.Current.Dispatcher;
            _cartItems = new ObservableCollection<CartItem>();
            LoadCartAsync().ConfigureAwait(false);
        }

        public ObservableCollection<CartItem> CartItems => _cartItems;

        public void AddItem(CartItem item)
        {
            _dispatcher.Invoke(() =>
            {
                var newItem = new CartItem
                {
                    ProductName = item.ProductName,
                    Volume = item.Volume,
                    PricePerM3 = item.PricePerM3,
                    Type = item.Type
                };
                _cartItems.Add(newItem);
            });
            SaveCartAsync().ConfigureAwait(false);
        }

        public void AddItems(IEnumerable<CartItem> items)
        {
            _dispatcher.Invoke(() =>
            {
                foreach (var item in items)
                {
                    var newItem = new CartItem
                    {
                        ProductName = item.ProductName,
                        Volume = item.Volume,
                        PricePerM3 = item.PricePerM3,
                        Type = item.Type,
                        Quantity = item.Quantity
                    };
                    _cartItems.Add(newItem);
                }
            });
            SaveCartAsync().ConfigureAwait(false);
        }

        public void RemoveItem(CartItem item)
        {
            _dispatcher.Invoke(() => _cartItems.Remove(item));
            SaveCartAsync().ConfigureAwait(false);
        }

        public void ClearCart()
        {
            _dispatcher.Invoke(() => _cartItems.Clear());
            SaveCartAsync().ConfigureAwait(false);
        }

        public async Task LoadCartAsync()
        {
            try
            {
                var items = await _jsonStorage.LoadAsync<List<CartItem>>(CART_FILE);
                await _dispatcher.InvokeAsync(() =>
                {
                    _cartItems.Clear();
                    foreach (var item in items ?? Enumerable.Empty<CartItem>())
                    {
                        _cartItems.Add(item);
                    }
                });
            }
            catch
            {
                await _dispatcher.InvokeAsync(() => _cartItems.Clear());
            }
        }

        public async Task SaveCartAsync()
        {
            try
            {
                await _jsonStorage.SaveAsync(_cartItems.ToList(), CART_FILE);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to save cart", ex);
            }
        }
    }
}
