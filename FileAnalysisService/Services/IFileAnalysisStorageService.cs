using System.Threading.Tasks;

namespace FileAnalysisService.Services
{
    public interface IFileAnalysisStorageService
    {
        Task<string> SaveWordCloudAsync(byte[] content);
        Task<bool> DeleteWordCloudAsync(string filePath);
        Task<bool> FileExistsAsync(string filePath);
        Task<string> GetWordCloudPathAsync(Guid fileId); // Новый метод
    }
}