using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using WoodworkManagementApp.ViewModels;

namespace WoodworkManagementApp.Pages
{
    public partial class OrdersPage : Page
    {
        private readonly IOrdersViewModel _ordersViewModel;

        public OrdersPage(IOrdersViewModel ordersViewModel)
        {
            _ordersViewModel = ordersViewModel;
            InitializeComponent();
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            DataContext = _ordersViewModel;
            await _ordersViewModel.InitializeAsync();
        }
    }
}