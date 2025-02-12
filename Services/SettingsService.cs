using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using WoodworkManagementApp.Models;
using WoodworkManagementApp.Helpers;
using Microsoft.Extensions.Logging;

namespace WoodworkManagementApp.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ILogger<SettingsService> _logger;
        private readonly SemaphoreSlim _settingsLock = new SemaphoreSlim(1, 1);
        private readonly string _settingsPath;
        private readonly IJsonStorageService _jsonStorage;
        private static readonly object _folderLock = new object();
        private readonly string[] _requiredTemplates = { "OrderTemplate.docx" };

        public SettingsService(
            IJsonStorageService jsonStorage,
            ILogger<SettingsService> logger)
        {
            _jsonStorage = jsonStorage;
            _logger = logger;
            _settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "WoodworkManagementApp",
                "settings.json"
            );
        }

        public async Task<AppSettings> LoadSettingsAsync()
        {
            try
            {
                var settings = await _jsonStorage.LoadAsync<AppSettings>("settings.json");
                return settings ?? new AppSettings();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load settings, using defaults");
                return new AppSettings();
            }
        }

        public async Task SaveSettingsAsync(AppSettings settings)
        {
            await _settingsLock.WaitAsync();
            try
            {
                await _jsonStorage.SaveAsync(settings, "settings.json");
            }
            finally
            {
                _settingsLock.Release();
            }
        }

        public bool IsOrdersFolderAvailable()
        {
            try
            {
                var settings = LoadSettingsAsync().GetAwaiter().GetResult();

                if (!Directory.Exists(settings.OrdersPath))
                    return true;

                var testFile = Path.Combine(settings.OrdersPath, $"test_{Guid.NewGuid()}.tmp");

                using (var fs = File.Create(testFile))
                {
                    fs.Write(new byte[] { 0 }, 0, 1);
                }
                File.Delete(testFile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void EnsureDirectoriesExist()
        {
            var settings = LoadSettingsAsync().GetAwaiter().GetResult();

            lock (_folderLock)
            {
                if (!Directory.Exists(settings.OrdersPath))
                {
                    Directory.CreateDirectory(settings.OrdersPath);
                    Directory.CreateDirectory(Path.Combine(settings.OrdersPath, "locks"));
                }

                if (!Directory.Exists(settings.TemplatesPath))
                {
                    Directory.CreateDirectory(settings.TemplatesPath);
                }
            }
        }

        public async Task ValidateSettingsAsync(AppSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var errors = new List<string>();

            if (!Directory.Exists(settings.OrdersPath))
            {
                try
                {
                    Directory.CreateDirectory(settings.OrdersPath);
                }
                catch (Exception ex)
                {
                    errors.Add($"Cannot create orders directory: {ex.Message}");
                }
            }
            if (!Directory.Exists(settings.TemplatesPath))
            {
                errors.Add("Templates directory does not exist");
            }
            else
            {
                foreach (var template in _requiredTemplates)
                {
                    var templatePath = Path.Combine(settings.TemplatesPath, template);
                    if (!File.Exists(templatePath))
                    {
                        errors.Add($"Required template missing: {template}");
                    }
                    else
                    {
                        try
                        {
                            await ValidateTemplateStructureAsync(templatePath);
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Invalid template {template}: {ex.Message}");
                        }
                    }
                }
            }

            if (errors.Any())
            {
                throw new SettingsValidationException(errors);
            }
        }

        private async Task ValidateTemplateStructureAsync(string templatePath)
        {
            try
            {
                using var doc = WordprocessingDocument.Open(templatePath, false);
                var mainPart = doc.MainDocumentPart;
                if (mainPart == null)
                    throw new InvalidOperationException("Template is missing main document part");

                // Validate required content controls
                var requiredTags = new[] { "OrderNumber", "CreationDate", "CreatorName", "ReceiverName" };
                var existingTags = mainPart.Document.Descendants<SdtElement>()
                    .Select(sdt => sdt.SdtProperties?.GetFirstChild<Tag>()?.Val?.Value)
                    .Where(tag => !string.IsNullOrEmpty(tag))
                    .ToList();

                var missingTags = requiredTags.Where(tag => !existingTags.Contains(tag)).ToList();
                if (missingTags.Any())
                {
                    throw new InvalidOperationException(
                        $"Template is missing required content controls: {string.Join(", ", missingTags)}");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to validate template structure", ex);
            }
        }
    }
}
