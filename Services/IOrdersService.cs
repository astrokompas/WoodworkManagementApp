using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WoodworkManagementApp.ViewModels.OrdersViewModel;
using WoodworkManagementApp.Models;

namespace WoodworkManagementApp.Services
{
    public interface IOrdersService
    {
        Task<ObservableCollection<Order>> LoadOrdersAsync(CancellationToken cancellationToken = default);
        Task SaveOrderAsync(Order order, CancellationToken cancellationToken = default);
        Task DeleteOrderAsync(string orderNumber, CancellationToken cancellationToken = default);
        Task<bool> LockOrderAsync(string orderNumber);
        Task UnlockOrderAsync(string orderNumber);
        string GetOrderFilePath(string orderNumber);
        Task GenerateThumbnailAsync(string orderNumber);
        event EventHandler<OrderChangedEventArgs> OrderChanged;
    }
}
