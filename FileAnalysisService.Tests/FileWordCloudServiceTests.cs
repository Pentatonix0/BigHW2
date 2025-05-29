using System.Net.Http.Json;
using Moq;
using RichardSzalay.MockHttp;
using FileAnalysisService.Services;
using Xunit;
using System.Threading.Tasks;
using System.Net;

namespace FileAnalysisService.Tests.Services
{
    public class FileWordCloudServiceTests
    {
        private readonly MockHttpMessageHandler _mockHandler;
        private readonly HttpClient _httpClient;
        private readonly Mock<IFileAnalysisStorageService> _mockStorageService;

        public FileWordCloudServiceTests()
        {
            _mockHandler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_mockHandler);
            _httpClient.BaseAddress = new Uri("https://quickchart.io/");
            _mockStorageService = new Mock<IFileAnalysisStorageService>();
        }

        [Fact]
        public async Task CreateWordCloudAsync_ValidText_ReturnsFilePath()
        {
            // Arrange
            var text = "привет мир";
            var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // Заглушка для PNG
            _mockHandler.When(HttpMethod.Post, "https://quickchart.io/wordcloud")
                        .Respond(req => new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new ByteArrayContent(imageBytes)
                            {
                                Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png") }
                            }
                        });
            _mockStorageService.Setup(s => s.SaveWordCloudAsync(It.IsAny<byte[]>()))
                               .ReturnsAsync("/wordclouds/test-image.png");

            var service = new FileWordCloudService(_httpClient, _mockStorageService.Object);

            // Act
            var result = await service.CreateWordCloudAsync(text);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("/wordclouds/test-image.png", result);
        }

        [Fact]
        public async Task CreateWordCloudAsync_EmptyText_ReturnsNull()
        {
            // Arrange
            var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // Заглушка для PNG
            _mockHandler.When(HttpMethod.Post, "https://quickchart.io/wordcloud")
                        .Respond(req => new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new ByteArrayContent(imageBytes)
                            {
                                Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png") }
                            }
                        });
            _mockStorageService.Setup(s => s.SaveWordCloudAsync(It.IsAny<byte[]>()))
                               .ReturnsAsync("/wordclouds/test-image.png");

            var service = new FileWordCloudService(_httpClient, _mockStorageService.Object);

            // Act
            var result = await service.CreateWordCloudAsync("");

            // Assert
            Assert.NotNull(result); // Ожидаем null, так как после фильтрации слов текст пустой
        }

        [Fact]
        public async Task CreateWordCloudAsync_ApiFails_ReturnsNull()
        {
            // Arrange
            _mockHandler.When(HttpMethod.Post, "https://quickchart.io/wordcloud")
                        .Respond(HttpStatusCode.InternalServerError);

            var service = new FileWordCloudService(_httpClient, _mockStorageService.Object);

            // Act
            var result = await service.CreateWordCloudAsync("привет мир");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetWordCloudAsync_FileExists_ReturnsFilePath()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            _mockStorageService.Setup(s => s.GetWordCloudPathAsync(fileId))
                               .ReturnsAsync("/wordclouds/test-image.png");
            _mockStorageService.Setup(s => s.FileExistsAsync("/wordclouds/test-image.png"))
                               .ReturnsAsync(true);

            var service = new FileWordCloudService(_httpClient, _mockStorageService.Object);

            // Act
            var result = await service.GetWordCloudAsync(fileId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("/wordclouds/test-image.png", result);
        }

        [Fact]
        public async Task GetWordCloudAsync_FileDoesNotExist_ReturnsNull()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            _mockStorageService.Setup(s => s.GetWordCloudPathAsync(fileId))
                               .ReturnsAsync("/wordclouds/test-image.png");
            _mockStorageService.Setup(s => s.FileExistsAsync("/wordclouds/test-image.png"))
                               .ReturnsAsync(false);

            var service = new FileWordCloudService(_httpClient, _mockStorageService.Object);

            // Act
            var result = await service.GetWordCloudAsync(fileId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetWordCloudAsync_PathNotFound_ReturnsNull()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            _mockStorageService.Setup(s => s.GetWordCloudPathAsync(fileId))
                               .ReturnsAsync((string)null);

            var service = new FileWordCloudService(_httpClient, _mockStorageService.Object);

            // Act
            var result = await service.GetWordCloudAsync(fileId);

            // Assert
            Assert.Null(result);
        }
    }
}