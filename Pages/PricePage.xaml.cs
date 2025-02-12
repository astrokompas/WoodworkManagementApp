using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using WoodworkManagementApp.Services;
using WoodworkManagementApp.ViewModels;

namespace WoodworkManagementApp.Pages
{
    public partial class PricePage : Page
    {
        private readonly IProductsViewModel _productsViewModel;
        private readonly IProductService _productService;
        private readonly IPriceService _priceService;

        public PricePage(
            IProductsViewModel productsViewModel,
            IProductService productService,
            IPriceService priceService)
        {
            _productsViewModel = productsViewModel;
            _productService = productService;
            _priceService = priceService;

            InitializeComponent();
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await _productsViewModel.InitializeAsync();
            DataContext = new PricePageViewModel(
                _productService,
                _priceService,
                _productsViewModel
            );
        }
    }
}