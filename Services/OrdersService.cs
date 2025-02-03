using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WoodworkManagementApp.ViewModels.OrdersViewModel;
using WoodworkManagementApp.Models;

namespace WoodworkManagementApp.Services
{
    public class OrdersService : IOrdersService
    {
        private readonly string _ordersPath;
        private readonly string _locksPath;
        private readonly IJsonStorageService _jsonStorage;
        private readonly IDocumentService _documentService;
        private readonly FileSystemWatcher _watcher;
        private const int LOCK_TIMEOUT_MINUTES = 5;
        private readonly string _currentUser;

        public event EventHandler<OrderChangedEventArgs> OrderChanged;

        public OrdersService(
            IJsonStorageService jsonStorage,
            IDocumentService documentService)
        {
            _jsonStorage = jsonStorage;
            _documentService = documentService;

            _ordersPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "WoodworkManagementApp",
                "Orders"
            );

            _locksPath = Path.Combine(_ordersPath, "locks");
            _currentUser = Environment.UserName;

            Directory.CreateDirectory(_ordersPath);
            Directory.CreateDirectory(_locksPath);

            InitializeWatcher();
            StartLockCleanupTimer();
        }

        private void InitializeWatcher()
        {
            _watcher = new FileSystemWatcher(_ordersPath)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                Filter = "*.docx"
            };

            _watcher.Created += OnOrderFileChanged;
            _watcher.Changed += OnOrderFileChanged;
            _watcher.Deleted += OnOrderFileChanged;
            _watcher.EnableRaisingEvents = true;
        }

        private void StartLockCleanupTimer()
        {
            var timer = new System.Timers.Timer(60000); // Check every minute
            timer.Elapsed += async (s, e) => await CleanupExpiredLocks();
            timer.Start();
        }

        public async Task<bool> LockOrderAsync(string orderNumber)
        {
            var lockFilePath = GetLockFilePath(orderNumber);

            try
            {
                // Check if lock exists and is valid
                if (File.Exists(lockFilePath))
                {
                    var existingLock = await _jsonStorage.LoadAsync<OrderLock>(GetLockFileName(orderNumber));

                    if (existingLock.UserName == _currentUser)
                    {
                        // Extend current user's lock
                        await UpdateLock(existingLock);
                        return true;
                    }

                    if (existingLock.ExpirationTime > DateTime.Now)
                    {
                        return false; // Lock is still valid
                    }
                }

                // Create new lock
                var newLock = new OrderLock
                {
                    OrderNumber = orderNumber,
                    UserName = _currentUser,
                    LockTime = DateTime.Now,
                    ExpirationTime = DateTime.Now.AddMinutes(LOCK_TIMEOUT_MINUTES)
                };

                await _jsonStorage.SaveAsync(newLock, GetLockFileName(orderNumber));
                return true;
            }
            catch (IOException)
            {
                return false; // Another process might be creating/updating the lock
            }
        }

        public async Task UnlockOrderAsync(string orderNumber)
        {
            var lockFilePath = GetLockFilePath(orderNumber);

            try
            {
                if (File.Exists(lockFilePath))
                {
                    var existingLock = await _jsonStorage.LoadAsync<OrderLock>(GetLockFileName(orderNumber));
                    if (existingLock.UserName == _currentUser)
                    {
                        File.Delete(lockFilePath);
                    }
                }
            }
            catch (IOException)
            {
                // Log error but don't throw - the lock will expire anyway
            }
        }

        private async Task CleanupExpiredLocks()
        {
            try
            {
                var lockFiles = Directory.GetFiles(_locksPath, "*.lock.json");
                foreach (var lockFile in lockFiles)
                {
                    try
                    {
                        var orderLock = await _jsonStorage.LoadAsync<OrderLock>(
                            Path.GetFileName(lockFile));

                        if (orderLock.ExpirationTime <= DateTime.Now)
                        {
                            File.Delete(lockFile);
                        }
                    }
                    catch
                    {
                        // Skip problematic lock files
                    }
                }
            }
            catch
            {
                // Log error but continue running
            }
        }

        private async Task UpdateLock(OrderLock orderLock)
        {
            orderLock.ExpirationTime = DateTime.Now.AddMinutes(LOCK_TIMEOUT_MINUTES);
            await _jsonStorage.SaveAsync(orderLock, GetLockFileName(orderLock.OrderNumber));
        }

        public string GetOrderFilePath(string orderNumber)
        {
            return Path.Combine(_ordersPath, $"{orderNumber}.docx");
        }

        private string GetLockFilePath(string orderNumber)
        {
            return Path.Combine(_locksPath, GetLockFileName(orderNumber));
        }

        private string GetLockFileName(string orderNumber)
        {
            return $"{orderNumber}.lock.json";
        }

        public async Task<ObservableCollection<Order>> LoadOrdersAsync()
        {
            var orders = new ObservableCollection<Order>();
            var files = Directory.GetFiles(_ordersPath, "*.docx");

            foreach (var file in files)
            {
                try
                {
                    var orderNumber = Path.GetFileNameWithoutExtension(file);
                    var order = await _documentService.ReadOrderDocumentAsync(orderNumber);
                    orders.Add(order);
                }
                catch
                {
                    // Skip corrupted files
                }
            }

            return orders;
        }

        public async Task SaveOrderAsync(Order order)
        {
            var lockExists = await LockOrderAsync(order.OrderNumber);
            if (!lockExists)
            {
                throw new InvalidOperationException("Order is locked by another user");
            }

            try
            {
                await _documentService.UpdateOrderDocumentAsync(order);
                await GenerateThumbnailAsync(order.OrderNumber);
                RaiseOrderChanged(order, OrderChangeType.Modified);
            }
            finally
            {
                await UnlockOrderAsync(order.OrderNumber);
            }
        }

        public async Task DeleteOrderAsync(string orderNumber)
        {
            var lockExists = await LockOrderAsync(orderNumber);
            if (!lockExists)
            {
                throw new InvalidOperationException("Order is locked by another user");
            }

            try
            {
                File.Delete(GetOrderFilePath(orderNumber));
                var thumbnailPath = Path.Combine(_ordersPath, $"{orderNumber}.thumb.png");
                if (File.Exists(thumbnailPath))
                {
                    File.Delete(thumbnailPath);
                }
                RaiseOrderChanged(null, orderNumber, OrderChangeType.Deleted);
            }
            finally
            {
                await UnlockOrderAsync(orderNumber);
            }
        }

        public async Task GenerateThumbnailAsync(string orderNumber)
        {
            var previewBytes = await _documentService.GeneratePreviewAsync(orderNumber);
            var thumbnailPath = Path.Combine(_ordersPath, $"{orderNumber}.thumb.png");
            await File.WriteAllBytesAsync(thumbnailPath, previewBytes);
        }

        private void OnOrderFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                var orderNumber = Path.GetFileNameWithoutExtension(e.Name);

                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Created:
                    case WatcherChangeTypes.Changed:
                        Task.Run(async () =>
                        {
                            var order = await _documentService.ReadOrderDocumentAsync(orderNumber);
                            RaiseOrderChanged(order, e.ChangeType == WatcherChangeTypes.Created ?
                                OrderChangeType.Added : OrderChangeType.Modified);
                        });
                        break;

                    case WatcherChangeTypes.Deleted:
                        RaiseOrderChanged(null, orderNumber, OrderChangeType.Deleted);
                        break;
                }
            }
            catch
            {
                // Log error but continue watching
            }
        }

        private void RaiseOrderChanged(Order order, OrderChangeType changeType)
        {
            RaiseOrderChanged(order, order?.OrderNumber, changeType);
        }

        private void RaiseOrderChanged(Order order, string orderNumber, OrderChangeType changeType)
        {
            OrderChanged?.Invoke(this, new OrderChangedEventArgs(orderNumber, order, changeType));
        }
    }
}
