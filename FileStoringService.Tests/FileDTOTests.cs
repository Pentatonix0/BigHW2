using System;
using FileStoringService.DTOs;
using Xunit;

namespace FileStoringService.Tests.DTOs
{
    public class FileDtoTests
    {
        [Fact]
        public void FileDto_Properties_SetAndGetCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var fileName = "test.txt";
            var size = 1024L;
            var uploadDate = new DateTime(2025, 5, 29, 11, 0, 0, DateTimeKind.Utc);
            var hash = "abc123";
            var fileDto = new FileDto
            {
                Id = id,
                FileName = fileName,
                Size = size,
                UploadDate = uploadDate,
                Hash = hash
            };

            // Act & Assert
            Assert.Equal(id, fileDto.Id);
            Assert.Equal(fileName, fileDto.FileName);
            Assert.Equal(size, fileDto.Size);
            Assert.Equal(uploadDate, fileDto.UploadDate);
            Assert.Equal(hash, fileDto.Hash);
        }

        [Fact]
        public void FileDto_FileName_CanBeNull()
        {
            // Arrange
            var fileDto = new FileDto
            {
                Id = Guid.NewGuid(),
                FileName = null,
                Size = 0L,
                UploadDate = DateTime.UtcNow,
                Hash = "abc123"
            };

            // Act & Assert
            Assert.Null(fileDto.FileName);
        }

        [Fact]
        public void FileDto_Hash_CanBeNull()
        {
            // Arrange
            var fileDto = new FileDto
            {
                Id = Guid.NewGuid(),
                FileName = "test.txt",
                Size = 0L,
                UploadDate = DateTime.UtcNow,
                Hash = null
            };

            // Act & Assert
            Assert.Null(fileDto.Hash);
        }

        [Fact]
        public void FileDto_Size_CanBeZero()
        {
            // Arrange
            var fileDto = new FileDto
            {
                Id = Guid.NewGuid(),
                FileName = "test.txt",
                Size = 0L,
                UploadDate = DateTime.UtcNow,
                Hash = "abc123"
            };

            // Act & Assert
            Assert.Equal(0L, fileDto.Size);
        }

        [Fact]
        public void FileDto_Size_CanBeNegative_ThrowsNoException()
        {
            // Arrange
            var fileDto = new FileDto
            {
                Id = Guid.NewGuid(),
                FileName = "test.txt",
                Size = -1024L,
                UploadDate = DateTime.UtcNow,
                Hash = "abc123"
            };

            // Act & Assert
            Assert.Equal(-1024L, fileDto.Size); // Проверяем, что отрицательное значение допустимо (хотя в реальности это может быть ошибкой)
        }
    }
}