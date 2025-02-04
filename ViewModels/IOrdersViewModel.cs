using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoodworkManagementApp.Models;

namespace WoodworkManagementApp.ViewModels
{
    public interface IOrdersViewModel
    {
        ObservableCollection<Order> Orders { get; }
        ICollectionView FilteredOrders { get; }
        string SearchText { get; set; }
        Task InitializeAsync();
    }
}
