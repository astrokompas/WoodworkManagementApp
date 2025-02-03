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
        public CalcPage()
        {
            InitializeComponent();
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            var productsViewModel = App.Services.GetRequiredService<IProductsViewModel>();
            await productsViewModel.InitializeAsync();

            DataContext = new CalcPageViewModel(
                App.Services.GetRequiredService<IProductService>(),
                App.Services.GetRequiredService<ICartService>(),
                productsViewModel
            );
        }
    }
}
