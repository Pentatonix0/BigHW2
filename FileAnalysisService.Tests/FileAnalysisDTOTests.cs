using System;
using FileAnalysisService.DTO;
using Xunit;

namespace FileAnalysisService.Tests.DTO
{
    public class DtoTests
    {
        [Fact]
        public void FileDto_Properties_SetAndGetCorrectly()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            var content = "Test content";
            var fileName = "test.txt";
            var fileDto = new FileDto
            {
                Id = fileId,
                Content = content,
                FileName = fileName
            };

            // Act & Assert
            Assert.Equal(fileId, fileDto.Id);
            Assert.Equal(content, fileDto.Content);
            Assert.Equal(fileName, fileDto.FileName);
        }

        [Fact]
        public void FileDto_ContentAndFileName_CanBeNull()
        {
            // Arrange
            var fileDto = new FileDto
            {
                Id = Guid.NewGuid(),
                Content = null,
                FileName = null
            };

            // Act & Assert
            Assert.Null(fileDto.Content);
            Assert.Null(fileDto.FileName);
        }

        [Fact]
        public void FileAnalysisResponseDto_Properties_SetAndGetCorrectly()
        {
            // Arrange
            var analysisId = Guid.NewGuid();
            var fileId = Guid.NewGuid();
            var paragraphCount = 5;
            var wordCount = 100;
            var charCount = 500;
            var wordCloudImagePath = "/wordclouds/test.png";
            var responseDto = new FileAnalysisResponceDto
            {
                AnalysisId = analysisId,
                FileId = fileId,
                ParagraphCount = paragraphCount,
                WordCount = wordCount,
                CharCount = charCount,
                WordCloudImagePath = wordCloudImagePath
            };

            // Act & Assert
            Assert.Equal(analysisId, responseDto.AnalysisId);
            Assert.Equal(fileId, responseDto.FileId);
            Assert.Equal(paragraphCount, responseDto.ParagraphCount);
            Assert.Equal(wordCount, responseDto.WordCount);
            Assert.Equal(charCount, responseDto.CharCount);
            Assert.Equal(wordCloudImagePath, responseDto.WordCloudImagePath);
        }

        [Fact]
        public void FileAnalysisResponseDto_WordCloudImagePath_CanBeNull()
        {
            // Arrange
            var responseDto = new FileAnalysisResponceDto
            {
                AnalysisId = Guid.NewGuid(),
                FileId = Guid.NewGuid(),
                ParagraphCount = 0,
                WordCount = 0,
                CharCount = 0,
                WordCloudImagePath = null
            };

            // Act & Assert
            Assert.Null(responseDto.WordCloudImagePath);
        }

        [Fact]
        public void FilePlagiarismRequestDTO_Properties_SetAndGetCorrectly()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            var otherFileId = Guid.NewGuid();
            var requestDto = new FilePlagiarismRequestDTO
            {
                FileId = fileId,
                OtherFileId = otherFileId
            };

            // Act & Assert
            Assert.Equal(fileId, requestDto.FileId);
            Assert.Equal(otherFileId, requestDto.OtherFileId);
        }

        [Fact]
        public void FilePlagiarismResponseDto_Properties_SetAndGetCorrectly()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            var otherFileId = Guid.NewGuid();
            var similarityPercentage = 75.5;
            var responseDto = new FilePlagiarismResponceDto
            {
                FileId = fileId,
                OtherFileId = otherFileId,
                SimilarityPercentage = similarityPercentage
            };

            // Act & Assert
            Assert.Equal(fileId, responseDto.FileId);
            Assert.Equal(otherFileId, responseDto.OtherFileId);
            Assert.Equal(similarityPercentage, responseDto.SimilarityPercentage);
        }

        [Fact]
        public void FilePlagiarismResponseDto_SimilarityPercentage_CanBeZero()
        {
            // Arrange
            var responseDto = new FilePlagiarismResponceDto
            {
                FileId = Guid.NewGuid(),
                OtherFileId = Guid.NewGuid(),
                SimilarityPercentage = 0.0
            };

            // Act & Assert
            Assert.Equal(0.0, responseDto.SimilarityPercentage);
        }

        [Fact]
        public void FilePlagiarismResponseDto_SimilarityPercentage_CanBeNegative()
        {
            // Arrange
            var responseDto = new FilePlagiarismResponceDto
            {
                FileId = Guid.NewGuid(),
                OtherFileId = Guid.NewGuid(),
                SimilarityPercentage = -10.5
            };

            // Act & Assert
            Assert.Equal(-10.5, responseDto.SimilarityPercentage);
        }
    }
}