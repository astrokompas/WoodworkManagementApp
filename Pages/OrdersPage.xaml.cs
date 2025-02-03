using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WoodworkManagementApp.Helpers;
using WoodworkManagementApp.Models;
using System.Windows;
using WoodworkManagementApp.Dialogs;

namespace WoodworkManagementApp.ViewModels
{
    public class OrderDialogViewModelBase : INotifyPropertyChanged
    {
        protected readonly IProductsViewModel _productsViewModel;
        protected Order _order;

        public ICommand AddProductsCommand { get; }
        public ICommand RemoveProductCommand { get; }

        public Order Order
        {
            get => _order;
            set
            {
                _order = value;
                OnPropertyChanged();
            }
        }

        public OrderDialogViewModelBase(IProductsViewModel productsViewModel)
        {
            _productsViewModel = productsViewModel;
            AddProductsCommand = new RelayCommand(ExecuteAddProducts);
            RemoveProductCommand = new RelayCommand<OrderProduct>(ExecuteRemoveProduct);
        }

        protected void ExecuteAddProducts()
        {
            var dialog = new ChooseProductDialog(_productsViewModel);
            if (dialog.ShowDialog() == true)
            {
                foreach (var product in dialog.SelectedProducts)
                {
                    Order.Products.Add(new OrderProduct { Product = product });
                }
            }
        }

        protected void ExecuteRemoveProduct(OrderProduct product)
        {
            if (product != null)
            {
                Order.Products.Remove(product);
            }
        }

        public bool ValidateOrder()
        {
            if (string.IsNullOrWhiteSpace(Order.OrderNumber))
            {
                MessageBox.Show("Numer zamówienia jest wymagany.", "Walidacja",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(Order.ReceiverName))
            {
                MessageBox.Show("Nazwa odbiorcy jest wymagana.", "Walidacja",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (Order.Products.Count == 0)
            {
                MessageBox.Show("Zamówienie musi zawierać przynajmniej jeden produkt.", "Walidacja",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AddOrderViewModel : OrderDialogViewModelBase
    {
        public AddOrderViewModel(IProductsViewModel productsViewModel)
            : base(productsViewModel)
        {
            Order = new Order
            {
                CreationDate = DateTime.Now,
                CreatorName = Environment.UserName,
                OrderNumber = GenerateOrderNumber()
            };
        }

        private string GenerateOrderNumber()
        {
            return $"ORD_{DateTime.Now:yyyyMMdd}_{Guid.NewGuid().ToString().Substring(0, 8)}";
        }
    }

    public class EditOrderViewModel : OrderDialogViewModelBase
    {
        public EditOrderViewModel(Order order, IProductsViewModel productsViewModel)
            : base(productsViewModel)
        {
            Order = order.Clone();
        }
    }
}