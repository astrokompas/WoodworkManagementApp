using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoodworkManagementApp.Services
{
    public interface IOrderThumbnailGenerator
    {
        Task<byte[]> GenerateThumbnailAsync(string documentPath);
    }
}
