using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using WoodworkManagementApp.Services;


namespace WoodworkManagementApp.Services
{
    public class JsonStorageService : IJsonStorageService
    {
        private readonly string _appDataPath;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNameCaseInsensitive = true
        };

        public JsonStorageService()
        {
            _appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AkacjowyKalkulator"
            );
            EnsureDirectoryExists();
        }

        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_appDataPath))
            {
                Directory.CreateDirectory(_appDataPath);
            }
        }

        private async Task EnsureFileExistsAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    await File.WriteAllTextAsync(filePath, "[]", Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    throw new IOException($"Failed to create file: {filePath}", ex);
                }
            }
        }

        private string GetFilePath(string fileName)
        {
            return Path.Combine(_appDataPath, fileName);
        }

        public async Task SaveAsync<T>(T data, string fileName = "data.json")
        {
            var filePath = GetFilePath(fileName);
            await EnsureFileExistsAsync(filePath);
            var jsonString = JsonSerializer.Serialize(data, JsonOptions);
            await File.WriteAllTextAsync(filePath, jsonString, Encoding.UTF8);
        }

        public async Task<T> LoadAsync<T>(string fileName = "data.json") where T : new()
        {
            var filePath = GetFilePath(fileName);
            await EnsureFileExistsAsync(filePath);
            var jsonString = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
            return JsonSerializer.Deserialize<T>(jsonString, JsonOptions) ?? new T();
        }
    }
}
