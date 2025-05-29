using Microsoft.EntityFrameworkCore;
using Moq;
using FileAnalysisService.Database;
using FileAnalysisService.Models;
using FileAnalysisService.Services;
using Xunit;

namespace FileAnalysisService.Tests.Services
{
	public class FileAnalysisServiceTests
	{
		private readonly FileAnalysisDBContext _dbContext;
		private readonly Mock<IFileWordCloudService> _mockWordCloudService;

		public FileAnalysisServiceTests()
		{
			var options = new DbContextOptionsBuilder<FileAnalysisDBContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			_dbContext = new FileAnalysisDBContext(options);

			_mockWordCloudService = new Mock<IFileWordCloudService>();
		}

		[Fact]
		public async Task AnalyzeFileAsync_ValidContent_SavesAnalysis()
		{
			var fileId = Guid.NewGuid();
			var content = "Paragraph 1\n\nParagraph 2";
			_mockWordCloudService.Setup(w => w.CreateWordCloudAsync(content))
								 .ReturnsAsync("http://cloud.url");

			var service = new TextAnalysisService(_dbContext, _mockWordCloudService.Object);

			var result = await service.AnalyzeFileAsync(fileId, content);

			Assert.NotNull(result);
			Assert.Equal(fileId, result.FileId);
			Assert.Equal(2, result.ParagraphCount);
			Assert.Equal(4, result.WordCount);
			Assert.Equal("http://cloud.url", result.WordCloudImageUrl);
		}

		[Fact]
		public async Task AnalyzeFileAsync_EmptyContent_SavesZeroValues()
		{
			var fileId = Guid.NewGuid();
			_mockWordCloudService.Setup(w => w.CreateWordCloudAsync(""))
								 .ReturnsAsync("http://cloud.url");

			var service = new TextAnalysisService(_dbContext, _mockWordCloudService.Object);

			var result = await service.AnalyzeFileAsync(fileId, "");

			Assert.NotNull(result);
			Assert.Equal(0, result.ParagraphCount);
			Assert.Equal(0, result.WordCount);
			Assert.Equal(0, result.CharCount);
		}

		[Fact]
		public async Task GetAnalysisResultAsync_ExistingResult_ReturnsResult()
		{
			var fileId = Guid.NewGuid();
			var storedResult = new FileAnalysisResultModel
			{
				Id = Guid.NewGuid(),
				FileId = fileId,
				ParagraphCount = 2,
				WordCount = 5,
				CharCount = 10,
				WordCloudImageUrl = "http://cloud.url",
				AnalysisDate = DateTime.UtcNow
			};
			_dbContext.AnalysisResults.Add(storedResult);
			await _dbContext.SaveChangesAsync();

			var service = new TextAnalysisService(_dbContext, _mockWordCloudService.Object);

			var result = await service.GetAnalysisResultAsync(fileId);

			Assert.NotNull(result);
			Assert.Equal(storedResult.Id, result.Id);
			Assert.Equal(2, result.ParagraphCount);
		}

		[Fact]
		public async Task GetAnalysisResultAsync_NonExistingResult_ReturnsNull()
		{
			var fileId = Guid.NewGuid();
			var service = new TextAnalysisService(_dbContext, _mockWordCloudService.Object);

			var result = await service.GetAnalysisResultAsync(fileId);

			Assert.Null(result);
		}
	}
}