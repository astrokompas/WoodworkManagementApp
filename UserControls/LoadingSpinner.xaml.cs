using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WoodworkManagementApp.UserControls
{
    public partial class LoadingSpinner : UserControl, IDisposable
    {
        private EventHandler _renderingEventHandler;
        private bool _disposed;

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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_renderingEventHandler != null)
                    {
                        CompositionTarget.Rendering -= _renderingEventHandler;
                        _renderingEventHandler = null;
                    }
                }
                _disposed = true;
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

        ~LoadingSpinner()
        {
            Dispose(false);
        }
    }
}