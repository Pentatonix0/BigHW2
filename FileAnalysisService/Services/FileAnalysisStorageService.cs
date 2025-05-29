using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using FileAnalysisService.Models;

namespace FileAnalysisService.Services
{
    public class FileAnalysisStorageService : IFileAnalysisStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _storagePath;

        public FileAnalysisStorageService(IWebHostEnvironment environment)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));

            // Store word clouds in a temp folder under "wordclouds"
            _storagePath = Path.Combine(Path.GetTempPath(), "wordclouds");
            Directory.CreateDirectory(_storagePath);
        }

        // Saves the word cloud image and returns a web-accessible URL path
        public async Task<string> SaveWordCloudAsync(byte[] content)
        {
            if (content == null || content.Length == 0)
                throw new ArgumentException("Content is empty or null.");

            var fileId = Guid.NewGuid().ToString();
            var filePath = Path.Combine(_storagePath, $"{fileId}.png");

            await File.WriteAllBytesAsync(filePath, content);

            // Return relative URL path for client usage
            return $"/wordclouds/{fileId}.png";
        }

        // Deletes a word cloud file if it exists
        public async Task<bool> DeleteWordCloudAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return false;

            try
            {
                // Convert relative URL to physical path before deletion
                File.Delete(filePath.Replace("/wordclouds/", _storagePath + "/"));
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Checks if a word cloud file exists
        public async Task<bool> FileExistsAsync(string filePath)
        {
            return File.Exists(filePath.Replace("/wordclouds/", _storagePath + "/"));
        }

        // Retrieves the relative URL path of a word cloud image if it exists
        public async Task<string> GetWordCloudPathAsync(Guid fileId)
        {
            var fileName = $"{fileId}.png";
            var filePath = Path.Combine(_storagePath, fileName);

            if (File.Exists(filePath))
            {
                return $"/wordclouds/{fileName}";
            }

            return null;
        }
    }
}
