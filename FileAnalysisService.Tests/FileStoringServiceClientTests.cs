using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Moq;
using RichardSzalay.MockHttp;
using FileAnalysisService.Clients;
using FileAnalysisService.DTO;
using Xunit;

namespace FileAnalysisService.Tests.Clients
{
	public class FileStoringServiceClientTests
	{
		private readonly MockHttpMessageHandler _mockHandler;
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;

		public FileStoringServiceClientTests()
		{
			_mockHandler = new MockHttpMessageHandler();
			_httpClient = new HttpClient(_mockHandler);
			_httpClient.BaseAddress = new Uri("http://filestoringservice:8080/");

			var configData = new Dictionary<string, string>
			{
				{ "ServiceUrls:FileStoringService", "http://filestoringservice:8080" }
			};
			_configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(configData)
				.Build();
		}

		[Fact]
		public async Task GetFileContentAsync_ValidFileId_ReturnsFileDto()
		{
			var fileId = Guid.NewGuid();
			_mockHandler.When($"*/api/files/{fileId}/metadata")
						.Respond("application/json", $"{{\"id\":\"{fileId}\",\"originalFileName\":\"test.txt\",\"contentType\":\"text/plain\",\"fileSize\":10,\"uploadedAt\":\"2025-05-29T00:00:00Z\",\"hash\":\"abc\"}}");
			_mockHandler.When($"*/api/files/{fileId}/download")
						.Respond("text/plain", "привет мир");

			var client = new FileStoringServiceClient(_httpClient, _configuration);

			var result = await client.GetFileContentAsync(fileId);

			Assert.NotNull(result);
			Assert.Equal(fileId, result.Id);
			Assert.Equal("test.txt", result.FileName);
			Assert.Equal("привет мир", result.Content);
		}

		[Fact]
		public async Task GetFileContentAsync_MetadataRequestFails_ReturnsNull()
		{
			var fileId = Guid.NewGuid();
			_mockHandler.When($"*/api/files/{fileId}/metadata")
						.Respond(System.Net.HttpStatusCode.NotFound);

			var client = new FileStoringServiceClient(_httpClient, _configuration);

			var result = await client.GetFileContentAsync(fileId);

			Assert.Null(result);
		}

		[Fact]
		public async Task GetFileContentAsync_DownloadRequestFails_ReturnsNull()
		{
			var fileId = Guid.NewGuid();
			_mockHandler.When($"*/api/files/{fileId}/metadata")
						.Respond("application/json", $"{{\"id\":\"{fileId}\",\"originalFileName\":\"test.txt\",\"contentType\":\"text/plain\",\"fileSize\":10,\"uploadedAt\":\"2025-05-29T00:00:00Z\",\"hash\":\"abc\"}}");
			_mockHandler.When($"*/api/files/{fileId}/download")
						.Respond(System.Net.HttpStatusCode.NotFound);

			var client = new FileStoringServiceClient(_httpClient, _configuration);

			var result = await client.GetFileContentAsync(fileId);

			Assert.Null(result);
		}

		[Fact]
		public async Task GetFileContentAsync_EmptyContent_ReturnsNull()
		{
			var fileId = Guid.NewGuid();
			_mockHandler.When($"*/api/files/{fileId}/metadata")
						.Respond("application/json", $"{{\"id\":\"{fileId}\",\"originalFileName\":\"test.txt\",\"contentType\":\"text/plain\",\"fileSize\":0,\"uploadedAt\":\"2025-05-29T00:00:00Z\",\"hash\":\"abc\"}}");
			_mockHandler.When($"*/api/files/{fileId}/download")
						.Respond("text/plain", "");

			var client = new FileStoringServiceClient(_httpClient, _configuration);

			var result = await client.GetFileContentAsync(fileId);

			Assert.Null(result);
		}
	}
}