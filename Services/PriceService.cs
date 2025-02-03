using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using WoodworkManagementApp.Models;
using System.Windows;


namespace WoodworkManagementApp.Services
{
    public class PriceService : IPriceService
    {
        private readonly IJsonStorageService _jsonStorage;
        private ObservableCollection<PriceItem> _priceItems;
        private const string PRICE_ITEMS_FILE = "price_items.json";
        private readonly Dispatcher _dispatcher;

        public PriceService(IJsonStorageService jsonStorage)
        {
            _jsonStorage = jsonStorage;
            _dispatcher = Application.Current.Dispatcher;
            _priceItems = new ObservableCollection<PriceItem>();
            LoadPriceItemsAsync().ConfigureAwait(false);
        }

        public ObservableCollection<PriceItem> PriceItems => _priceItems;

        public void AddItem(Product product, decimal? volume = null, int? pieces = null)
        {
            var newItem = new PriceItem
            {
                Product = product,
                Volume = volume,
                Pieces = pieces
            };

            _dispatcher.Invoke(() =>
            {
                _priceItems.Add(newItem);
                SavePriceItemsAsync().ConfigureAwait(false);
            });
        }

        public void RemoveItem(PriceItem item)
        {
            _dispatcher.Invoke(() => _priceItems.Remove(item));
            SavePriceItemsAsync().ConfigureAwait(false);
        }

        public void ClearItems()
        {
            _dispatcher.Invoke(() => _priceItems.Clear());
            SavePriceItemsAsync().ConfigureAwait(false);
        }

        public async Task LoadPriceItemsAsync()
        {
            try
            {
                var items = await _jsonStorage.LoadAsync<List<PriceItem>>(PRICE_ITEMS_FILE);
                await _dispatcher.InvokeAsync(() =>
                {
                    _priceItems.Clear();
                    foreach (var item in items ?? Enumerable.Empty<PriceItem>())
                    {
                        _priceItems.Add(item);
                    }
                });
            }
            catch
            {
                await _dispatcher.InvokeAsync(() => _priceItems.Clear());
            }
        }

        public async Task SavePriceItemsAsync()
        {
            try
            {
                await _jsonStorage.SaveAsync(_priceItems.ToList(), PRICE_ITEMS_FILE);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to save price items", ex);
            }
        }
    }
}
