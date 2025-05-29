using FileAnalysisService.Models;
namespace FileAnalysisService.Services
{
	public interface IFilesPlagiarismService
	{
		Task<PlagiarismModel> CompareTwoFilesAsync(Guid FileId, Guid otherFileId, string fileContent, string otherFContent);
	}
}