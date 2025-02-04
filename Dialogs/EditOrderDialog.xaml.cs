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
    public partial class EditOrderDialog : Window
    {
        private readonly EditOrderViewModel _viewModel;

        public Order Order => _viewModel.Order;

        public EditOrderDialog(Order order, IProductsViewModel productsViewModel)
        {
            InitializeComponent();
            _viewModel = new EditOrderViewModel(order, productsViewModel);
            DataContext = _viewModel;
        }
    }
}
