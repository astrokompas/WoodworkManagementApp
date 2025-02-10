using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using WoodworkManagementApp.Helpers;
using WoodworkManagementApp.Models;
using WoodworkManagementApp.Services;

namespace WoodworkManagementApp.ViewModels
{
    public class SettingsDialogViewModel : INotifyPropertyChanged
    {
        private readonly ISettingsService _settingsService;
        private readonly Window _dialogWindow;
        private string _ordersPath;
        private string _templatesPath;

        public string OrdersPath
        {
            get => _ordersPath;
            set
            {
                _ordersPath = value;
                OnPropertyChanged();
            }
        }

        public string TemplatesPath
        {
            get => _templatesPath;
            set
            {
                _templatesPath = value;
                OnPropertyChanged();
            }
        }

        public ICommand BrowseOrdersPathCommand { get; }
        public ICommand BrowseTemplatesPathCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public SettingsDialogViewModel(ISettingsService settingsService, Window dialogWindow)
        {
            _settingsService = settingsService;
            _dialogWindow = dialogWindow;

            BrowseOrdersPathCommand = new RelayCommand(ExecuteBrowseOrdersPath);
            BrowseTemplatesPathCommand = new RelayCommand(ExecuteBrowseTemplatesPath);
            SaveCommand = new RelayCommand(ExecuteSave);
            CancelCommand = new RelayCommand(ExecuteCancel);

            LoadSettings();
        }

        private async void LoadSettings()
        {
            var settings = await _settingsService.LoadSettingsAsync();
            OrdersPath = settings.OrdersPath;
            TemplatesPath = settings.TemplatesPath;
        }

        private void ExecuteBrowseOrdersPath()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "Orders",
                InitialDirectory = OrdersPath,
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                OverwritePrompt = false
            };

            if (dialog.ShowDialog() == true)
            {
                OrdersPath = Path.GetDirectoryName(dialog.FileName);
            }
        }

        private void ExecuteBrowseTemplatesPath()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "Templates",
                InitialDirectory = TemplatesPath,
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                OverwritePrompt = false
            };

            if (dialog.ShowDialog() == true)
            {
                TemplatesPath = Path.GetDirectoryName(dialog.FileName);
            }
        }

        private async void ExecuteSave()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(OrdersPath) || string.IsNullOrWhiteSpace(TemplatesPath))
                {
                    MessageBox.Show("Ścieżki nie mogą być puste.", "Błąd",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var settings = new AppSettings
                {
                    OrdersPath = OrdersPath,
                    TemplatesPath = TemplatesPath
                };

                await _settingsService.SaveSettingsAsync(settings);
                _dialogWindow.DialogResult = true;
                _dialogWindow.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas zapisywania ustawień: {ex.Message}", "Błąd",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancel()
        {
            _dialogWindow.DialogResult = false;
            _dialogWindow.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
