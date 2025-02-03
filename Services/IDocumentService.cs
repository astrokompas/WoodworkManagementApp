using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoodworkManagementApp.Models;

namespace WoodworkManagementApp.Services
{
    public interface IDocumentService
    {
        Task CreateOrderDocumentAsync(Order order);
        Task<Order> ReadOrderDocumentAsync(string orderNumber);
        Task UpdateOrderDocumentAsync(Order order);
        Task<byte[]> GeneratePreviewAsync(string orderNumber);
    }
}
