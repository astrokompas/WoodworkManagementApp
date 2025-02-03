using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using WoodworkManagementApp.Helpers;
using WoodworkManagementApp.Models;
using WoodworkManagementApp.Services;
using WoodworkManagementApp.ViewModels;
using System.Windows;
using WoodworkManagementApp.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;


namespace WoodworkManagementApp.ViewModels
{
    public class CalcPageViewModel : INotifyPropertyChanged
    {
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly IProductsViewModel _productsViewModel;
        private readonly Dispatcher _dispatcher;
        private bool _isCartOpen;

        private string _millimetersInput;
        private string _centimetersInput;
        private string _decimetersInput;
        private decimal _metersFromMm;
        private decimal _metersFromCm;
        private decimal _metersFromDm;

        private string _width;
        private string _height;
        private string _length;
        private string _quantity;
        private decimal _cuboidVolume;
        private Product _selectedCuboidProduct;

        private string _radius;
        private string _cylinderLength;
        private string _cylinderQuantity;
        private decimal _cylinderVolume;
        private Product _selectedCylinderProduct;

        private ObservableCollection<Product> _selectedCuboidProducts;
        private ObservableCollection<Product> _selectedCylinderProducts;
        public bool HasSelectedCuboidProducts => SelectedCuboidProducts?.Any() == true;
        public bool HasSelectedCylinderProducts => SelectedCylinderProducts?.Any() == true;

        public ICommand AddCuboidToCartCommand { get; }
        public ICommand AddCylinderToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand ToggleCartCommand { get; }
        public ICommand ChooseCuboidProductCommand { get; }
        public ICommand ChooseCylinderProductCommand { get; }
        public ICommand CalculateCommand { get; }

        public CalcPageViewModel(IProductService productService, ICartService cartService, IProductsViewModel productsViewModel)
        {
            _productService = productService;
            _cartService = cartService;
            _productsViewModel = productsViewModel;
            _dispatcher = Application.Current.Dispatcher;
            _selectedCuboidProducts = new ObservableCollection<Product>();
            _selectedCylinderProducts = new ObservableCollection<Product>();

            AddCuboidToCartCommand = new RelayCommand(ExecuteAddCuboidToCart);
            AddCylinderToCartCommand = new RelayCommand(ExecuteAddCylinderToCart);
            RemoveFromCartCommand = new RelayCommand<CartItem>(ExecuteRemoveFromCart);
            ClearCartCommand = new RelayCommand(ExecuteClearCart);
            ToggleCartCommand = new RelayCommand(ExecuteToggleCart);
            ChooseCuboidProductCommand = new RelayCommand(ExecuteChooseCuboidProduct);
            ChooseCylinderProductCommand = new RelayCommand(ExecuteChooseCylinderProduct);

            CalculateCommand = new RelayCommand(ExecuteCalculate);

            Products = _productsViewModel.Products;

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                await _cartService.LoadCartAsync();
            }
            catch (Exception ex)
            {
                await _dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Error loading cart: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        public ObservableCollection<Product> SelectedCuboidProducts
        {
            get => _selectedCuboidProducts;
            set
            {
                _selectedCuboidProducts = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedCuboidProducts));
            }
        }

        public ObservableCollection<Product> SelectedCylinderProducts
        {
            get => _selectedCylinderProducts;
            set
            {
                _selectedCylinderProducts = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedCylinderProducts));
            }
        }

        public bool IsCartOpen
        {
            get => _isCartOpen;
            set
            {
                _isCartOpen = value;
                OnPropertyChanged();
            }
        }

        public string MillimetersInput
        {
            get => _millimetersInput;
            set
            {
                _millimetersInput = value;
                if (decimal.TryParse(value, out decimal mm))
                    MetersFromMm = Math.Round(mm / 1000, 3);
                OnPropertyChanged();
            }
        }

        public string CentimetersInput
        {
            get => _centimetersInput;
            set
            {
                _centimetersInput = value;
                if (decimal.TryParse(value, out decimal cm))
                    MetersFromCm = Math.Round(cm / 100, 3);
                OnPropertyChanged();
            }
        }

        public string DecimetersInput
        {
            get => _decimetersInput;
            set
            {
                _decimetersInput = value;
                if (decimal.TryParse(value, out decimal dm))
                    MetersFromDm = Math.Round(dm / 10, 3);
                OnPropertyChanged();
            }
        }

        public decimal MetersFromMm
        {
            get => _metersFromMm;
            private set
            {
                _metersFromMm = value;
                OnPropertyChanged();
            }
        }

        public decimal MetersFromCm
        {
            get => _metersFromCm;
            private set
            {
                _metersFromCm = value;
                OnPropertyChanged();
            }
        }

        public decimal MetersFromDm
        {
            get => _metersFromDm;
            private set
            {
                _metersFromDm = value;
                OnPropertyChanged();
            }
        }

        public string Width
        {
            get => _width;
            set
            {
                _width = value;
                CalculateCuboidVolume();
                OnPropertyChanged();
            }
        }

        public string Height
        {
            get => _height;
            set
            {
                _height = value;
                CalculateCuboidVolume();
                OnPropertyChanged();
            }
        }

        public string Length
        {
            get => _length;
            set
            {
                _length = value;
                CalculateCuboidVolume();
                OnPropertyChanged();
            }
        }

        public string Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                CalculateCuboidVolume();
                OnPropertyChanged();
            }
        }

        public decimal CuboidVolume
        {
            get => _cuboidVolume;
            private set
            {
                _cuboidVolume = value;
                OnPropertyChanged();
            }
        }

        public string Radius
        {
            get => _radius;
            set
            {
                _radius = value;
                CalculateCylinderVolume();
                OnPropertyChanged();
            }
        }

        public string CylinderLength
        {
            get => _cylinderLength;
            set
            {
                _cylinderLength = value;
                CalculateCylinderVolume();
                OnPropertyChanged();
            }
        }

        public string CylinderQuantity
        {
            get => _cylinderQuantity;
            set
            {
                _cylinderQuantity = value;
                CalculateCylinderVolume();
                OnPropertyChanged();
            }
        }

        public decimal CylinderVolume
        {
            get => _cylinderVolume;
            private set
            {
                _cylinderVolume = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Product> Products
        {
            get => _productsViewModel.Products;
            private set
            {
                OnPropertyChanged();
            }
        }

        public Product SelectedCuboidProduct
        {
            get => _selectedCuboidProduct;
            set
            {
                _selectedCuboidProduct = value;
                OnPropertyChanged();
            }
        }

        public Product SelectedCylinderProduct
        {
            get => _selectedCylinderProduct;
            set
            {
                _selectedCylinderProduct = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<CartItem> CartItems => _cartService.CartItems;

        private void CalculateCuboidVolume()
        {
            if (decimal.TryParse(Width, out decimal w) &&
                decimal.TryParse(Height, out decimal h) &&
                decimal.TryParse(Length, out decimal l) &&
                int.TryParse(Quantity, out int q))
            {
                CuboidVolume = Math.Round(w * h * l * q, 3);
            }
        }

        private void CalculateCylinderVolume()
        {
            if (decimal.TryParse(Radius, out decimal r) &&
                decimal.TryParse(CylinderLength, out decimal l) &&
                int.TryParse(CylinderQuantity, out int q))
            {
                CylinderVolume = Math.Round((decimal)(Math.PI * Math.Pow((double)r, 2)) * l * q, 3);
            }
        }

        private void ExecuteChooseCuboidProduct()
        {
            var dialog = new ChooseProductDialog(_productsViewModel);
            if (dialog.ShowDialog() == true)
            {
                SelectedCuboidProducts = new ObservableCollection<Product>(dialog.SelectedProducts);
            }
        }

        private void ExecuteChooseCylinderProduct()
        {
            var dialog = new ChooseProductDialog(_productsViewModel);
            if (dialog.ShowDialog() == true)
            {
                SelectedCylinderProducts = new ObservableCollection<Product>(dialog.SelectedProducts);
            }
        }

        private void ExecuteAddCuboidToCart()
        {
            if (!HasSelectedCuboidProducts) return;

            int quantity;
            if (!int.TryParse(Quantity, out quantity) || quantity <= 0)
            {
                MessageBox.Show("Proszę wprowadzić prawidłową ilość", "Błąd",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var items = SelectedCuboidProducts.Select(product => new CartItem
            {
                ProductName = product.Name,
                Volume = CuboidVolume,
                PricePerM3 = product.PricePerM3,
                Type = "Cuboid",
                Quantity = quantity
            });

            _cartService.AddItems(items);
            ClearCuboidInputs();
            SelectedCuboidProducts.Clear();
            IsCartOpen = true;
        }

        private void ExecuteAddCylinderToCart()
        {
            if (!HasSelectedCylinderProducts) return;

            int quantity;
            if (!int.TryParse(CylinderQuantity, out quantity) || quantity <= 0)
            {
                MessageBox.Show("Proszę wprowadzić prawidłową ilość", "Błąd",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var items = SelectedCylinderProducts.Select(product => new CartItem
            {
                ProductName = product.Name,
                Volume = CylinderVolume,
                PricePerM3 = product.PricePerM3,
                Type = "Cylinder",
                Quantity = quantity
            });

            _cartService.AddItems(items);
            ClearCylinderInputs();
            SelectedCylinderProducts.Clear();
            IsCartOpen = true;
        }

        private void ExecuteCalculate()
        {
            try
            {
                var priceService = App.Services.GetRequiredService<IPriceService>();

                priceService.ClearItems();

                foreach (var cartItem in CartItems)
                {
                    var product = Products.FirstOrDefault(p => p.Name == cartItem.ProductName);
                    if (product != null)
                    {
                        var priceItem = new PriceItem
                        {
                            Product = product,
                            Volume = cartItem.Volume,
                            Pieces = cartItem.Quantity
                        };

                        priceService.AddItem(product);

                        var addedItem = priceService.PriceItems.LastOrDefault();
                        if (addedItem != null)
                        {
                            addedItem.Volume = cartItem.Volume;
                            addedItem.Pieces = cartItem.Quantity;
                        }
                    }
                }

                _cartService.ClearCart();
                IsCartOpen = false;

                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    var pricePageButton = mainWindow.FindName("PricePageButton") as RadioButton;
                    if (pricePageButton != null)
                    {
                        pricePageButton.IsChecked = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while calculating: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteToggleCart()
        {
            IsCartOpen = !IsCartOpen;
        }

        private void ExecuteRemoveFromCart(CartItem item)
        {
            _cartService.RemoveItem(item);
        }

        private void ExecuteClearCart()
        {
            _cartService.ClearCart();
        }

        private void ClearCuboidInputs()
        {
            Width = string.Empty;
            Height = string.Empty;
            Length = string.Empty;
            Quantity = string.Empty;
            SelectedCuboidProduct = null;
            SelectedCuboidProducts.Clear();
        }

        private void ClearCylinderInputs()
        {
            Radius = string.Empty;
            CylinderLength = string.Empty;
            CylinderQuantity = string.Empty;
            SelectedCylinderProduct = null;
            SelectedCylinderProducts.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}