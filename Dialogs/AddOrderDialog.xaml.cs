﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WoodworkManagementApp.ViewModels;
using WoodworkManagementApp.Models;
using WoodworkManagementApp.Services;

namespace WoodworkManagementApp.Dialogs
{
    public partial class AddOrderDialog : Window
    {
        private readonly OrderDialogViewModelBase.AddOrderViewModel _viewModel;
        private readonly IDialogService _dialogService;

        public Order Order => _viewModel.Order;

        public AddOrderDialog(IProductsViewModel productsViewModel, IDialogService dialogService)
        {
            InitializeComponent();
            _dialogService = dialogService;

            try
            {
                _viewModel = new OrderDialogViewModelBase.AddOrderViewModel(productsViewModel);
                DataContext = _viewModel;
            }
            catch (Exception ex)
            {
                _dialogService.ShowError("Error Initializing",
                    "Failed to initialize the order dialog. Please try again.\n\nError: " + ex.Message);
                DialogResult = false;
                Close();
            }
        }

        private async void AddOrderDialog_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                IsEnabled = false;
                await _viewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError("Loading Error",
                    "Failed to load required data. Please try again.\n\nError: " + ex.Message);
                DialogResult = false;
                Close();
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.ValidateOrder())
            {
                DialogResult = true;
                Close();
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