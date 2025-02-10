using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using WoodworkManagementApp.Helpers;
using WoodworkManagementApp.Services;

public class PrintPreviewDialogViewModel : INotifyPropertyChanged
{
    private readonly Window _dialogWindow;
    private readonly string _documentPath;
    private readonly ObservableCollection<PrinterInfo> _printers;
    private PrinterInfo _selectedPrinter;
    private int _copies = 1;
    private bool _isBlackAndWhite = true;
    private bool _isPrinterAvailable;
    private bool _isLoading;
    private string _loadingMessage;
    private string _errorMessage;
    private readonly IPrintService _printService;
    private readonly ILogger<PrintPreviewDialogViewModel> _logger;

    public ObservableCollection<PrinterInfo> Printers => _printers;

    public PrinterInfo SelectedPrinter
    {
        get => _selectedPrinter;
        set
        {
            _selectedPrinter = value;
            OnPropertyChanged();
        }
    }

    public int Copies
    {
        get => _copies;
        set
        {
            if (value >= 1 && value <= 99)
            {
                _copies = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsBlackAndWhite
    {
        get => _isBlackAndWhite;
        set
        {
            _isBlackAndWhite = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public string LoadingMessage
    {
        get => _loadingMessage;
        set
        {
            _loadingMessage = value;
            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public ICommand PrintCommand { get; }
    public ICommand IncreaseCopiesCommand { get; }
    public ICommand DecreaseCopiesCommand { get; }
    public ICommand RefreshPrintersCommand { get; }

    public PrintPreviewDialogViewModel(
        Window dialogWindow,
        string documentPath,
        IPrintService printService,
        ILogger<PrintPreviewDialogViewModel> logger)
    {
        _dialogWindow = dialogWindow;
        _documentPath = documentPath;
        _printService = printService;
        _logger = logger;
        _printers = new ObservableCollection<PrinterInfo>();

        PrintCommand = new RelayCommand(async () => await ExecutePrintAsync(), CanExecutePrint);
        IncreaseCopiesCommand = new RelayCommand(() => Copies++, () => Copies < 99);
        DecreaseCopiesCommand = new RelayCommand(() => Copies--, () => Copies > 1);
        RefreshPrintersCommand = new RelayCommand(async () => await RefreshPrintersAsync());

        _ = InitializePrintersAsync();
    }

    private bool CanExecutePrint() => _selectedPrinter != null && _selectedPrinter.IsOnline;

    private async Task InitializePrintersAsync()
    {
        try
        {
            IsLoading = true;
            LoadingMessage = "Loading printers...";

            var availablePrinters = await _printService.GetAvailablePrintersAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                _printers.Clear();
                foreach (var printer in availablePrinters)
                {
                    _printers.Add(printer);
                }

                UpdateSelectedPrinter();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize printers");
            ErrorMessage = "Failed to load printers. Please try again.";
        }
        finally
        {
            IsLoading = false;
            LoadingMessage = string.Empty;
        }
    }

    private void UpdateSelectedPrinter()
    {
        var defaultPrinter = _printers.FirstOrDefault(p => p.IsOnline);
        if (defaultPrinter != null)
        {
            SelectedPrinter = defaultPrinter;
            _isPrinterAvailable = true;
        }
        else
        {
            _isPrinterAvailable = false;
            ErrorMessage = "No available printers found";
        }
    }

    private async Task RefreshPrintersAsync()
    {
        await InitializePrintersAsync();
    }

    private async Task ExecutePrintAsync()
    {
        if (!_isPrinterAvailable || SelectedPrinter == null)
        {
            ErrorMessage = "No printer available";
            return;
        }

        try
        {
            IsLoading = true;
            LoadingMessage = "Preparing document for printing...";

            var printSettings = new PrintSettings
            {
                PrinterName = SelectedPrinter.Name,
                Copies = Copies,
                IsColorPrinting = !IsBlackAndWhite
            };

            await _printService.PrintDocumentAsync(_documentPath, printSettings);

            _dialogWindow.DialogResult = true;
            _dialogWindow.Close();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Print failed");
            ErrorMessage = $"Failed to print: {ex.Message}";
            await RefreshPrintersAsync();
        }
        finally
        {
            IsLoading = false;
            LoadingMessage = string.Empty;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}