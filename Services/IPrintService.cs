using System.Collections.Generic;
using System.Threading.Tasks;

namespace WoodworkManagementApp.Services
{
    public class PrinterInfo
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public bool IsOnline { get; set; }
        public bool SupportsColor { get; set; }
        public bool IsDuplexSupported { get; set; }
    }

    public class PrintSettings
    {
        public string PrinterName { get; set; }
        public int Copies { get; set; } = 1;
        public bool IsColorPrinting { get; set; } = false;
        public bool IsDuplex { get; set; } = false;
        public double ScalingFactor { get; set; } = 1.0;
    }

    public interface IPrintService
    {
        Task<IEnumerable<PrinterInfo>> GetAvailablePrintersAsync();
        Task<bool> PrintDocumentAsync(string documentPath, PrintSettings settings);
        Task<string> GetDefaultPrinterNameAsync();
        Task<PrinterInfo> GetPrinterInfoAsync(string printerName);
    }
}