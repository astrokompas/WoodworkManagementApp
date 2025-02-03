using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WoodworkManagementApp.Models;
using WoodworkManagementApp.ViewModels;

namespace WoodworkManagementApp.Dialogs
{
    public partial class ChooseProductDialog : Window
    {
        private readonly IProductsViewModel _productsViewModel;
        public Product SelectedProduct { get; private set; }

        public ChooseProductDialog(IProductsViewModel productsViewModel)
        {
            InitializeComponent();
            _productsViewModel = productsViewModel;
            DataContext = _productsViewModel;

            Loaded += ChooseProductDialog_Loaded;
            if (MainBorder != null)
            {
                MainBorder.SizeChanged += MainBorder_SizeChanged;
            }
        }

        private void ChooseProductDialog_Loaded(object sender, RoutedEventArgs e)
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
            DialogResult = false;
            Close();
        }

        private void ProductCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Product product)
            {
                SelectedProduct = product;
                DialogResult = true;
                Close();
            }
        }
    }
}