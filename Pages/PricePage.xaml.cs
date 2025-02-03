using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using WoodworkManagementApp.Services;
using WoodworkManagementApp.ViewModels;

namespace WoodworkManagementApp.Pages
{
    public partial class PricePage : Page
    {
        public PricePage()
        {
            InitializeComponent();
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            var productsViewModel = App.Services.GetRequiredService<IProductsViewModel>();
            await productsViewModel.InitializeAsync();
            DataContext = new PricePageViewModel(
                App.Services.GetRequiredService<IProductService>(),
                App.Services.GetRequiredService<IPriceService>(),
                productsViewModel
            );
        }
    }
}