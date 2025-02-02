using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoodworkManagementApp.Models;


namespace WoodworkManagementApp.Services
{
    public interface ICartManager
    {
        Task SaveCartStateAsync(ObservableCollection<CartItem> items);
        Task<ObservableCollection<CartItem>> LoadCartStateAsync();
        Task ClearCartStateAsync();
    }
}
