using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using WoodworkManagementApp.Helpers;
using WoodworkManagementApp.Models;
using WoodworkManagementApp.Services;
using System.Windows;
using WoodworkManagementApp.Dialogs;
using System.Windows.Threading;
using System.IO;


namespace WoodworkManagementApp.ViewModels
{
    public class OrdersViewModel : IOrdersViewModel, INotifyPropertyChanged
    {
        private readonly IOrdersService _ordersService;
        private readonly IProductsViewModel _productsViewModel;
        private readonly Dispatcher _dispatcher;
        private ObservableCollection<Order> _orders;
        private string _searchText;
        private ICollectionView _filteredOrders;

        public ICommand AddOrderCommand { get; }
        public ICommand EditOrderCommand { get; }
        public ICommand DeleteOrderCommand { get; }
        public ICommand DownloadOrderCommand { get; }
        public ICommand ClearSearchCommand { get; }

        public OrdersViewModel(IOrdersService ordersService, IProductsViewModel productsViewModel)
        {
            _ordersService = ordersService;
            _productsViewModel = productsViewModel;
            _dispatcher = Application.Current.Dispatcher;
            _orders = new ObservableCollection<Order>();

            // Initialize commands
            AddOrderCommand = new RelayCommand(ExecuteAddOrder);
            EditOrderCommand = new RelayCommand<Order>(ExecuteEditOrder);
            DeleteOrderCommand = new RelayCommand<Order>(ExecuteDeleteOrder);
            DownloadOrderCommand = new RelayCommand<Order>(ExecuteDownloadOrder);
            ClearSearchCommand = new RelayCommand(ExecuteClearSearch);

            // Subscribe to order changes
            _ordersService.OrderChanged += OnOrderChanged;
        }

        public ObservableCollection<Order> Orders
        {
            get => _orders;
            private set
            {
                _orders = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView FilteredOrders
        {
            get => _filteredOrders;
            private set
            {
                _filteredOrders = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                FilteredOrders?.Refresh();
                OnPropertyChanged();
            }
        }

        public async Task InitializeAsync()
        {
            try
            {
                var loadedOrders = await _ordersService.LoadOrdersAsync();
                await _dispatcher.InvokeAsync(() =>
                {
                    Orders = new ObservableCollection<Order>(loadedOrders);
                    FilteredOrders = CollectionViewSource.GetDefaultView(Orders);
                    FilteredOrders.Filter = FilterOrders;
                });
            }
            catch (Exception ex)
            {
                await _dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Error loading orders: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private bool FilterOrders(object item)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return true;

            if (item is Order order)
            {
                return order.OrderNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                       order.ReceiverName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                       order.CreatorName.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private async void ExecuteAddOrder()
        {
            var dialog = new AddOrderDialog(_productsViewModel);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var orderToSave = dialog.Order;
                    await _ordersService.SaveOrderAsync(orderToSave);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving order: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ExecuteEditOrder(Order order)
        {
            if (order == null) return;

            try
            {
                if (!await _ordersService.LockOrderAsync(order.OrderNumber))
                {
                    MessageBox.Show($"Order is currently being edited by {order.LockedBy}", "Warning",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dialog = new EditOrderDialog(order, _productsViewModel);
                if (dialog.ShowDialog() == true)
                {
                    var orderToSave = dialog.Order;
                    await _ordersService.SaveOrderAsync(orderToSave);
                }

                await _ordersService.UnlockOrderAsync(order.OrderNumber);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing order: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                await _ordersService.UnlockOrderAsync(order.OrderNumber);
            }
        }

        private async void ExecuteDeleteOrder(Order order)
        {
            if (order == null) return;

            if (MessageDialog.Show(
                $"Czy na pewno chcesz usunąć zamówienie {order.OrderNumber}?",
                "Potwierdzenie"))
            {
                try
                {
                    await _ordersService.DeleteOrderAsync(order.OrderNumber);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting order: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteDownloadOrder(Order order)
        {
            if (order == null) return;

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"Order_{order.OrderNumber}.docx",
                DefaultExt = ".docx",
                Filter = "Word Documents (.docx)|*.docx"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    File.Copy(
                        Path.Combine(_ordersService.GetOrderFilePath(order.OrderNumber)),
                        dialog.FileName,
                        true
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error downloading order: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteClearSearch()
        {
            SearchText = string.Empty;
        }

        private void OnOrderChanged(object sender, OrderChangedEventArgs e)
        {
            _dispatcher.Invoke(() =>
            {
                var existingOrder = Orders.FirstOrDefault(o => o.OrderNumber == e.OrderNumber);

                switch (e.ChangeType)
                {
                    case OrderChangeType.Added:
                        if (existingOrder == null)
                            Orders.Add(e.Order);
                        break;

                    case OrderChangeType.Modified:
                        if (existingOrder != null)
                        {
                            var index = Orders.IndexOf(existingOrder);
                            Orders[index] = e.Order;
                        }
                        break;

                    case OrderChangeType.Deleted:
                        if (existingOrder != null)
                            Orders.Remove(existingOrder);
                        break;
                }
            });
        }

        public enum OrderChangeType
        {
            Added,
            Modified,
            Deleted
        }

        public class OrderChangedEventArgs : EventArgs
        {
            public string OrderNumber { get; }
            public Order Order { get; }
            public OrderChangeType ChangeType { get; }

            public OrderChangedEventArgs(string orderNumber, Order order, OrderChangeType changeType)
            {
                OrderNumber = orderNumber;
                Order = order;
                ChangeType = changeType;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}