using System.Windows;
using System.Windows.Input;
using WoodworkManagementApp.ViewModels;
using WoodworkManagementApp.Models;

namespace WoodworkManagementApp.Dialogs
{
    public partial class EditOrderDialog : Window
    {
        private readonly OrderDialogViewModelBase.EditOrderViewModel _viewModel;
        public Order Order => _viewModel.Order;

        public EditOrderDialog(Order order, IProductsViewModel productsViewModel)
        {
            InitializeComponent();
            _viewModel = new OrderDialogViewModelBase.EditOrderViewModel(order, productsViewModel);
            DataContext = _viewModel;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.ValidateOrder())
            {
                DialogResult = true;
                Close();
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                Close();
            else
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}