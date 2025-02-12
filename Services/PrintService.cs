using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Printing;
using System.Windows.Xps;
using System.Windows;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Windows.Xps.Packaging;

namespace WoodworkManagementApp.Services
{
    public class PrintService : IPrintService
    {
        private readonly ILogger<PrintService> _logger;
        private readonly PrintServer _printServer;

        public PrintService(ILogger<PrintService> logger)
        {
            _logger = logger;
            _printServer = new LocalPrintServer();
        }

        public async Task<IEnumerable<PrinterInfo>> GetAvailablePrintersAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var printers = new List<PrinterInfo>();
                    var printQueues = _printServer.GetPrintQueues(new[]
                    {
                    EnumeratedPrintQueueTypes.Local,
                    EnumeratedPrintQueueTypes.Connections
                });

                    foreach (var queue in printQueues)
                    {
                        try
                        {
                            using (queue)
                            {
                                var info = new PrinterInfo
                                {
                                    Name = queue.FullName,
                                    Status = queue.QueueStatus.ToString(),
                                    IsOnline = queue.QueueStatus == PrintQueueStatus.None,
                                    SupportsColor = queue.DefaultPrintTicket?.OutputColor == OutputColor.Color,
                                    IsDuplexSupported = queue.DefaultPrintTicket?.Duplexing != Duplexing.OneSided
                                };
                                printers.Add(info);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error getting info for printer {PrinterName}", queue.Name);
                        }
                    }

                    return printers;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting available printers");
                    return Enumerable.Empty<PrinterInfo>();
                }
            });
        }

        public async Task<bool> PrintDocumentAsync(string documentPath, PrintSettings settings)
        {
            if (!File.Exists(documentPath))
                throw new FileNotFoundException("Document not found", documentPath);

            return await Task.Run(() =>
            {
                try
                {
                    using var printQueue = _printServer.GetPrintQueue(settings.PrinterName);
                    if (printQueue.QueueStatus != PrintQueueStatus.None)
                    {
                        throw new InvalidOperationException($"Printer {settings.PrinterName} is not ready");
                    }

                    // Create print ticket
                    var printTicket = printQueue.DefaultPrintTicket;
                    printTicket.CopyCount = settings.Copies;
                    printTicket.OutputColor = settings.IsColorPrinting ? OutputColor.Color : OutputColor.Monochrome;
                    printTicket.Duplexing = settings.IsDuplex ? Duplexing.TwoSidedLongEdge : Duplexing.OneSided;

                    // Convert document to XPS if needed
                    var xpsPath = Path.Combine(Path.GetTempPath(), $"print_{Guid.NewGuid()}.xps");
                    try
                    {
                        var wordDoc = new Aspose.Words.Document(documentPath);
                        try
                        {
                            wordDoc.Save(xpsPath, Aspose.Words.SaveFormat.Xps);
                        }
                        finally
                        {
                            if (wordDoc is IDisposable disposable)
                            {
                                disposable.Dispose();
                            }
                        }

                        using var xpsDoc = new XpsDocument(xpsPath, FileAccess.Read);
                        var xpsPrintDoc = xpsDoc.GetFixedDocumentSequence();

                        var writer = PrintQueue.CreateXpsDocumentWriter(printQueue);
                        writer.Write(xpsPrintDoc, printTicket);

                        return true;
                    }
                    finally
                    {
                        try
                        {
                            if (File.Exists(xpsPath))
                            {
                                File.Delete(xpsPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete temporary XPS file: {Path}", xpsPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error printing document {DocumentPath}", documentPath);
                    throw;
                }
            });
        }

        public async Task<string> GetDefaultPrinterNameAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var defaultPrintQueue = LocalPrintServer.GetDefaultPrintQueue();
                    return defaultPrintQueue.Name;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting default printer name");
                    return string.Empty;
                }
            });
        }

        public async Task<PrinterInfo> GetPrinterInfoAsync(string printerName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var printQueue = new PrintQueue(_printServer, printerName);
                    return new PrinterInfo
                    {
                        Name = printQueue.Name,
                        Status = printQueue.QueueStatus.ToString(),
                        IsOnline = printQueue.QueueStatus == PrintQueueStatus.None,
                        SupportsColor = printQueue.DefaultPrintTicket?.OutputColor == OutputColor.Color,
                        IsDuplexSupported = printQueue.DefaultPrintTicket?.Duplexing != Duplexing.OneSided
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting printer info for {PrinterName}", printerName);
                    return null;
                }
            });
        }
    }
}