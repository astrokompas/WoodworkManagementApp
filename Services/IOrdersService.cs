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
        Task<ObservableCollection<Order>> LoadOrdersAsync();
        Task SaveOrderAsync(Order order);
        Task DeleteOrderAsync(string orderNumber);
        Task<bool> LockOrderAsync(string orderNumber);
        Task UnlockOrderAsync(string orderNumber);
        string GetOrderFilePath(string orderNumber);
        Task GenerateThumbnailAsync(string orderNumber);
        event EventHandler<OrderChangedEventArgs> OrderChanged;
    }
}
