using System.Diagnostics;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WoodworkManagementApp.Services;
using WoodworkManagementApp.ViewModels;
using System;
using WoodworkManagementApp.Dialogs;
using WoodworkManagementApp.Pages;


namespace WoodworkManagementApp
{
    public partial class App : Application
    {
        private IHost _host;
        private ILogger<App> _logger;
        private static IServiceProvider _services;
        public static IServiceProvider Services
        {
            get => _services;
            private set => _services = value;
        }

        private IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddDebug();
            logging.AddEventLog();
        })
        .ConfigureServices((context, services) =>
        {
            ConfigureServices(services);
        });

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger?.LogCritical(e.ExceptionObject as Exception, "Unhandled AppDomain exception");
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _logger?.LogCritical(e.Exception, "Unhandled Dispatcher exception");
            e.Handled = true;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            _logger?.LogCritical(e.Exception, "Unhandled Task exception");
            e.SetObserved();
        }


        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
                TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

                _host = CreateHostBuilder(e.Args).Build();
                await _host.StartAsync();

                _logger = _host.Services.GetRequiredService<ILogger<App>>();
                var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                HandleStartupException(ex);
            }
        }

        private void HandleStartupException(Exception ex)
        {
            _logger?.LogCritical(ex, "Application failed to start");

            MessageBox.Show(
                "The application failed to start properly. Please contact support.\n\n" +
                $"Error: {ex.Message}",
                "Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Current.Shutdown(1);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            try
            {
                if (_host != null)
                {
                    await _host.StopAsync(TimeSpan.FromSeconds(5));
                    _host.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during application shutdown");
            }
            finally
            {
                base.OnExit(e);
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<MainWindow>();

            services.AddSingleton<IJsonStorageService, JsonStorageService>();
            services.AddSingleton<ICartManager, CartManager>();
            services.AddSingleton<IProductService, ProductService>();
            services.AddSingleton<ICartService, CartService>();
            services.AddSingleton<IPriceService, PriceService>();
            services.AddSingleton<IDocumentService, DocumentService>();
            services.AddSingleton<IOrdersService, OrdersService>();
            services.AddSingleton<IOrderThumbnailGenerator, OrderThumbnailGenerator>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IPrintService, PrintService>();
            services.AddTransient<IDialogService, DialogService>();

            services.AddSingleton<IProductsViewModel, ProductsViewModel>();
            services.AddTransient<PricePageViewModel>();
            services.AddTransient<OrderDialogViewModelBase>();
            services.AddTransient<CalcPageViewModel>();
            services.AddTransient<IOrdersViewModel, OrdersViewModel>();
            services.AddTransient<PrintPreviewDialogViewModel>();
            services.AddTransient<ProductSelectionViewModel>();
            services.AddTransient<SettingsDialogViewModel>();

            services.AddTransient<PricePage>();
            services.AddTransient<ProductsPage>();
            services.AddTransient<CalcPage>();
            services.AddTransient<OrdersPage>();

            services.AddTransient<AddOrderDialog>();
            services.AddTransient<AddProductDialog>();
            services.AddTransient<ChooseProductDialog>();
            services.AddTransient<ConfirmationDialog>();
            services.AddTransient<EditOrderDialog>();
            services.AddTransient<EditProductDialog>();
            services.AddTransient<MessageDialog>();
            services.AddTransient<MultiSelectProductDialog>();
            services.AddTransient<PrintPreviewDialog>();
            services.AddTransient<SettingsDialog>();
        }

    }
}