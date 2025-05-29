using System.Net.Http.Json;
using Moq;
using RichardSzalay.MockHttp;
using FileAnalysisService.Clients;
using FileAnalysisService.Services;
using Xunit;

namespace FileAnalysisService.Tests.Services
{
	public class FileWordCloudServiceTests
	{
		private readonly MockHttpMessageHandler _mockHandler;
		private readonly HttpClient _httpClient;

		public FileWordCloudServiceTests()
		{
			_mockHandler = new MockHttpMessageHandler();
			_httpClient = new HttpClient(_mockHandler);
			_httpClient.BaseAddress = new Uri("https://quickchart.io/");
		}

		[Fact]
		public async Task CreateWordCloudAsync_ValidText_ReturnsUrl()
		{
			var text = "привет мир";
			_mockHandler.When("*")
						.Respond("application/json", "{\"url\":\"http://cloud.url\"}");

			var service = new FileWordCloudService(_httpClient);

			var result = await service.CreateWordCloudAsync(text);

			Assert.NotNull(result);
			Assert.Equal("http://cloud.url", result);
		}

		[Fact]
		public async Task CreateWordCloudAsync_EmptyText_ReturnsNull()
		{
			_mockHandler.When("*")
						.Respond("application/json", "{\"url\":\"http://cloud.url\"}");

			var service = new FileWordCloudService(_httpClient);

			var result = await service.CreateWordCloudAsync("");

			Assert.NotNull(result);
			Assert.Equal("http://cloud.url", result);
		}

		[Fact]
		public async Task CreateWordCloudAsync_ApiFails_ReturnsNull()
		{
			_mockHandler.When("*")
						.Respond(System.Net.HttpStatusCode.InternalServerError);

			var service = new FileWordCloudService(_httpClient);

			var result = await service.CreateWordCloudAsync("привет мир");

			Assert.Null(result);
		}
	}
}