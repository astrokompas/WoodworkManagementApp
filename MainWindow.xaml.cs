using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WoodworkManagementApp.Pages;
using System.Diagnostics;
using System.Windows.Navigation;
using System.ComponentModel;
using WoodworkManagementApp.Dialogs;
using Microsoft.Extensions.DependencyInjection;


namespace WoodworkManagementApp
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private bool _isClosing = false;

        public MainWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            StateChanged += MainWindow_StateChanged;

            if (MainWindowBorder != null)
            {
                MainWindowBorder.SizeChanged += MainWindowBorder_SizeChanged;
            }

            UpdateClipping();

            if (MainFrame != null)
            {
                MainFrame.Navigate(_serviceProvider.GetRequiredService<PricePage>());
            }
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (MainWindowBorder == null) return;

            if (WindowState == WindowState.Normal)
            {
                Width = 1300;
                Height = 800;
                MainWindowBorder.Margin = new Thickness(0);
                var workArea = SystemParameters.WorkArea;
                Left = (workArea.Width - Width) / 2;
                Top = (workArea.Height - Height) / 2;
            }
            else if (WindowState == WindowState.Maximized)
            {
                var workArea = SystemParameters.WorkArea;
                Width = workArea.Width;
                Height = workArea.Height;
                Left = workArea.Left;
                Top = workArea.Top;

                double taskbarHeight = SystemParameters.PrimaryScreenHeight - workArea.Height;
                MainWindowBorder.Margin = new Thickness(
                    8,
                    8,
                    8,
                    8 + taskbarHeight
                );
            }
            UpdateClipping();
        }

        private void MainWindowBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateClipping();
        }

        private void UpdateClipping()
        {
            if (MainWindowBorder == null) return;

            MainWindowBorder.Clip = new RectangleGeometry
            {
                RadiusX = 8,
                RadiusY = 8,
                Rect = new Rect(0, 0, MainWindowBorder.ActualWidth, MainWindowBorder.ActualHeight)
            };
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MaximizeButton_Click(sender, e);
            }
            else
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ?
                WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (ConfirmClose())
            {
                _isClosing = true;
                Close();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_isClosing)
            {
                e.Cancel = !ConfirmClose();
                if (!e.Cancel)
                {
                    _isClosing = true;
                }
            }
            base.OnClosing(e);
        }

        private bool ConfirmClose()
        {
            return MessageDialog.Show(
                "Czy na pewno chcesz zamknąć aplikację?",
                "Zamykanie aplikacji");
        }

        private void NavButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.Tag != null && MainFrame != null)
            {
                Page page = radioButton.Tag.ToString() switch
                {
                    "Oferta" => _serviceProvider.GetRequiredService<PricePage>(),
                    "Produkty" => _serviceProvider.GetRequiredService<ProductsPage>(),
                    "Przelicznik" => _serviceProvider.GetRequiredService<CalcPage>(),
                    "Zamówienia" => _serviceProvider.GetRequiredService<OrdersPage>(),
                    _ => null
                };

                if (page != null)
                {
                    MainFrame.Navigate(page);
                }
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}