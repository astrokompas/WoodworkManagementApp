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
    public interface IProductsViewModel
    {
        ObservableCollection<Product> Products { get; }
        ICollectionView ProductsView { get; }
        string SearchText { get; set; }
    }
}
