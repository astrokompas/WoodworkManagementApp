using System.Diagnostics;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WoodworkManagementApp.Services;
using WoodworkManagementApp.ViewModels;
using System;


namespace WoodworkManagementApp
{
    public partial class App : Application
    {
        private static IServiceProvider _services;
        public static IServiceProvider Services
        {
            get => _services;
            private set => _services = value;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string processName = Process.GetCurrentProcess().ProcessName;
            if (Process.GetProcessesByName(processName).Count() > 1)
            {
                Shutdown();
                return;
            }

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            Services = serviceCollection.BuildServiceProvider();

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IJsonStorageService, JsonStorageService>();
            services.AddSingleton<ICartManager, CartManager>();
            services.AddSingleton<IProductService, ProductService>();
            services.AddSingleton<ICartService, CartService>();

            services.AddSingleton<IProductsViewModel, ProductsViewModel>();
        }

    }
}