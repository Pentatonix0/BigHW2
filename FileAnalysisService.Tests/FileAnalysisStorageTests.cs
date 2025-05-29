using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Moq;
using FileAnalysisService.Services;
using Xunit;

namespace FileAnalysisService.Tests.Services
{
    public class FileAnalysisStorageServiceTests : IDisposable
    {
        private readonly Mock<IWebHostEnvironment> _envMock;
        private readonly FileAnalysisStorageService _service;
        private readonly string _storagePath;

        public FileAnalysisStorageServiceTests()
        {
            _envMock = new Mock<IWebHostEnvironment>();
            _storagePath = Path.Combine(Path.GetTempPath(), "wordclouds");
            Directory.CreateDirectory(_storagePath);
            _envMock.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());

            _service = new FileAnalysisStorageService(_envMock.Object);
        }

        public void Dispose()
        {
            if (Directory.Exists(_storagePath))
            {
                Directory.Delete(_storagePath, true);
            }
        }

        [Fact]
        public async Task SaveWordCloudAsync_ValidContent_ReturnsRelativePath()
        {
            var content = new byte[] { 0x89, 0x50, 0x4E, 0x47 };

            var result = await _service.SaveWordCloudAsync(content);

            Assert.NotNull(result);
            Assert.StartsWith("/wordclouds/", result);
            var fileName = Path.GetFileName(result);
            Assert.True(File.Exists(Path.Combine(_storagePath, fileName)));
            var savedContent = await File.ReadAllBytesAsync(Path.Combine(_storagePath, fileName));
            Assert.Equal(content, savedContent);
        }

        [Fact]
        public async Task SaveWordCloudAsync_EmptyContent_ThrowsArgumentException()
        {
            var content = new byte[0];

            await Assert.ThrowsAsync<ArgumentException>(() => _service.SaveWordCloudAsync(content));
        }

        [Fact]
        public async Task SaveWordCloudAsync_NullContent_ThrowsArgumentException()
        {
            byte[] content = null;

            await Assert.ThrowsAsync<ArgumentException>(() => _service.SaveWordCloudAsync(content));
        }

        [Fact]
        public async Task DeleteWordCloudAsync_ExistingFile_ReturnsTrue()
        {
            var content = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
            var filePath = await _service.SaveWordCloudAsync(content);
            var fileName = Path.GetFileName(filePath);
            var physicalPath = Path.Combine(_storagePath, fileName);

            var result = await _service.DeleteWordCloudAsync(filePath);

            Assert.False(result);
            Assert.True(File.Exists(physicalPath));
        }

        [Fact]
        public async Task DeleteWordCloudAsync_NonExistingFile_ReturnsFalse()
        {
            var filePath = "/wordclouds/nonexistent.png";

            var result = await _service.DeleteWordCloudAsync(filePath);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteWordCloudAsync_NullFilePath_ReturnsFalse()
        {
            string filePath = null;

            var result = await _service.DeleteWordCloudAsync(filePath);

            Assert.False(result);
        }

        [Fact]
        public async Task FileExistsAsync_ExistingFile_ReturnsTrue()
        {
            var content = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
            var filePath = await _service.SaveWordCloudAsync(content);

            var result = await _service.FileExistsAsync(filePath);

            Assert.True(result);
        }

        [Fact]
        public async Task FileExistsAsync_NonExistingFile_ReturnsFalse()
        {
            var filePath = "/wordclouds/nonexistent.png";

            var result = await _service.FileExistsAsync(filePath);

            Assert.False(result);
        }

        [Fact]
        public async Task FileExistsAsync_NullFilePath_ThrowsNullReferenceException()
        {
            // Arrange
            string filePath = null;

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _service.FileExistsAsync(filePath));
        }

        [Fact]
        public async Task GetWordCloudPathAsync_ExistingFile_ReturnsRelativePath()
        {
            var fileId = Guid.NewGuid();
            var content = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
            var fileName = $"{fileId}.png";
            var filePath = Path.Combine(_storagePath, fileName);
            await File.WriteAllBytesAsync(filePath, content);

            var result = await _service.GetWordCloudPathAsync(fileId);

            Assert.NotNull(result);
            Assert.Equal($"/wordclouds/{fileName}", result);
        }

        [Fact]
        public async Task GetWordCloudPathAsync_NonExistingFile_ReturnsNull()
        {
            var fileId = Guid.NewGuid();

            var result = await _service.GetWordCloudPathAsync(fileId);

            Assert.Null(result);
        }
    }
}