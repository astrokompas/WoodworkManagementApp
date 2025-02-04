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
using System.Windows.Shapes;
using WoodworkManagementApp.ViewModels;
using WoodworkManagementApp.Models;

namespace WoodworkManagementApp.Dialogs
{
    public partial class AddOrderDialog : Window
    {
        private readonly AddOrderViewModel _viewModel;

        public Order Order => _viewModel.Order;

        public AddOrderDialog(IProductsViewModel productsViewModel)
        {
            InitializeComponent();
            _viewModel = new AddOrderViewModel(productsViewModel);
            DataContext = _viewModel;
        }
    }
}
