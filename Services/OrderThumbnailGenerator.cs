using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoodworkManagementApp.Services
{
    public class OrderThumbnailGenerator : IOrderThumbnailGenerator, IDisposable
    {
        private readonly string _tempPath;
        private readonly HashSet<string> _tempFiles = new();
        private readonly object _lockObject = new();

        public async Task<byte[]> GenerateThumbnailAsync(string documentPath)
        {
            var tempFile = Path.Combine(_tempPath, $"thumb_{Guid.NewGuid()}.png");
            lock (_lockObject)
            {
                _tempFiles.Add(tempFile);
            }

            try
            {
                using var doc = new Aspose.Words.Document(documentPath);
                var options = new Aspose.Words.Saving.ImageSaveOptions(Aspose.Words.SaveFormat.Png)
                {
                    PageIndex = 0,
                    Resolution = 96
                };

                await Task.Run(() => doc.Save(tempFile, options));
                var thumbnailBytes = await File.ReadAllBytesAsync(tempFile);
                return thumbnailBytes;
            }
            finally
            {
                await CleanupTempFileAsync(tempFile);
            }
        }

        private async Task CleanupTempFileAsync(string tempFile)
        {
            try
            {
                if (File.Exists(tempFile))
                {
                    await Task.Run(() => File.Delete(tempFile));
                }
            }
            catch (Exception ex)
            {
                // Log but don't throw - will be cleaned up later
                Debug.WriteLine($"Failed to delete temp file {tempFile}: {ex.Message}");
            }
            finally
            {
                lock (_lockObject)
                {
                    _tempFiles.Remove(tempFile);
                }
            }
        }

        public void Dispose()
        {
            string[] filesToDelete;
            lock (_lockObject)
            {
                filesToDelete = _tempFiles.ToArray();
                _tempFiles.Clear();
            }

            foreach (var file in filesToDelete)
            {
                try
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
                catch
                {
                    // Just log, nothing else we can do during disposal
                }
            }
        }
    }
}
