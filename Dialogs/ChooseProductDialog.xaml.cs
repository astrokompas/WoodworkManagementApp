using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WoodworkManagementApp.Models;
using WoodworkManagementApp.ViewModels;


namespace WoodworkManagementApp.Dialogs
{
    public partial class ChooseProductDialog : Window, INotifyPropertyChanged
    {
        private readonly IProductsViewModel _productsViewModel;
        private readonly HashSet<Product> _selectedProducts;
        private ObservableCollection<ProductSelectionViewModel> _productSelectionViewModels;

        public IEnumerable<Product> SelectedProducts => _selectedProducts;

        public ObservableCollection<ProductSelectionViewModel> ProductSelectionViewModels
        {
            get => _productSelectionViewModels;
            private set
            {
                _productSelectionViewModels = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _productsViewModel.SearchText;
            set => _productsViewModel.SearchText = value;
        }

        public ChooseProductDialog(IProductsViewModel productsViewModel)
        {
            InitializeComponent();
            _productsViewModel = productsViewModel;
            _selectedProducts = new HashSet<Product>();

            ProductSelectionViewModels = new ObservableCollection<ProductSelectionViewModel>(
                _productsViewModel.Products.Select(p => new ProductSelectionViewModel(p))
            );

            DataContext = this;
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

        private void ProductCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element &&
                element.DataContext is ProductSelectionViewModel viewModel)
            {
                viewModel.IsSelected = !viewModel.IsSelected;
                if (viewModel.IsSelected)
                {
                    _selectedProducts.Add(viewModel.Product);
                }
                else
                {
                    _selectedProducts.Remove(viewModel.Product);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var viewModel in ProductSelectionViewModels)
            {
                viewModel.IsSelected = false;
            }
            _selectedProducts.Clear();
            DialogResult = false;
            Close();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProducts.Count > 0)
            {
                DialogResult = true;
                foreach (var viewModel in ProductSelectionViewModels)
                {
                    viewModel.IsSelected = false;
                }
                Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}