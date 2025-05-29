using FileAnalysisService.Models;


namespace FileAnalysisService.Services
{
	public interface IFileAnalysisService
	{
		Task<FileAnalysisResultModel> AnalyzeFileAsync(Guid fileId, string content);
		Task<FileAnalysisResultModel> GetAnalysisResultAsync(Guid fileId);
	}
}