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
using Microsoft.Extensions.Logging;


namespace WoodworkManagementApp.ViewModels
{
    public class OrdersViewModel : IOrdersViewModel, INotifyPropertyChanged
    {
        private readonly IPrintService _printService;
        private readonly IOrdersService _ordersService;
        private readonly IProductsViewModel _productsViewModel;
        private readonly Dispatcher _dispatcher;
        private readonly IDialogService _dialogService;
        private readonly ILogger<OrdersViewModel> _logger;
        private ObservableCollection<Order> _orders;
        private string _searchText;
        private ICollectionView _filteredOrders;
        private const int PAGE_SIZE = 9;
        private int _currentPage = 1;
        private int _totalPages;
        private bool _isLoading;
        private string _loadingText;

        public ICommand AddOrderCommand { get; }
        public ICommand EditOrderCommand { get; }
        public ICommand DeleteOrderCommand { get; }
        public ICommand DownloadOrderCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand OpenSettingsCommand { get; }

        public OrdersViewModel(
            IOrdersService ordersService,
            IProductsViewModel productsViewModel,
            IDialogService dialogService,
            IPrintService printService,
            ILogger<OrdersViewModel> logger)
        {
            _ordersService = ordersService;
            _productsViewModel = productsViewModel;
            _dialogService = dialogService;
            _printService = printService;
            _logger = logger;
            _dispatcher = Application.Current.Dispatcher;
            _orders = new ObservableCollection<Order>();

            AddOrderCommand = new RelayCommand(ExecuteAddOrder);
            EditOrderCommand = new RelayCommand<Order>(ExecuteEditOrder);
            DeleteOrderCommand = new RelayCommand<Order>(ExecuteDeleteOrder);
            DownloadOrderCommand = new RelayCommand<Order>(ExecuteDownloadOrder);
            ClearSearchCommand = new RelayCommand(ExecuteClearSearch);
            NextPageCommand = new RelayCommand(ExecuteNextPage, CanExecuteNextPage);
            PreviousPageCommand = new RelayCommand(ExecutePreviousPage, CanExecutePreviousPage);
            OpenSettingsCommand = new RelayCommand(ExecuteOpenSettings);

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
                _filteredOrders?.Refresh();
                OnPropertyChanged();
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (value < 1 || (TotalPages > 0 && value > TotalPages)) return;
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged();
                    UpdateDisplayedOrders();
                    UpdatePaginationCommands();
                }
            }
        }

        public int TotalPages
        {
            get => _totalPages;
            private set
            {
                _totalPages = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string LoadingText
        {
            get => _loadingText;
            set
            {
                _loadingText = value;
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

            if (item is not Order order)
                return false;

            var searchTerms = SearchText.ToLower().Split(' ',
                StringSplitOptions.RemoveEmptyEntries);

            return searchTerms.All(term =>
                (order.OrderNumber?.ToLower().Contains(term) ?? false) ||
                (order.ReceiverName?.ToLower().Contains(term) ?? false) ||
                (order.CreatorName?.ToLower().Contains(term) ?? false) ||
                (order.Comments?.ToLower().Contains(term) ?? false));
        }

        private async Task<bool> ValidateOrder(Order order)
        {
            try
            {
                var orderPath = _ordersService.GetOrderFilePath(order.OrderNumber);
                FileValidator.ValidateWordDocument(orderPath);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Błąd walidacji dokumentu: {ex.Message}",
                    "Błąd",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private async void ExecuteAddOrder()
        {
            var dialog = new AddOrderDialog(_productsViewModel, _dialogService);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var orderToSave = dialog.Order;
                    await _ordersService.SaveOrderAsync(orderToSave);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving order");
                    _dialogService.ShowError("Save Error", $"Error saving order: {ex.Message}");
                }
            }
        }

        private async void ExecuteEditOrder(Order order)
        {
            if (order == null || !await ValidateOrder(order))
                return;

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

        private async void ExecuteDownloadOrder(Order order)
        {
            if (order == null || !await ValidateOrder(order))
                return;

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
                        _ordersService.GetOrderFilePath(order.OrderNumber),
                        dialog.FileName,
                        true
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error downloading order {OrderNumber}", order.OrderNumber);
                    _dialogService.ShowError("Download Error", $"Error downloading order: {ex.Message}");
                }
            }
        }

        private void UpdateDisplayedOrders()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                var totalItems = Orders.Count;
                TotalPages = (int)Math.Ceiling(totalItems / (double)PAGE_SIZE);

                var skip = (CurrentPage - 1) * PAGE_SIZE;
                var pagedOrders = Orders.Skip(skip).Take(PAGE_SIZE).ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    FilteredOrders = CollectionViewSource.GetDefaultView(pagedOrders);
                });
            }
            else
            {
                var filteredList = Orders.Where(o => FilterOrders(o)).ToList();
                TotalPages = (int)Math.Ceiling(filteredList.Count / (double)PAGE_SIZE);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    FilteredOrders = CollectionViewSource.GetDefaultView(filteredList);
                });
            }
        }

        private void UpdatePaginationCommands()
        {
            (NextPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (PreviousPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private void ExecuteOpenSettings()
        {
            var dialog = new SettingsDialog();
            if (dialog.ShowDialog() == true)
            {
                InitializeAsync().ConfigureAwait(false);
            }
        }

        private void ExecuteNextPage() => CurrentPage++;
        private bool CanExecuteNextPage() => CurrentPage < TotalPages;
        private void ExecutePreviousPage() => CurrentPage--;
        private bool CanExecutePreviousPage() => CurrentPage > 1;

        private void ExecuteClearSearch()
        {
            SearchText = string.Empty;
        }

        private void OnOrderChanged(object sender, OrderChangedEventArgs e)
        {
            _dispatcher.InvokeAsync(() =>
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

        public void Dispose()
        {
            _ordersService.OrderChanged -= OnOrderChanged;
        }
    }
}