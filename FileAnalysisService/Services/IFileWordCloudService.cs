namespace FileAnalysisService.Services
{
    public interface IFileWordCloudService
    {
        Task<string> CreateWordCloudAsync(string text);
        Task<string> GetWordCloudAsync(Guid fileId); // Новый метод для получения пути к word cloud
    }
}