using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoodworkManagementApp.Models;


namespace WoodworkManagementApp.Services
{
    public interface IProductService
    {
        Task<ObservableCollection<Product>> LoadProductsAsync();
        Task SaveProductsAsync(IEnumerable<Product> products);
        Task ImportFromExcelAsync(string filePath);
        Task ExportToExcelAsync(string filePath, IEnumerable<Product> products);
    }
}
