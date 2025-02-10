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
using Microsoft.Extensions.DependencyInjection;
using WoodworkManagementApp.Services;
using WoodworkManagementApp.ViewModels;

namespace WoodworkManagementApp.Dialogs
{
    public partial class SettingsDialog : Window
    {
        private readonly SettingsDialogViewModel _viewModel;

        public SettingsDialog()
        {
            InitializeComponent();
            _viewModel = new SettingsDialogViewModel(
                App.Services.GetRequiredService<ISettingsService>(),
                this
            );
            DataContext = _viewModel;
        }
    }
}
