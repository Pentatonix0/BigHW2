using FileAnalysisService.DTO;

namespace FileAnalysisService.Clients
{

	public interface IFileStoringServiceClient
	{
		Task<FileDto> GetFileContentAsync(Guid fileId);
	}
}