using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Xps.Packaging;
using WoodworkManagementApp.Services;
using WoodworkManagementApp.ViewModels;
using Aspose.Words;


namespace WoodworkManagementApp.Dialogs
{
    public partial class PrintPreviewDialog : Window, IDisposable
    {
        private readonly PrintPreviewDialogViewModel _viewModel;
        private XpsDocument _xpsDocument;
        private readonly ILogger<PrintPreviewDialog> _logger;
        private readonly IPrintService _printService;
        private readonly ILoggerFactory _loggerFactory;
        private bool _disposed;

        public PrintPreviewDialog(
            string documentPath,
            ILogger<PrintPreviewDialog> logger,
            IPrintService printService,
            ILoggerFactory loggerFactory)
        {
            InitializeComponent();
            _logger = logger;
            _printService = printService;
            _loggerFactory = loggerFactory;

            try
            {
                _viewModel = new PrintPreviewDialogViewModel(
                    this,
                    documentPath,
                    _printService,
                    _loggerFactory.CreateLogger<PrintPreviewDialogViewModel>());

                DataContext = _viewModel;
                LoadPreview(documentPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize print preview");
                Close();
            }
        }

        private async void LoadPreview(string documentPath)
        {
            try
            {
                IsEnabled = false;
                ShowLoadingIndicator("Loading preview...");

                await Task.Run(() =>
                {
                    var doc = new Aspose.Words.Document(documentPath);
                    try
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            var xpsSaveOptions = new Aspose.Words.Saving.XpsSaveOptions
                            {
                                SaveFormat = Aspose.Words.SaveFormat.Xps,
                                UseHighQualityRendering = false
                            };

                            doc.UpdatePageLayout();

                            foreach (Aspose.Words.Section section in doc.Sections)
                            {
                                var pageSetup = section.PageSetup;
                                pageSetup.PageWidth = pageSetup.PageWidth * 96 / 300;
                                pageSetup.PageHeight = pageSetup.PageHeight * 96 / 300;
                            }

                            doc.Save(memoryStream, xpsSaveOptions);
                            memoryStream.Position = 0;

                            var tempFile = Path.Combine(Path.GetTempPath(), $"preview_{Guid.NewGuid()}.xps");
                            File.WriteAllBytes(tempFile, memoryStream.ToArray());

                            Dispatcher.Invoke(() =>
                            {
                                try
                                {
                                    _xpsDocument = new XpsDocument(tempFile, FileAccess.Read);
                                    PreviewViewer.Document = _xpsDocument.GetFixedDocumentSequence();
                                }
                                finally
                                {
                                    try
                                    {
                                        if (File.Exists(tempFile))
                                        {
                                            File.Delete(tempFile);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogWarning(ex, "Failed to delete temporary XPS file: {Path}", tempFile);
                                    }
                                }
                            });
                        }
                    }
                    finally
                    {
                        try
                        {
                            (doc as IDisposable)?.Dispose();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error disposing Aspose.Words document");
                        }
                    }
                });
            }
            catch (OutOfMemoryException ex)
            {
                _logger.LogError(ex, "Out of memory while loading preview");
                MessageBox.Show(
                    "The document is too large to preview. Please try printing directly.",
                    "Preview Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load preview");
                MessageBox.Show(
                    $"Error loading preview: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Close();
            }
            finally
            {
                HideLoadingIndicator();
                IsEnabled = true;
            }
        }

        private void ShowLoadingIndicator(string message)
        {
            if (LoadingPanel != null && LoadingText != null)
            {
                LoadingText.Text = message;
                LoadingPanel.Visibility = Visibility.Visible;
            }
        }

        private void HideLoadingIndicator()
        {
            if (LoadingPanel != null)
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Dispose();
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
                    try
                    {
                        if (_xpsDocument != null)
                        {
                            _xpsDocument.Close();
                            _xpsDocument = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Error disposing XPS document");
                    }
                }
                _disposed = true;
            }
        }

        ~PrintPreviewDialog()
        {
            Dispose(false);
        }
    }
}