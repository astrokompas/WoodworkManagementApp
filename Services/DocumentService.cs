using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocWord = DocumentFormat.OpenXml.Wordprocessing;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using WoodworkManagementApp.Models;
using WoodworkManagementApp.Services;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Xps.Packaging;
using System.Windows;
using WoodworkManagementApp.Helpers;
using Microsoft.Extensions.Logging;


public class DocumentService : IDocumentService
{
    private bool _disposed;
    private readonly string _documentsPath;
    private readonly string _templatesPath;
    private readonly IProductService _productsService;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        IProductService productsService,
        ILogger<DocumentService> logger)
    {
        _productsService = productsService;
        _logger = logger;

        _documentsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "WoodworkManagementApp",
            "Orders"
        );
        _templatesPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "WoodworkManagementApp",
            "Templates"
        );

        Directory.CreateDirectory(_documentsPath);
        Directory.CreateDirectory(_templatesPath);
    }

    private void UpdateDocument(WordprocessingDocument doc, Order order)
    {
        var body = doc.MainDocumentPart.Document.Body;

        ReplaceContentControlText(body, "OrderNumber", order.OrderNumber);
        ReplaceContentControlText(body, "CreationDate", order.CreationDate.ToShortDateString());
        ReplaceContentControlText(body, "CreatorName", order.CreatorName);
        ReplaceContentControlText(body, "ReceiverName", order.ReceiverName);
        ReplaceContentControlText(body, "CompletionDate", order.CompletionDate);
        ReplaceContentControlText(body, "Comments", order.Comments);

        var table = CreateProductsTable(order.Products);
        body.AppendChild(table);

        doc.MainDocumentPart.Document.Save();
    }

    public async Task CreateOrderDocumentAsync(Order order)
    {
        if (order == null) throw new ArgumentNullException(nameof(order));

        var templatePath = Path.Combine(_templatesPath, "OrderTemplate.docx");
        var orderPath = Path.Combine(_documentsPath, $"{order.OrderNumber}.docx");
        var tempPath = Path.Combine(_documentsPath, $"temp_{Guid.NewGuid()}.docx");

        try
        {
            FileValidator.ValidateWordDocument(templatePath);
            File.Copy(templatePath, tempPath, true);

            await Task.Run(() =>
            {
                using var doc = WordprocessingDocument.Open(tempPath, true);
                UpdateDocument(doc, order);
            });

            // Atomic file operation
            if (File.Exists(orderPath))
            {
                var backup = orderPath + ".bak";
                File.Move(orderPath, backup, true);
                try
                {
                    File.Move(tempPath, orderPath, true);
                    File.Delete(backup);
                }
                catch
                {
                    if (File.Exists(backup))
                    {
                        File.Move(backup, orderPath, true);
                    }
                    throw;
                }
            }
            else
            {
                File.Move(tempPath, orderPath, true);
            }
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                try
                {
                    File.Delete(tempPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete temporary file: {Path}", tempPath);
                }
            }
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Cleanup code
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async Task<Order> ReadOrderDocumentAsync(string orderNumber)
    {
        var orderPath = Path.Combine(_documentsPath, $"{orderNumber}.docx");

        FileValidator.ValidateWordDocument(orderPath);

        var order = new Order();

        await Task.Run(() =>
        {
            using (WordprocessingDocument doc = WordprocessingDocument.Open(orderPath, false))
            {
                var body = doc.MainDocumentPart.Document.Body;

                order.OrderNumber = GetContentControlText(body, "OrderNumber");
                order.CreatorName = GetContentControlText(body, "CreatorName");
                order.ReceiverName = GetContentControlText(body, "ReceiverName");
                order.CompletionDate = GetContentControlText(body, "CompletionDate");
                order.Comments = GetContentControlText(body, "Comments");

                var creationDateStr = GetContentControlText(body, "CreationDate");
                if (DateTime.TryParse(creationDateStr, out DateTime creationDate))
                {
                    order.CreationDate = creationDate;
                }

                order.Products = ReadProductsTable(body);
            }
        });

        return order;
    }

    public async Task UpdateOrderDocumentAsync(Order order)
    {
        await CreateOrderDocumentAsync(order);
    }

    public async Task<byte[]> GeneratePreviewAsync(string orderNumber)
    {
        var orderPath = Path.Combine(_documentsPath, $"{orderNumber}.docx");

        FileValidator.ValidateWordDocument(orderPath);

        return await Task.Run(() =>
        {
            var doc = new Aspose.Words.Document(orderPath);
            try
            {
                using var stream = new MemoryStream();
                var options = new Aspose.Words.Saving.ImageSaveOptions(Aspose.Words.SaveFormat.Png)
                {
                    Resolution = 96
                };

                doc.Save(stream, options);
                return stream.ToArray();
            }
            finally
            {
                if (doc is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        });
    }

    private DocWord.TableProperties CreateTableProperties()
    {
        return new DocWord.TableProperties(
            new DocWord.TableBorders(
                new DocWord.TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                new DocWord.BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                new DocWord.LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                new DocWord.RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                new DocWord.InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                new DocWord.InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 }
            ),
            new DocWord.TableWidth { Type = DocWord.TableWidthUnitValues.Pct, Width = "5000" },
            new DocWord.TableLayout { Type = DocWord.TableLayoutValues.Fixed },
            new DocWord.TableLook { Val = new HexBinaryValue() { Value = "04A0" } }
        );
    }

    private void SetTableColumnWidths(DocWord.Table table)
    {
        var gridCol1 = new DocWord.GridColumn() { Width = "2400" };
        var gridCol2 = new DocWord.GridColumn() { Width = "1200" };
        var gridCol3 = new DocWord.GridColumn() { Width = "1200" };
        var gridCol4 = new DocWord.GridColumn() { Width = "1200" };
        var gridCol5 = new DocWord.GridColumn() { Width = "1400" };

        var grid = new DocWord.TableGrid(gridCol1, gridCol2, gridCol3, gridCol4, gridCol5);
        table.AppendChild(grid);
    }

    private DocWord.Table CreateProductsTable(IEnumerable<OrderProduct> products)
    {
        var table = new DocWord.Table();
        var props = new DocWord.TableProperties(
            new DocWord.TableBorders(
                new DocWord.TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single) },
                new DocWord.BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single) },
                new DocWord.LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single) },
                new DocWord.RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single) },
                new DocWord.InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single) },
                new DocWord.InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single) }
            )
        );
        table.AppendChild(props);

        var headerRow = new DocWord.TableRow();
        headerRow.Append(
            CreateCell("Produkt"),
            CreateCell("Ilość (m³)"),
            CreateCell("Sztuki"),
            CreateCell("Rabat (%)"),
            CreateCell("Cena całkowita")
        );
        table.AppendChild(headerRow);

        foreach (var product in products)
        {
            var row = new DocWord.TableRow();
            row.Append(
                CreateCell(product.Product.Name),
                CreateCell(product.Volume.ToString("N2")),
                CreateCell(product.Pieces.ToString()),
                CreateCell(product.Discount.ToString("N2")),
                CreateCell(product.TotalPrice.ToString("C2"))
            );
            table.AppendChild(row);
        }

        return table;
    }

    private ObservableCollection<OrderProduct> ReadProductsTable(DocWord.Body body)
    {
        var products = new ObservableCollection<OrderProduct>();
        var table = body.Elements<DocWord.Table>().FirstOrDefault();

        if (table != null)
        {
            var rows = table.Elements<DocWord.TableRow>().Skip(1);
            foreach (var row in rows)
            {
                var cells = row.Elements<DocWord.TableCell>().ToList();
                if (cells.Count >= 5)
                {
                    var productName = GetCellText(cells[0]);
                    var volume = decimal.Parse(GetCellText(cells[1]));
                    var pieces = int.Parse(GetCellText(cells[2]));
                    var discount = decimal.Parse(GetCellText(cells[3]));

                    var product = _productsService.GetProductByName(productName);
                    if (product != null)
                    {
                        var orderProduct = new OrderProduct
                        {
                            Product = product,
                            Volume = volume,
                            Pieces = pieces,
                            Discount = discount
                        };
                        products.Add(orderProduct);
                    }
                }
            }
        }
        return products;
    }

    private DocWord.TableCell CreateCell(string text)
    {
        return new DocWord.TableCell(
            new DocWord.Paragraph(
                new DocWord.Run(
                    new DocWord.Text(text)
                )
            )
        );
    }

    private string GetCellText(DocWord.TableCell cell)
    {
        return cell.InnerText;
    }

    private void ReplaceContentControlText(DocWord.Body body, string tag, string text)
    {
        var controls = body.Descendants<DocWord.SdtElement>()
            .Where(sdt => sdt.SdtProperties.GetFirstChild<DocWord.Tag>()?.Val == tag);

        foreach (var control in controls)
        {
            var run = new DocWord.Run(new DocWord.Text(text));
            control.SdtProperties.RemoveAllChildren();
            control.InnerXml = run.OuterXml;
        }
    }

    private string GetContentControlText(DocWord.Body body, string tag)
    {
        var control = body.Descendants<DocWord.SdtElement>()
            .FirstOrDefault(sdt => sdt.SdtProperties.GetFirstChild<DocWord.Tag>()?.Val == tag);

        return control?.InnerText ?? string.Empty;
    }
}