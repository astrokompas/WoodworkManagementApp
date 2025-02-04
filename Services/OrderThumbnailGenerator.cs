using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoodworkManagementApp.Services
{
    public class OrderThumbnailGenerator : IOrderThumbnailGenerator
    {
        public async Task<byte[]> GenerateThumbnailAsync(string documentPath)
        {
            return await Task.Run(() =>
            {
                using (var doc = new Aspose.Words.Document(documentPath))
                {
                    using (var stream = new MemoryStream())
                    {
                        var options = new Aspose.Words.Saving.ImageSaveOptions(Aspose.Words.SaveFormat.Png)
                        {
                            PageCount = 1,
                            Resolution = 96
                        };

                        doc.Save(stream, options);
                        return stream.ToArray();
                    }
                }
            });
        }
    }
}
