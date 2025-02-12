using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Extensions.DependencyInjection;
using WoodworkManagementApp.Services;
using WoodworkManagementApp.ViewModels;
using WoodworkManagementApp;


namespace WoodworkManagementApp.Pages
{
    public partial class CalcPage : Page
    {
        private readonly IProductsViewModel _productsViewModel;
        private readonly IProductService _productService;
        private readonly ICartService _cartService;

        public CalcPage(
            IProductsViewModel productsViewModel,
            IProductService productService,
            ICartService cartService)
        {
            _productsViewModel = productsViewModel;
            _productService = productService;
            _cartService = cartService;

            InitializeComponent();
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await _productsViewModel.InitializeAsync();
            DataContext = new CalcPageViewModel(
                _productService,
                _cartService,
                _productsViewModel
            );
        }
    }
}
