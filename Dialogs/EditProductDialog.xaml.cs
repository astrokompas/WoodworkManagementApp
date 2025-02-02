using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WoodworkManagementApp.Models;


namespace WoodworkManagementApp.Dialogs
{
    public partial class EditProductDialog : Window
    {
        private readonly Product _product;
        private readonly Product _editingProduct;

        public EditProductDialog(Product product)
        {
            InitializeComponent();
            _product = product;
            _editingProduct = new Product
            {
                Name = product.Name,
                PricePerM3 = product.PricePerM3,
                Discount = product.Discount,
                Category = product.Category,
                Order = product.Order
            };

            DataContext = _editingProduct;
            Loaded += Dialog_Loaded;
        }

        private void Dialog_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateClipping();
        }

        private void UpdateClipping()
        {
            if (MainBorder != null)
            {
                MainBorder.Clip = new RectangleGeometry
                {
                    RadiusX = 8,
                    RadiusY = 8,
                    Rect = new Rect(0, 0, MainBorder.ActualWidth, MainBorder.ActualHeight)
                };
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (HasValidationErrors())
            {
                MessageBox.Show("Please correct all errors before saving.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _product.Name = _editingProduct.Name;
            _product.PricePerM3 = _editingProduct.PricePerM3;
            _product.Discount = _editingProduct.Discount;
            _product.Category = _editingProduct.Category;

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private bool HasValidationErrors()
        {
            var textBoxes = FindVisualChildren<TextBox>(this);
            return textBoxes.Any(tb => Validation.GetHasError(tb));
        }

        private IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child != null && child is T)
                    yield return (T)child;

                foreach (T childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                Close();
            else
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
