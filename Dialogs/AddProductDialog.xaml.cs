using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WoodworkManagementApp.Models;


namespace WoodworkManagementApp.Dialogs
{
    public partial class AddProductDialog : Window
    {
        private readonly Product _product;

        public AddProductDialog()
        {
            InitializeComponent();
            _product = new Product
            {
                Name = "",
                PricePerM3 = 0,
                Discount = 0
            };
            DataContext = _product;
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

        public Product Product => _product;

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (HasValidationErrors())
            {
                MessageBox.Show("Proszę poprawić wszystkie błędy przed zapisaniem.", "Błąd walidacji",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_product.Name))
            {
                MessageBox.Show("Nazwa produktu nie może być pusta.", "Błąd walidacji",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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