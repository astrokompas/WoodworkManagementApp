﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WoodworkManagementApp.Services;
using Microsoft.Extensions.DependencyInjection;
using WoodworkManagementApp.ViewModels;
using WoodworkManagementApp;


namespace WoodworkManagementApp.Pages
{
    public partial class ProductsPage : Page
    {
        private readonly IProductsViewModel _productsViewModel;

        public ProductsPage(IProductsViewModel productsViewModel)
        {
            _productsViewModel = productsViewModel;
            InitializeComponent();
            DataContext = _productsViewModel;
        }
    }
}
