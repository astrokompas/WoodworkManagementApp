using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WoodworkManagementApp.Services
{
    public class OrderThumbnailGenerator : IOrderThumbnailGenerator, IDisposable
    {
        private readonly string _tempPath;
        private readonly HashSet<string> _tempFiles = new();
        private readonly object _lockObject = new();
        private readonly ILogger<OrderThumbnailGenerator> _logger;
        private bool _disposed;

        public OrderThumbnailGenerator(ILogger<OrderThumbnailGenerator> logger)
        {
            _logger = logger;
            _tempPath = Path.Combine(Path.GetTempPath(), "WoodworkManagementApp", "Thumbnails");
            Directory.CreateDirectory(_tempPath);
        }

        public async Task<byte[]> GenerateThumbnailAsync(string documentPath)
        {
            if (!File.Exists(documentPath))
            {
                throw new FileNotFoundException("Document not found", documentPath);
            }

            var tempFile = Path.Combine(_tempPath, $"thumb_{Guid.NewGuid()}.png");
            lock (_lockObject)
            {
                _tempFiles.Add(tempFile);
            }

            try
            {
                var doc = new Aspose.Words.Document(documentPath);
                try
                {
                    var options = new Aspose.Words.Saving.ImageSaveOptions(Aspose.Words.SaveFormat.Png)
                    {
                        Resolution = 96
                    };

                    // Set page to render
                    options.PageSet = new Aspose.Words.Saving.PageSet(0); // This replaces PageIndex

                    await Task.Run(() => doc.Save(tempFile, options));
                    var thumbnailBytes = await File.ReadAllBytesAsync(tempFile);
                    return thumbnailBytes;
                }
                finally
                {
                    if (doc is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate thumbnail for document: {Path}", documentPath);
                throw;
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
                _logger.LogWarning(ex, "Failed to delete temporary file: {Path}", tempFile);
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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
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
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete temporary file during disposal: {Path}", file);
                    }
                }
            }

            _disposed = true;
        }

        ~OrderThumbnailGenerator()
        {
            Dispose(false);
        }
    }
}