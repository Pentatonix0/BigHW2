using Microsoft.EntityFrameworkCore;
using FileAnalysisService.Database;
using FileAnalysisService.Models;
using FileAnalysisService.Services;
using Xunit;

namespace FileAnalysisService.Tests.Services
{
	public class FilePlagiarismServiceTests
	{
		private readonly FileAnalysisDBContext _dbContext;

		public FilePlagiarismServiceTests()
		{
			var options = new DbContextOptionsBuilder<FileAnalysisDBContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			_dbContext = new FileAnalysisDBContext(options);
		}

		[Fact]
		public async Task CompareTwoFilesAsync_IdenticalFiles_Returns100Percent()
		{
			var fileId = Guid.NewGuid();
			var otherFileId = Guid.NewGuid();
			var content = "привет мир";
			var service = new FilePlagiarismService(_dbContext);

			var result = await service.CompareTwoFilesAsync(fileId, otherFileId, content, content);

			Assert.NotNull(result);
			Assert.Equal(fileId, result.FileId);
			Assert.Equal(otherFileId, result.OtherFileId);
			Assert.Equal(100.0, result.SimilarityPercentage);
		}

		[Fact]
		public async Task CompareTwoFilesAsync_DifferentFiles_ReturnsLowerSimilarity()
		{
			var fileId = Guid.NewGuid();
			var otherFileId = Guid.NewGuid();
			var content1 = "привет мир";
			var content2 = "пока земля";
			var service = new FilePlagiarismService(_dbContext);

			var result = await service.CompareTwoFilesAsync(fileId, otherFileId, content1, content2);

			Assert.NotNull(result);
			Assert.True(result.SimilarityPercentage < 100.0);
		}

		[Fact]
		public async Task CompareTwoFilesAsync_EmptyContent_Returns100Percent()
		{
			var fileId = Guid.NewGuid();
			var otherFileId = Guid.NewGuid();
			var service = new FilePlagiarismService(_dbContext);

			var result = await service.CompareTwoFilesAsync(fileId, otherFileId, "", "");

			Assert.NotNull(result);
			Assert.Equal(100.0, result.SimilarityPercentage);
		}

		[Fact]
		public async Task CompareTwoFilesAsync_ExistingResult_ReturnsStoredResult()
		{
			var fileId = Guid.NewGuid();
			var otherFileId = Guid.NewGuid();
			var storedResult = new PlagiarismModel
			{
				Id = Guid.NewGuid(),
				FileId = fileId,
				OtherFileId = otherFileId,
				SimilarityPercentage = 75.0,
				ComparisonDate = DateTime.UtcNow
			};
			_dbContext.PlagiarismResults.Add(storedResult);
			await _dbContext.SaveChangesAsync();

			var service = new FilePlagiarismService(_dbContext);

			var result = await service.CompareTwoFilesAsync(fileId, otherFileId, "привет мир", "пока земля");

			Assert.NotNull(result);
			Assert.Equal(storedResult.Id, result.Id);
			Assert.Equal(75.0, result.SimilarityPercentage);
		}
	}
}