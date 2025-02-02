using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoodworkManagementApp.Services;
using System.Windows.Input;
using System.Windows.Threading;
using WoodworkManagementApp.Helpers;
using WoodworkManagementApp.Models;
using System.Runtime.CompilerServices;
using System.Windows;


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
        private string _mmError;
        private string _cmError;
        private string _dmError;

        private string _width;
        private string _height;
        private string _length;
        private string _quantity;
        private decimal _cuboidVolume;
        private string _cuboidError;
        private Product _selectedCuboidProduct;

        private string _radius;
        private string _cylinderLength;
        private string _cylinderQuantity;
        private decimal _cylinderVolume;
        private string _cylinderError;
        private Product _selectedCylinderProduct;

        private ObservableCollection<Product> _products;

        public ICommand AddCuboidToCartCommand { get; }
        public ICommand AddCylinderToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand ToggleCartCommand { get; }

        public CalcPageViewModel(IProductService productService, ICartService cartService, IProductsViewModel productsViewModel)
        {
            _productService = productService;
            _cartService = cartService;
            _productsViewModel = productsViewModel;
            _dispatcher = Application.Current.Dispatcher;

            AddCuboidToCartCommand = new RelayCommand(ExecuteAddCuboidToCart, CanAddCuboidToCart);
            AddCylinderToCartCommand = new RelayCommand(ExecuteAddCylinderToCart, CanAddCylinderToCart);
            RemoveFromCartCommand = new RelayCommand<CartItem>(ExecuteRemoveFromCart);
            ClearCartCommand = new RelayCommand(ExecuteClearCart);
            ToggleCartCommand = new RelayCommand(ExecuteToggleCart);

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
                ValidateAndConvertMillimeters();
                OnPropertyChanged();
            }
        }

        public string CentimetersInput
        {
            get => _centimetersInput;
            set
            {
                _centimetersInput = value;
                ValidateAndConvertCentimeters();
                OnPropertyChanged();
            }
        }

        public string DecimetersInput
        {
            get => _decimetersInput;
            set
            {
                _decimetersInput = value;
                ValidateAndConvertDecimeters();
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

        public string MmError
        {
            get => _mmError;
            private set
            {
                _mmError = value;
                OnPropertyChanged();
            }
        }

        public string CmError
        {
            get => _cmError;
            private set
            {
                _cmError = value;
                OnPropertyChanged();
            }
        }

        public string DmError
        {
            get => _dmError;
            private set
            {
                _dmError = value;
                OnPropertyChanged();
            }
        }
        public string Width
        {
            get => _width;
            set
            {
                _width = value;
                ValidateAndCalculateCuboidVolume();
                OnPropertyChanged();
            }
        }

        public string Height
        {
            get => _height;
            set
            {
                _height = value;
                ValidateAndCalculateCuboidVolume();
                OnPropertyChanged();
            }
        }

        public string Length
        {
            get => _length;
            set
            {
                _length = value;
                ValidateAndCalculateCuboidVolume();
                OnPropertyChanged();
            }
        }

        public string Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                ValidateAndCalculateCuboidVolume();
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

        public string CuboidError
        {
            get => _cuboidError;
            private set
            {
                _cuboidError = value;
                OnPropertyChanged();
            }
        }

        public string Radius
        {
            get => _radius;
            set
            {
                _radius = value;
                ValidateAndCalculateCylinderVolume();
                OnPropertyChanged();
            }
        }

        public string CylinderLength
        {
            get => _cylinderLength;
            set
            {
                _cylinderLength = value;
                ValidateAndCalculateCylinderVolume();
                OnPropertyChanged();
            }
        }

        public string CylinderQuantity
        {
            get => _cylinderQuantity;
            set
            {
                _cylinderQuantity = value;
                ValidateAndCalculateCylinderVolume();
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

        public string CylinderError
        {
            get => _cylinderError;
            private set
            {
                _cylinderError = value;
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

        private void ValidateAndConvertMillimeters()
        {
            if (string.IsNullOrWhiteSpace(MillimetersInput))
            {
                MetersFromMm = 0;
                MmError = null;
                return;
            }

            if (!decimal.TryParse(MillimetersInput, out decimal mm))
            {
                MmError = "Wprowadź poprawną wartość";
                return;
            }

            if (mm < 0)
            {
                MmError = "Wartość nie może być ujemna";
                return;
            }

            MmError = null;
            MetersFromMm = Math.Round(mm / 1000, 3);
        }

        private void ValidateAndConvertCentimeters()
        {
            if (string.IsNullOrWhiteSpace(CentimetersInput))
            {
                MetersFromCm = 0;
                CmError = null;
                return;
            }

            if (!decimal.TryParse(CentimetersInput, out decimal cm))
            {
                CmError = "Wprowadź poprawną wartość";
                return;
            }

            if (cm < 0)
            {
                CmError = "Wartość nie może być ujemna";
                return;
            }

            CmError = null;
            MetersFromCm = Math.Round(cm / 100, 3);
        }

        private void ValidateAndConvertDecimeters()
        {
            if (string.IsNullOrWhiteSpace(DecimetersInput))
            {
                MetersFromDm = 0;
                DmError = null;
                return;
            }

            if (!decimal.TryParse(DecimetersInput, out decimal dm))
            {
                DmError = "Wprowadź poprawną wartość";
                return;
            }

            if (dm < 0)
            {
                DmError = "Wartość nie może być ujemna";
                return;
            }

            DmError = null;
            MetersFromDm = Math.Round(dm / 10, 3);
        }

        private void ValidateAndCalculateCuboidVolume()
        {
            if (string.IsNullOrWhiteSpace(Width) || string.IsNullOrWhiteSpace(Height) ||
                string.IsNullOrWhiteSpace(Length) || string.IsNullOrWhiteSpace(Quantity))
            {
                CuboidVolume = 0;
                CuboidError = "Wypełnij wszystkie pola";
                return;
            }

            if (!decimal.TryParse(Width, out decimal w) || !decimal.TryParse(Height, out decimal h) ||
                !decimal.TryParse(Length, out decimal l) || !decimal.TryParse(Quantity, out decimal q))
            {
                CuboidError = "Wprowadź poprawne wartości";
                return;
            }

            if (w <= 0 || h <= 0 || l <= 0 || q <= 0)
            {
                CuboidError = "Wartości muszą być większe od zera";
                return;
            }

            CuboidError = null;
            CuboidVolume = Math.Round(w * h * l * q, 3);
        }

        private void ValidateAndCalculateCylinderVolume()
        {
            if (string.IsNullOrWhiteSpace(Radius) || string.IsNullOrWhiteSpace(CylinderLength) ||
                string.IsNullOrWhiteSpace(CylinderQuantity))
            {
                CylinderVolume = 0;
                CylinderError = "Wypełnij wszystkie pola";
                return;
            }

            if (!decimal.TryParse(Radius, out decimal r) || !decimal.TryParse(CylinderLength, out decimal l) ||
                !decimal.TryParse(CylinderQuantity, out decimal q))
            {
                CylinderError = "Wprowadź poprawne wartości";
                return;
            }

            if (r <= 0 || l <= 0 || q <= 0)
            {
                CylinderError = "Wartości muszą być większe od zera";
                return;
            }

            CylinderError = null;
            CylinderVolume = Math.Round((decimal)(Math.PI * Math.Pow((double)r, 2)) * l * q, 3);
        }

        private bool CanAddCuboidToCart() =>
                CuboidVolume > 0 && SelectedCuboidProduct != null && string.IsNullOrEmpty(CuboidError);

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

        private void ExecuteToggleCart()
        {
            IsCartOpen = !IsCartOpen;
        }

        private bool CanAddCylinderToCart() =>
                CylinderVolume > 0 && SelectedCylinderProduct != null && string.IsNullOrEmpty(CylinderError);

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
