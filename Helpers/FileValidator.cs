﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoodworkManagementApp.Helpers
{
    public static class FileValidator
    {
        private static readonly string[] AllowedWordExtensions = { ".docx", ".doc" };
        private static readonly byte[] DocxHeader = { 0x50, 0x4B, 0x03, 0x04 };
        private static readonly byte[] DocHeader = { 0xD0, 0xCF, 0x11, 0xE0 };

        public static bool IsValidWordDocument(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            var extension = Path.GetExtension(filePath).ToLower();
            if (!AllowedWordExtensions.Contains(extension))
                return false;

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    if (stream.Length < 4)
                        return false;
                    var header = new byte[4];
                    stream.Read(header, 0, 4);
                    return extension == ".docx"
                        ? header.SequenceEqual(DocxHeader)
                        : header.SequenceEqual(DocHeader);
                }
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> IsValidWordDocumentAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            var extension = Path.GetExtension(filePath).ToLower();
            if (!AllowedWordExtensions.Contains(extension))
                return false;

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (stream.Length < 4)
                        return false;
                    var header = new byte[4];
                    await stream.ReadAsync(header, 0, 4);
                    return extension == ".docx"
                        ? header.SequenceEqual(DocxHeader)
                        : header.SequenceEqual(DocHeader);
                }
            }
            catch
            {
                return false;
            }
        }

        public static void ValidateWordDocument(string filePath)
        {
            if (!IsValidWordDocument(filePath))
            {
                throw new InvalidOperationException("File is not a valid Word document.");
            }
        }

        public static async Task ValidateWordDocumentAsync(string filePath)
        {
            if (!await IsValidWordDocumentAsync(filePath))
            {
                throw new InvalidOperationException("File is not a valid Word document.");
            }
        }
    }
}
