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
        public ICommand GenerateOrderCommand { get; }

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
            GenerateOrderCommand = new RelayCommand(ExecuteGenerateOrder, CanExecuteGenerateOrder);

            InitializeAsync();
            InitializeCollectionHandlers();
        }

        private void InitializeCollectionHandlers()
        {
            if (PriceItems != null)
            {
                PriceItems.CollectionChanged += (s, e) =>
                {
                    if (e.NewItems != null)
                    {
                        foreach (PriceItem item in e.NewItems)
                        {
                            item.PropertyChanged += Item_PropertyChanged;
                        }
                    }
                    if (e.OldItems != null)
                    {
                        foreach (PriceItem item in e.OldItems)
                        {
                            item.PropertyChanged -= Item_PropertyChanged;
                        }
                    }
                    NotifySummaryChanges();
                };

                foreach (var item in PriceItems)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }
            }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PriceItem.Volume) ||
                e.PropertyName == nameof(PriceItem.Pieces) ||
                e.PropertyName == nameof(PriceItem.Discount) ||
                e.PropertyName == nameof(PriceItem.TotalPrice))
            {
                NotifySummaryChanges();
            }
        }

        private bool CanExecuteGenerateOrder()
        {
            return HasItems;
        }

        private void ExecuteGenerateOrder()
        {
            MessageBox.Show("Funkcja generowania zamówienia będzie dostępna wkrótce.", "Informacja",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void InitializeAsync()
        {
            try
            {
                await _priceService.LoadPriceItemsAsync();
                NotifyHasItemsChanged();
                InitializeCollectionHandlers();
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
                NotifySummaryChanges();
            }
        }

        private void ExecuteRemoveItem(PriceItem item)
        {
            if (item == null) return;
            item.PropertyChanged -= Item_PropertyChanged;
            _priceService.RemoveItem(item);
            NotifyHasItemsChanged();
            NotifySummaryChanges();
        }

        private void ExecuteClearItems()
        {
            if (MessageDialog.Show(
                "Czy na pewno chcesz wyczyścić wszystkie produkty?",
                "Potwierdzenie"))
            {
                foreach (var item in PriceItems)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                }
                _priceService.ClearItems();
                NotifyHasItemsChanged();
                NotifySummaryChanges();
            }
        }

        public decimal TotalVolume
        {
            get => PriceItems?.Sum(item => item.Volume ?? 0) ?? 0;
        }

        public int TotalPieces
        {
            get => PriceItems?.Sum(item => item.Pieces ?? 0) ?? 0;
        }

        public decimal TotalDiscountAmount
        {
            get
            {
                if (!PriceItems?.Any() ?? true) return 0;

                decimal totalDiscount = 0;
                foreach (var item in PriceItems)
                {
                    if (item.Volume >= item.Product.Discount && item.Volume.HasValue && item.Discount.HasValue)
                    {
                        decimal priceBeforeDiscount = item.Volume.Value * item.Product.PricePerM3;
                        decimal discountMultiplier = 1 - item.Discount.Value / 100m;
                        decimal priceAfterDiscount = priceBeforeDiscount * discountMultiplier;

                        totalDiscount += priceBeforeDiscount - priceAfterDiscount;
                    }
                }
                return totalDiscount;
            }
        }

        public decimal TotalPrice
        {
            get => PriceItems?.Sum(item => item.TotalPrice) ?? 0;
        }

        private void NotifySummaryChanges()
        {
            _dispatcher.Invoke(() =>
            {
                OnPropertyChanged(nameof(TotalVolume));
                OnPropertyChanged(nameof(TotalPieces));
                OnPropertyChanged(nameof(TotalDiscountAmount));
                OnPropertyChanged(nameof(TotalPrice));
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (propertyName == nameof(PriceItems))
            {
                NotifySummaryChanges();
            }
        }
    }
}