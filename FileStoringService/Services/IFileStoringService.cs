using System;
using System.IO;
using System.Threading.Tasks;
using FileStoringService.Models;

namespace FileStoringService.Services
{
    public interface IFileStoringService
    {
        Task<string> SaveFileAsync(Stream stream, string fileName);
        Task<Stream> GetFileAsync(string fileName);
        Task<bool> DeleteFileAsync(string fileName);
        string GetFilePath(string fileName);
        Task<FileModel> GetFileInfoAsync(Guid id);
        Task<byte[]> GetFileContentAsync(Guid id);
        Task<List<FileModel>> GetAllFilesAsync(); // Новый метод
    }
}