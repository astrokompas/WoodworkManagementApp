using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace WoodworkManagementApp.UserControls
{
    public partial class LoadingSpinner : UserControl
    {
        public LoadingSpinner()
        {
            InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _renderingEventHandler = (s, e) => InvalidateVisual();
                CompositionTarget.Rendering += _renderingEventHandler;
            }
        }

        public void Dispose()
        {
            if (_renderingEventHandler != null)
            {
                CompositionTarget.Rendering -= _renderingEventHandler;
                _renderingEventHandler = null;
            }
        }

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register(
                nameof(IsLoading),
                typeof(bool),
                typeof(LoadingSpinner),
                new PropertyMetadata(false));

        public static readonly DependencyProperty LoadingTextProperty =
            DependencyProperty.Register(
                nameof(LoadingText),
                typeof(string),
                typeof(LoadingSpinner),
                new PropertyMetadata(string.Empty));

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public string LoadingText
        {
            get => (string)GetValue(LoadingTextProperty);
            set => SetValue(LoadingTextProperty, value);
        }
    }
}
