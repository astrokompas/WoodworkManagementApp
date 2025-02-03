using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WoodworkManagementApp.Models;
using WoodworkManagementApp.ViewModels;

namespace WoodworkManagementApp.Dialogs
{
    public partial class MultiSelectProductDialog : Window
    {
        private readonly IProductsViewModel _productsViewModel;
        private readonly HashSet<Product> _selectedProducts;

        public IEnumerable<Product> SelectedProducts => _selectedProducts;

        public MultiSelectProductDialog(IProductsViewModel productsViewModel)
        {
            InitializeComponent();
            _productsViewModel = productsViewModel;
            _selectedProducts = new HashSet<Product>();
            DataContext = _productsViewModel;

            Loaded += MultiSelectProductDialog_Loaded;
            if (MainBorder != null)
            {
                MainBorder.SizeChanged += MainBorder_SizeChanged;
            }
        }

        private void MultiSelectProductDialog_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateClipping();
        }

        private void MainBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateClipping();
        }

        private void UpdateClipping()
        {
            if (MainBorder == null) return;

            MainBorder.Clip = new RectangleGeometry
            {
                RadiusX = 8,
                RadiusY = 8,
                Rect = new Rect(0, 0, MainBorder.ActualWidth, MainBorder.ActualHeight)
            };
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset selection state
            foreach (var product in _productsViewModel.Products)
            {
                product.IsSelected = false;
            }
            _selectedProducts.Clear();

            DialogResult = false;
            Close();
        }

        private void ProductCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Product product)
            {
                // Toggle selection
                product.IsSelected = !product.IsSelected;

                if (product.IsSelected)
                {
                    _selectedProducts.Add(product);
                }
                else
                {
                    _selectedProducts.Remove(product);
                }
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProducts.Count > 0)
            {
                DialogResult = true;

                // Reset selection state after saving the selection
                foreach (var product in _productsViewModel.Products)
                {
                    product.IsSelected = false;
                }

                Close();
            }
        }
    }
}