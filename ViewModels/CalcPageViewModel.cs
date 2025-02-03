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

        public ICommand AddCuboidToCartCommand { get; }
        public ICommand AddCylinderToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand ToggleCartCommand { get; }
        public ICommand ChooseCuboidProductCommand { get; }
        public ICommand ChooseCylinderProductCommand { get; }

        public CalcPageViewModel(IProductService productService, ICartService cartService, IProductsViewModel productsViewModel)
        {
            _productService = productService;
            _cartService = cartService;
            _productsViewModel = productsViewModel;
            _dispatcher = Application.Current.Dispatcher;

            AddCuboidToCartCommand = new RelayCommand(ExecuteAddCuboidToCart);
            AddCylinderToCartCommand = new RelayCommand(ExecuteAddCylinderToCart);
            RemoveFromCartCommand = new RelayCommand<CartItem>(ExecuteRemoveFromCart);
            ClearCartCommand = new RelayCommand(ExecuteClearCart);
            ToggleCartCommand = new RelayCommand(ExecuteToggleCart);
            ChooseCuboidProductCommand = new RelayCommand(ExecuteChooseCuboidProduct);
            ChooseCylinderProductCommand = new RelayCommand(ExecuteChooseCylinderProduct);

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
                decimal.TryParse(Quantity, out decimal q))
            {
                CuboidVolume = Math.Round(w * h * l * q, 3);
            }
        }

        private void CalculateCylinderVolume()
        {
            if (decimal.TryParse(Radius, out decimal r) &&
                decimal.TryParse(CylinderLength, out decimal l) &&
                decimal.TryParse(CylinderQuantity, out decimal q))
            {
                CylinderVolume = Math.Round((decimal)(Math.PI * Math.Pow((double)r, 2)) * l * q, 3);
            }
        }

        private void ExecuteChooseCuboidProduct()
        {
            var dialog = new ChooseProductDialog(_productsViewModel);
            if (dialog.ShowDialog() == true)
            {
                SelectedCuboidProduct = dialog.SelectedProduct;
            }
        }

        private void ExecuteChooseCylinderProduct()
        {
            var dialog = new ChooseProductDialog(_productsViewModel);
            if (dialog.ShowDialog() == true)
            {
                SelectedCylinderProduct = dialog.SelectedProduct;
            }
        }

        private void ExecuteAddCuboidToCart()
        {
            if (SelectedCuboidProduct == null) return;

            var item = new CartItem
            {
                ProductName = SelectedCuboidProduct.Name,
                Volume = CuboidVolume,
                PricePerM3 = SelectedCuboidProduct.PricePerM3,
                Type = "Cuboid"
            };

            _cartService.AddItem(item);
            ClearCuboidInputs();
            IsCartOpen = true;
        }

        private void ExecuteAddCylinderToCart()
        {
            if (SelectedCylinderProduct == null) return;

            var item = new CartItem
            {
                ProductName = SelectedCylinderProduct.Name,
                Volume = CylinderVolume,
                PricePerM3 = SelectedCylinderProduct.PricePerM3,
                Type = "Cylinder"
            };

            _cartService.AddItem(item);
            ClearCylinderInputs();
            IsCartOpen = true;
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
        }

        private void ClearCylinderInputs()
        {
            Radius = string.Empty;
            CylinderLength = string.Empty;
            CylinderQuantity = string.Empty;
            SelectedCylinderProduct = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}