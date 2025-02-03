using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Windows.Documents;
using WoodworkManagementApp.Models;
using WoodworkManagementApp.Services;

public class DocumentService : IDocumentService
{
    private readonly string _documentsPath;
    private readonly string _templatesPath;

    public DocumentService()
    {
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

    public async Task CreateOrderDocumentAsync(Order order)
    {
        var templatePath = Path.Combine(_templatesPath, "OrderTemplate.docx");
        var orderPath = Path.Combine(_documentsPath, $"{order.OrderNumber}.docx");

        // Copy template
        File.Copy(templatePath, orderPath, true);

        await Task.Run(() =>
        {
            using (WordprocessingDocument doc = WordprocessingDocument.Open(orderPath, true))
            {
                var mainPart = doc.MainDocumentPart;
                var body = mainPart.Document.Body;

                // Replace content controls with order data
                ReplaceContentControlText(body, "OrderNumber", order.OrderNumber);
                ReplaceContentControlText(body, "CreationDate", order.CreationDate.ToShortDateString());
                ReplaceContentControlText(body, "CreatorName", order.CreatorName);
                ReplaceContentControlText(body, "ReceiverName", order.ReceiverName);
                ReplaceContentControlText(body, "CompletionDate", order.CompletionDate);
                ReplaceContentControlText(body, "Comments", order.Comments);

                // Create products table
                var table = CreateProductsTable(order.Products);
                body.AppendChild(table);

                mainPart.Document.Save();
            }
        });
    }

    public async Task<Order> ReadOrderDocumentAsync(string orderNumber)
    {
        var orderPath = Path.Combine(_documentsPath, $"{orderNumber}.docx");
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

                // Parse creation date
                var creationDateStr = GetContentControlText(body, "CreationDate");
                if (DateTime.TryParse(creationDateStr, out DateTime creationDate))
                {
                    order.CreationDate = creationDate;
                }

                // Read products table
                order.Products = ReadProductsTable(body);
            }
        });

        return order;
    }

    public async Task UpdateOrderDocumentAsync(Order order)
    {
        await CreateOrderDocumentAsync(order); // For simplicity, we recreate the document
    }

    public async Task<byte[]> GeneratePreviewAsync(string orderNumber)
    {
        var orderPath = Path.Combine(_documentsPath, $"{orderNumber}.docx");

        // Here you would implement the actual preview generation
        // This is a placeholder implementation
        return await Task.FromResult(new byte[0]);
    }

    private Table CreateProductsTable(IEnumerable<OrderProduct> products)
    {
        var table = new Table();
        var props = new TableProperties(
            new TableBorders(
                new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single) },
                new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single) },
                new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single) },
                new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single) },
                new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single) },
                new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single) }
            )
        );
        table.AppendChild(props);

        // Add header row
        var headerRow = new TableRow();
        headerRow.Append(
            CreateCell("Produkt"),
            CreateCell("Ilość (m³)"),
            CreateCell("Sztuki"),
            CreateCell("Rabat (%)"),
            CreateCell("Cena całkowita")
        );
        table.AppendChild(headerRow);

        // Add product rows
        foreach (var product in products)
        {
            var row = new TableRow();
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

    private ObservableCollection<OrderProduct> ReadProductsTable(Body body)
    {
        var products = new ObservableCollection<OrderProduct>();
        var table = body.Elements<Table>().FirstOrDefault();

        if (table != null)
        {
            var rows = table.Elements<TableRow>().Skip(1); // Skip header row
            foreach (var row in rows)
            {
                var cells = row.Elements<TableCell>().ToList();
                if (cells.Count >= 5)
                {
                    var product = new OrderProduct
                    {
                        // Note: This is simplified - you'd need to look up the actual Product object
                        Volume = decimal.Parse(GetCellText(cells[1])),
                        Pieces = int.Parse(GetCellText(cells[2])),
                        Discount = decimal.Parse(GetCellText(cells[3]))
                    };
                    products.Add(product);
                }
            }
        }

        return products;
    }

    private TableCell CreateCell(string text)
    {
        return new TableCell(new Paragraph(new Run(new Text(text))));
    }

    private string GetCellText(TableCell cell)
    {
        return cell.InnerText;
    }

    private void ReplaceContentControlText(Body body, string tag, string text)
    {
        var controls = body.Descendants<SdtElement>()
            .Where(sdt => sdt.SdtProperties.GetFirstChild<Tag>()?.Val == tag);

        foreach (var control in controls)
        {
            var run = new Run(new Text(text));
            control.SdtProperties.RemoveAllChildren();
            control.InnerXml = run.OuterXml;
        }
    }

    private string GetContentControlText(Body body, string tag)
    {
        var control = body.Descendants<SdtElement>()
            .FirstOrDefault(sdt => sdt.SdtProperties.GetFirstChild<Tag>()?.Val == tag);

        return control?.InnerText ?? string.Empty;
    }
}