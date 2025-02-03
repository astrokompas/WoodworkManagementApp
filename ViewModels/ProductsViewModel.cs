using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WoodworkManagementApp.Services;
using System.Windows.Data;
using System.Windows.Input;
using WoodworkManagementApp.Helpers;
using System.Windows;
using WoodworkManagementApp.Dialogs;
using System.Windows.Threading;
using WoodworkManagementApp.Models;


namespace WoodworkManagementApp.ViewModels
{
    public class ProductsViewModel : IProductsViewModel, INotifyPropertyChanged
    {
        private readonly IProductService _productService;
        private readonly IDialogService _dialogService;
        private ObservableCollection<Product> _products;
        private string _searchText;
        private ICollectionView _productsView;
        private readonly Dispatcher _dispatcher;

        public ICommand AddProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand DuplicateProductCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand ImportExcelCommand { get; }
        public ICommand ExportExcelCommand { get; }
        public ICommand ClearSearchCommand { get; }

        public ProductsViewModel(IProductService productService)
        {
            _productService = productService;
            _dialogService = new DialogService();
            _dispatcher = Application.Current.Dispatcher;
            _products = new ObservableCollection<Product>();

            AddProductCommand = new RelayCommand(ExecuteAddProduct);
            EditProductCommand = new RelayCommand<Product>(ExecuteEditProduct);
            DeleteProductCommand = new RelayCommand<Product>(ExecuteDeleteProduct);
            DuplicateProductCommand = new RelayCommand<Product>(ExecuteDuplicateProduct);
            MoveUpCommand = new RelayCommand<Product>(ExecuteMoveUp);
            MoveDownCommand = new RelayCommand<Product>(ExecuteMoveDown);
            ImportExcelCommand = new RelayCommand(ExecuteImportExcel);
            ExportExcelCommand = new RelayCommand(ExecuteExportExcel);
            ClearSearchCommand = new RelayCommand(ExecuteClearSearch);
        }

        public async Task InitializeAsync()
        {
            if (_products.Count > 0) return;

            try
            {
                var loadedProducts = await _productService.LoadProductsAsync();
                await _dispatcher.InvokeAsync(() =>
                {
                    Products = new ObservableCollection<Product>(loadedProducts);
                    ProductsView = CollectionViewSource.GetDefaultView(Products);
                    ProductsView.Filter = FilterProducts;
                });
            }
            catch (Exception ex)
            {
                await _dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Error loading products: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        public ObservableCollection<Product> Products
        {
            get => _products;
            set
            {
                _products = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView ProductsView
        {
            get => _productsView;
            set
            {
                _productsView = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                ProductsView?.Refresh();
                OnPropertyChanged();
            }
        }

        private bool FilterProducts(object item)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return true;

            if (item is Product product)
            {
                return product.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private async void ExecuteAddProduct()
        {
            var dialog = new AddProductDialog();
            if (dialog.ShowDialog() == true)
            {
                var product = dialog.Product;
                product.Order = Products.Count;

                await _dispatcher.InvokeAsync(() =>
                {
                    Products.Add(product);
                });
                await SaveChanges();
            }
        }

        private async void ExecuteEditProduct(Product product)
        {
            if (product == null) return;

            var dialog = new EditProductDialog(product);
            if (dialog.ShowDialog() == true)
            {
                await SaveChanges();
                ProductsView.Refresh();
            }
        }

        private async void ExecuteDeleteProduct(Product product)
        {
            if (product == null) return;

            bool confirmed = MessageDialog.Show(
                $"Czy na pewno chcesz usunąć produkt \"{product.Name}\"?",
                "Usuwanie produktu");

            if (confirmed)
            {
                await _dispatcher.InvokeAsync(() =>
                {
                    Products.Remove(product);
                    UpdateOrders();
                });
                await SaveChanges();
            }
        }

        private async void ExecuteDuplicateProduct(Product product)
        {
            if (product == null) return;

            var duplicate = new Product
            {
                Name = $"{product.Name} (Copy)",
                PricePerM3 = product.PricePerM3,
                Discount = product.Discount,
                Category = product.Category,
                Order = Products.Count
            };

            await _dispatcher.InvokeAsync(() =>
            {
                Products.Add(duplicate);
            });
            await SaveChanges();
        }

        private async void ExecuteImportExcel()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                Title = "Select Excel File"
            };
            if (dialog.ShowDialog() == true)
            {
                if (MessageDialog.Show("Czy na pewno chcesz zaimportować produkty? Ta operacja nadpisze istniejące dane.", "Potwierdzenie importu"))
                {
                    try
                    {
                        await _productService.ImportFromExcelAsync(dialog.FileName);
                        await InitializeAsync();
                        ConfirmationDialog.Show("Produkty zostały zaimportowane pomyślnie!", "Sukces");
                    }
                    catch (Exception ex)
                    {
                        ConfirmationDialog.Show($"Błąd podczas importowania produktów: {ex.Message}", "Błąd");
                    }
                }
            }
        }

        private async void ExecuteExportExcel()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                DefaultExt = ".xlsx",
                Title = "Save Excel File"
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    await _productService.ExportToExcelAsync(dialog.FileName, Products);
                    ConfirmationDialog.Show("Produkty zostały wyeksportowane pomyślnie!", "Sukces");
                }
                catch (Exception ex)
                {
                    ConfirmationDialog.Show($"Błąd podczas eksportowania produktów: {ex.Message}", "Błąd");
                }
            }
        }

        private async void ExecuteMoveUp(Product product)
        {
            if (product == null || product.Order <= 0) return;

            var index = product.Order;
            var otherProduct = Products.FirstOrDefault(p => p.Order == index - 1);
            if (otherProduct != null)
            {
                product.Order--;
                otherProduct.Order++;
                await _dispatcher.InvokeAsync(() =>
                {
                    UpdateCollectionOrder();
                });
                await SaveChanges();
            }
        }

        private async void ExecuteMoveDown(Product product)
        {
            if (product == null || product.Order >= Products.Count - 1) return;

            var index = product.Order;
            var otherProduct = Products.FirstOrDefault(p => p.Order == index + 1);
            if (otherProduct != null)
            {
                product.Order++;
                otherProduct.Order--;
                await _dispatcher.InvokeAsync(() =>
                {
                    UpdateCollectionOrder();
                });
                await SaveChanges();
            }
        }

        private void ExecuteClearSearch()
        {
            SearchText = string.Empty;
        }

        private void UpdateCollectionOrder()
        {
            var orderedProducts = Products.OrderBy(p => p.Order).ToList();
            Products.Clear();
            foreach (var product in orderedProducts)
            {
                Products.Add(product);
            }
        }

        private void UpdateOrders()
        {
            for (int i = 0; i < Products.Count; i++)
            {
                Products[i].Order = i;
            }
        }

        private async Task SaveChanges()
        {
            try
            {
                await _productService.SaveProductsAsync(Products);
            }
            catch (Exception ex)
            {
                await _dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Error saving products: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
