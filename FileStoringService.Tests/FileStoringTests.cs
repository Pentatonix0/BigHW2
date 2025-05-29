using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;
using FileStoringService.Database;
using FileStoringService.Models;
using FileStoringService.Services;

namespace FileStoringService.Tests
{
	public class FileStorageServiceTests : IDisposable
	{
		private readonly FileDBContext _dbContext;
		private readonly Mock<IWebHostEnvironment> _envMock;
		private readonly FileStorageService _service;
		private readonly string _testStoragePath;

		public FileStorageServiceTests()
		{
			// Setup in-memory database
			var options = new DbContextOptionsBuilder<FileDBContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			_dbContext = new FileDBContext(options);

			// Setup IWebHostEnvironment mock
			_envMock = new Mock<IWebHostEnvironment>();
			_testStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles");
			Directory.CreateDirectory(_testStoragePath);
			_envMock.Setup(e => e.ContentRootPath).Returns(Directory.GetCurrentDirectory());

			// Initialize service
			_service = new FileStorageService(_dbContext, _envMock.Object);
		}

		public void Dispose()
		{
			// Cleanup database
			_dbContext.Database.EnsureDeleted();
			_dbContext.Dispose();

			// Cleanup test files
			if (Directory.Exists(_testStoragePath))
			{
				Directory.Delete(_testStoragePath, true);
			}
		}

		[Fact]
		public async Task SaveFileAsync_ValidTxtFile_SavesFileAndMetadata()
		{
			// Arrange
			var fileContent = "Test content";
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
			var fileName = "test.txt";

			// Act
			var fileId = await _service.SaveFileAsync(stream, fileName);

			// Assert
			var metadata = await _dbContext.Files.FirstOrDefaultAsync(f => f.Id == Guid.Parse(fileId));
			Assert.NotNull(metadata);
			Assert.Equal(fileName, metadata.FileName);
			Assert.Equal(fileContent.Length, metadata.Size);
			Assert.NotEmpty(metadata.Hash);
			Assert.True(File.Exists(metadata.Path));

			var savedContent = await File.ReadAllTextAsync(metadata.Path);
			Assert.Equal(fileContent, savedContent);
		}

		[Fact]
		public async Task SaveFileAsync_EmptyStream_ThrowsArgumentException()
		{
			// Arrange
			using var stream = new MemoryStream();
			var fileName = "test.txt";

			// Act & Assert
			await Assert.ThrowsAsync<ArgumentException>(() => _service.SaveFileAsync(stream, fileName));
		}

		[Fact]
		public async Task SaveFileAsync_NonTxtFile_ThrowsArgumentException()
		{
			// Arrange
			using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
			var fileName = "test.pdf";

			// Act & Assert
			await Assert.ThrowsAsync<ArgumentException>(() => _service.SaveFileAsync(stream, fileName));
		}

		[Fact]
		public async Task GetFileAsync_ExistingFile_ReturnsStream()
		{
			// Arrange
			var fileContent = "Test content";
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
			var fileName = "test.txt";
			var fileId = await _service.SaveFileAsync(stream, fileName);

			// Act
			using var resultStream = await _service.GetFileAsync(fileName);
			using var reader = new StreamReader(resultStream);
			var content = await reader.ReadToEndAsync();

			// Assert
			Assert.Equal(fileContent, content);
		}

		[Fact]
		public async Task GetFileAsync_NonExistingFile_ThrowsFileNotFoundException()
		{
			// Arrange
			var fileName = "nonexistent.txt";

			// Act & Assert
			await Assert.ThrowsAsync<FileNotFoundException>(() => _service.GetFileAsync(fileName));
		}

		[Fact]
		public async Task DeleteFileAsync_ExistingFile_DeletesFileAndMetadata()
		{
			// Arrange
			using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
			var fileName = "test.txt";
			var fileId = await _service.SaveFileAsync(stream, fileName);
			var metadata = await _dbContext.Files.FirstOrDefaultAsync(f => f.Id == Guid.Parse(fileId));
			var filePath = metadata.Path;

			// Act
			var result = await _service.DeleteFileAsync(fileName);

			// Assert
			Assert.True(result);
			Assert.False(File.Exists(filePath));
			Assert.Null(await _dbContext.Files.FirstOrDefaultAsync(f => f.FileName == fileName));
		}

		[Fact]
		public async Task DeleteFileAsync_NonExistingFile_ReturnsFalse()
		{
			// Arrange
			var fileName = "nonexistent.txt";

			// Act
			var result = await _service.DeleteFileAsync(fileName);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public async Task GetFilePath_ExistingFile_ReturnsPath()
		{
			// Arrange
			using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
			var fileName = "test.txt";
			var fileId = await _service.SaveFileAsync(stream, fileName);
			var metadata = await _dbContext.Files.FirstOrDefaultAsync(f => f.Id == Guid.Parse(fileId));

			// Act
			var path = _service.GetFilePath(fileName);

			// Assert
			Assert.Equal(metadata.Path, path);
			Assert.True(File.Exists(path));
		}

		[Fact]
		public void GetFilePath_NonExistingFile_ThrowsFileNotFoundException()
		{
			// Arrange
			var fileName = "nonexistent.txt";

			// Act & Assert
			Assert.Throws<FileNotFoundException>(() => _service.GetFilePath(fileName));
		}

		[Fact]
		public async Task GetFileInfoAsync_ExistingFile_ReturnsFileInfo()
		{
			// Arrange
			using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
			var fileName = "test.txt";
			var fileId = await _service.SaveFileAsync(stream, fileName);
			var guid = Guid.Parse(fileId);

			// Act
			var fileInfo = await _service.GetFileInfoAsync(guid);

			// Assert
			Assert.NotNull(fileInfo);
			Assert.True(fileInfo.Exists);
			Assert.Equal(3, fileInfo.Length);
		}

		[Fact]
		public async Task GetFileInfoAsync_NonExistingFile_ReturnsNull()
		{
			// Arrange
			var guid = Guid.NewGuid();

			// Act
			var fileInfo = await _service.GetFileInfoAsync(guid);

			// Assert
			Assert.Null(fileInfo);
		}

		[Fact]
		public async Task GetFileContentAsync_ExistingFile_ReturnsContent()
		{
			// Arrange
			var fileContent = new byte[] { 1, 2, 3 };
			using var stream = new MemoryStream(fileContent);
			var fileName = "test.txt";
			var fileId = await _service.SaveFileAsync(stream, fileName);
			var guid = Guid.Parse(fileId);

			// Act
			var content = await _service.GetFileContentAsync(guid);

			// Assert
			Assert.NotNull(content);
			Assert.Equal(fileContent, content);
		}

		[Fact]
		public async Task GetFileContentAsync_NonExistingFile_ReturnsNull()
		{
			// Arrange
			var guid = Guid.NewGuid();

			// Act
			var content = await _service.GetFileContentAsync(guid);

			// Assert
			Assert.Null(content);
		}

		[Fact]
		public async Task FileExistsAsync_ExistingHash_ReturnsTrue()
		{
			// Arrange
			using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
			var fileName = "test.txt";
			var fileId = await _service.SaveFileAsync(stream, fileName);
			var metadata = await _dbContext.Files.FirstOrDefaultAsync(f => f.Id == Guid.Parse(fileId));
			var hash = metadata.Hash;

			// Act
			var exists = await _service.FileExistsAsync(hash);

			// Assert
			Assert.True(exists);
		}

		[Fact]
		public async Task FileExistsAsync_NonExistingHash_ReturnsFalse()
		{
			// Arrange
			var hash = "nonexistenthash";

			// Act
			var exists = await _service.FileExistsAsync(hash);

			// Assert
			Assert.False(exists);
		}
	}
}