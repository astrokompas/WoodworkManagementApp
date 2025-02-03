using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using WoodworkManagementApp.Helpers;
using WoodworkManagementApp.Models;
using WoodworkManagementApp.Services;
using WoodworkManagementApp.Dialogs;
using System.Windows;

namespace WoodworkManagementApp.ViewModels
{
    public class PricePageViewModel : INotifyPropertyChanged
    {
        private readonly IProductService _productService;
        private readonly IPriceService _priceService;
        private readonly IProductsViewModel _productsViewModel;
        private readonly Dispatcher _dispatcher;

        public ICommand AddProductsCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand ClearItemsCommand { get; }

        public PricePageViewModel(
            IProductService productService,
            IPriceService priceService,
            IProductsViewModel productsViewModel)
        {
            _productService = productService;
            _priceService = priceService;
            _productsViewModel = productsViewModel;
            _dispatcher = Application.Current.Dispatcher;

            AddProductsCommand = new RelayCommand(ExecuteAddProducts);
            RemoveItemCommand = new RelayCommand<PriceItem>(ExecuteRemoveItem);
            ClearItemsCommand = new RelayCommand(ExecuteClearItems);

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                await _priceService.LoadPriceItemsAsync();
                NotifyHasItemsChanged();
            }
            catch (Exception ex)
            {
                await _dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Error loading price items: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        public ObservableCollection<PriceItem> PriceItems => _priceService.PriceItems;

        public bool HasItems => PriceItems.Count > 0;

        private void NotifyHasItemsChanged()
        {
            OnPropertyChanged(nameof(HasItems));
        }

        private void ExecuteAddProducts()
        {
            var dialog = new MultiSelectProductDialog(_productsViewModel);
            if (dialog.ShowDialog() == true)
            {
                foreach (var product in dialog.SelectedProducts)
                {
                    _priceService.AddItem(product);
                }
                NotifyHasItemsChanged();
            }
        }

        private void ExecuteRemoveItem(PriceItem item)
        {
            if (item == null) return;
            _priceService.RemoveItem(item);
            NotifyHasItemsChanged();
        }

        private void ExecuteClearItems()
        {
            if (MessageDialog.Show(
                "Czy na pewno chcesz wyczyścić wszystkie produkty?",
                "Potwierdzenie"))
            {
                _priceService.ClearItems();
                NotifyHasItemsChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}