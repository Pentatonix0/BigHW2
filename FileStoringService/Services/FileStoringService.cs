using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using FileStoringService.Database;
using FileStoringService.Models;

namespace FileStoringService.Services
{
    public class FileStorageService : IFileStoringService
    {
        private readonly FileDBContext _dbContext;
        private readonly IWebHostEnvironment _environment;
        private readonly string _storagePath;

        public FileStorageService(FileDBContext dbContext, IWebHostEnvironment environment)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            // Define the path where files will be stored: wwwroot/Files
            _storagePath = Path.Combine(_environment.ContentRootPath, "wwwroot", "Files");
            Directory.CreateDirectory(_storagePath); // Ensure the storage directory exists
        }

        // Saves the file from the stream to disk and stores metadata in the database
        public async Task<string> SaveFileAsync(Stream stream, string fileName)
        {
            if (stream == null || stream.Length == 0)
                throw new ArgumentException("Stream is empty or null.");

            if (!fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Only .txt files are allowed.");

            var fileId = Guid.NewGuid().ToString();
            var safeFileName = $"{fileId}.txt";
            var filePath = Path.Combine(_storagePath, safeFileName);
            long fileSize;

            // Write the file to disk
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream);
                fileSize = fileStream.Length;
            }

            // Calculate SHA256 hash of the file for integrity and uniqueness check
            var fileHash = await CalculateFileHashAsync(filePath);

            // Create file metadata entity to store in database
            var metadata = new FileModel
            {
                Id = Guid.Parse(fileId),
                FileName = fileName,
                Path = filePath,
                Hash = fileHash,
                Size = fileSize,
                UploadDate = DateTime.UtcNow
            };

            _dbContext.Files.Add(metadata);
            await _dbContext.SaveChangesAsync(); // Save metadata to the database

            return fileId; // Return the unique identifier of the saved file
        }

        // Retrieves a file stream for a file by its name to serve its contents
        public async Task<Stream> GetFileAsync(string fileName)
        {
            var metadata = await _dbContext.Files
                .FirstOrDefaultAsync(f => f.FileName == fileName);
            if (metadata == null)
                throw new FileNotFoundException("File not found.");

            if (!File.Exists(metadata.Path))
                throw new FileNotFoundException("File content not found on disk.");

            // Return a readable file stream
            return new FileStream(metadata.Path, FileMode.Open, FileAccess.Read);
        }

        // Deletes the file and its metadata from disk and database
        public async Task<bool> DeleteFileAsync(string fileName)
        {
            var metadata = await _dbContext.Files
                .FirstOrDefaultAsync(f => f.FileName == fileName);
            if (metadata == null)
                return false;

            if (File.Exists(metadata.Path))
                File.Delete(metadata.Path); // Delete physical file from disk

            _dbContext.Files.Remove(metadata);
            await _dbContext.SaveChangesAsync(); // Remove database record

            return true;
        }

        // Gets the full file path for a given filename (used internally)
        public string GetFilePath(string fileName)
        {
            var metadata = _dbContext.Files
                .AsNoTracking()
                .FirstOrDefault(f => f.FileName == fileName);
            return metadata?.Path ?? throw new FileNotFoundException("File not found.");
        }

        // Retrieves file metadata by file ID
        public async Task<FileModel> GetFileInfoAsync(Guid id)
        {
            var metadata = await _dbContext.Files
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id);
            if (metadata == null)
                return null;

            if (!File.Exists(metadata.Path))
                return null;

            return metadata;
        }

        // Reads and returns the file content as byte array by file ID
        public async Task<byte[]> GetFileContentAsync(Guid id)
        {
            var metadata = await _dbContext.Files
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id);
            if (metadata == null)
                return null;

            if (!File.Exists(metadata.Path))
                return null;

            return await File.ReadAllBytesAsync(metadata.Path);
        }

        // Checks if a file with the specified hash already exists (to avoid duplicates)
        public async Task<bool> FileExistsAsync(string hash)
        {
            return await _dbContext.Files
                .AsNoTracking()
                .AnyAsync(f => f.Hash == hash);
        }

        // Returns all files that have corresponding physical files on disk
        public async Task<List<FileModel>> GetAllFilesAsync()
        {
            var files = await _dbContext.Files
                .AsNoTracking()
                .ToListAsync();

            // Filter only those files where physical file exists
            var validFiles = files.Where(f => File.Exists(f.Path)).ToList();

            return validFiles;
        }

        // Helper method to calculate SHA256 hash of a file
        private async Task<string> CalculateFileHashAsync(string filePath)
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hashBytes = await sha256.ComputeHashAsync(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
