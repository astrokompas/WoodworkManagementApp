using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoodworkManagementApp.Models;


namespace WoodworkManagementApp.Services
{
    public interface ICartService
    {
        ObservableCollection<CartItem> CartItems { get; }
        void AddItem(CartItem item);
        void RemoveItem(CartItem item);
        void ClearCart();
        Task LoadCartAsync();
        Task SaveCartAsync();
        void AddItems(IEnumerable<CartItem> items);
    }
}
