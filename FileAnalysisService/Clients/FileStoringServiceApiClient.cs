using System.Net.Http.Json;
using FileAnalysisService.DTO;

namespace FileAnalysisService.Clients
{
	public class FileStoringServiceClient : IFileStoringServiceClient
	{
		private readonly HttpClient _httpClient;
		private readonly string _baseUrl;

		// Internal DTO to deserialize metadata from FileStoringService
		private class FileMetadataDto
		{
			public Guid Id { get; set; }
			public string FileName { get; set; }
			public string ContentType { get; set; }
			public long FileSize { get; set; }
			public DateTime UploadedAt { get; set; }
			public string Hash { get; set; }
		}

		public FileStoringServiceClient(HttpClient httpClient, IConfiguration configuration)
		{
			_httpClient = httpClient;

			// Get base URL of FileStoringService from configuration
			_baseUrl = configuration["ServiceUrls:FileStoringService"]
				?? throw new InvalidOperationException("FileStoringService URL is not configured.");

			// Ensure HttpClient has a base address set
			if (_httpClient.BaseAddress == null && !string.IsNullOrEmpty(_baseUrl))
			{
				_httpClient.BaseAddress = new Uri(_baseUrl.EndsWith('/') ? _baseUrl : _baseUrl + "/");
			}
			else if (_httpClient.BaseAddress == null)
			{
				throw new InvalidOperationException("HttpClient BaseAddress is not set.");
			}
		}

		public async Task<FileDto> GetFileContentAsync(Guid fileId)
		{
			var metadataEndpoint = $"api/files/metadata/{fileId}";
			var metadataResponse = await _httpClient.GetAsync(metadataEndpoint);

			// If metadata is not available, return null
			if (!metadataResponse.IsSuccessStatusCode)
			{
				return null;
			}

			var metadata = await metadataResponse.Content.ReadFromJsonAsync<FileMetadataDto>();
			if (metadata == null)
			{
				return null;
			}

			var downloadEndpoint = $"api/files/content/{fileId}";
			var fileResponse = await _httpClient.GetAsync(downloadEndpoint);

			// If file content cannot be fetched, return null
			if (!fileResponse.IsSuccessStatusCode)
			{
				return null;
			}

			var content = await fileResponse.Content.ReadAsStringAsync();
			if (string.IsNullOrEmpty(content))
			{
				return null;
			}

			// Combine metadata and file content into a DTO
			return new FileDto
			{
				Id = metadata.Id,
				FileName = metadata.FileName,
				Content = content
			};
		}
	}
}
