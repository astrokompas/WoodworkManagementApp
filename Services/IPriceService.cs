using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoodworkManagementApp.Models;

namespace WoodworkManagementApp.Services
{
    public interface IPriceService
    {
        ObservableCollection<PriceItem> PriceItems { get; }
        void AddItem(Product product);
        void RemoveItem(PriceItem item);
        void ClearItems();
        Task LoadPriceItemsAsync();
        Task SavePriceItemsAsync();
    }
}
