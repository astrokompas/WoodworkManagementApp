using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Windows.Threading;
using System.Windows;
using WoodworkManagementApp.Models;

namespace WoodworkManagementApp.Services
{
    public class ProductService : IProductService
    {
        private readonly IJsonStorageService _jsonStorage;
        private const string PRODUCTS_FILE = "products.json";
        private readonly Dispatcher _dispatcher;
        private ObservableCollection<Product> _products;

        public ObservableCollection<Product> Products
        {
            get => _products;
            private set => _products = value;
        }

        public ProductService(IJsonStorageService jsonStorage)
        {
            _jsonStorage = jsonStorage;
            _dispatcher = Application.Current.Dispatcher;
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            _products = new ObservableCollection<Product>();
        }

        public async Task<ObservableCollection<Product>> LoadProductsAsync()
        {
            try
            {
                var products = await _jsonStorage.LoadAsync<List<Product>>(PRODUCTS_FILE);
                var orderedProducts = products != null
                    ? products.OrderBy(p => p.Order).ToList()
                    : new List<Product>();

                await _dispatcher.InvokeAsync(() =>
                {
                    Products = new ObservableCollection<Product>(orderedProducts);
                });
                return Products;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load products", ex);
            }
        }

        public Product GetProductByName(string name)
        {
            return Products?.FirstOrDefault(p =>
                p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task SaveProductsAsync(IEnumerable<Product> products)
        {
            try
            {
                await _jsonStorage.SaveAsync(products.ToList(), PRODUCTS_FILE);
                await _dispatcher.InvokeAsync(() =>
                {
                    Products = new ObservableCollection<Product>(products);
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to save products", ex);
            }
        }

        public async Task ImportFromExcelAsync(string filePath)
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets[0];
            var products = new List<Product>();

            int rowCount = worksheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                products.Add(new Product
                {
                    Name = worksheet.Cells[row, 1].Value?.ToString() ?? "",
                    PricePerM3 = decimal.Parse(worksheet.Cells[row, 2].Value?.ToString() ?? "0"),
                    Discount = int.Parse(worksheet.Cells[row, 3].Value?.ToString() ?? "0"),
                    Category = worksheet.Cells[row, 4].Value?.ToString() ?? "",
                    Order = products.Count
                });
            }

            await SaveProductsAsync(products);
        }

        public async Task ExportToExcelAsync(string filePath, IEnumerable<Product> products)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Products");

            worksheet.Cells[1, 1].Value = "Name";
            worksheet.Cells[1, 2].Value = "Price (m³)";
            worksheet.Cells[1, 3].Value = "Discount (%)";
            worksheet.Cells[1, 4].Value = "Category";

            int row = 2;
            foreach (var product in products)
            {
                worksheet.Cells[row, 1].Value = product.Name;
                worksheet.Cells[row, 2].Value = product.PricePerM3;
                worksheet.Cells[row, 3].Value = product.Discount;
                worksheet.Cells[row, 4].Value = product.Category;
                row++;
            }

            using (var range = worksheet.Cells[1, 1, 1, 4])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 179, 163, 101));
            }

            await package.SaveAsAsync(new FileInfo(filePath));
        }
    }
}