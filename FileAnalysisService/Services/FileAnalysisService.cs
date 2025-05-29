using FileAnalysisService.Database;
using FileAnalysisService.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace FileAnalysisService.Services
{
    public class TextAnalysisService : IFileAnalysisService
    {
        private readonly FileAnalysisDBContext _dbContext;
        private readonly IFileWordCloudService _wordCloudService;

        public TextAnalysisService(FileAnalysisDBContext dbContext, IFileWordCloudService wordCloudService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _wordCloudService = wordCloudService ?? throw new ArgumentNullException(nameof(wordCloudService));
        }

        // Analyze file content: count paragraphs, words, characters, and generate word cloud
        public async Task<FileAnalysisResultModel> AnalyzeFileAsync(Guid fileId, string content)
        {
            // Avoid re-analysis if a result already exists
            var existingResult = await GetAnalysisResultAsync(fileId);
            if (existingResult != null)
                return existingResult;

            var paragraphCount = CountParagraphs(content);
            var wordCount = CountWords(content);
            var charCount = content.Length;

            string? wordCloudPath = null;
            try
            {
                var absolutePath = await _wordCloudService.CreateWordCloudAsync(content);
                if (!string.IsNullOrEmpty(absolutePath))
                {
                    // Convert absolute file path to a relative URL for client access
                    var fileName = Path.GetFileName(absolutePath);
                    wordCloudPath = $"/wordclouds/{fileName}";
                }
            }
            catch (Exception)
            {
                // Word cloud generation failure is non-critical
            }

            var result = new FileAnalysisResultModel
            {
                Id = Guid.NewGuid(),
                FileId = fileId,
                ParagraphCount = paragraphCount,
                WordCount = wordCount,
                CharCount = charCount,
                WordCloudImagePath = wordCloudPath,
                AnalysisDate = DateTime.UtcNow
            };

            _dbContext.AnalysisResults.Add(result);
            await _dbContext.SaveChangesAsync();

            return result;
        }

        // Retrieve existing analysis result by file ID
        public async Task<FileAnalysisResultModel> GetAnalysisResultAsync(Guid fileId)
        {
            return await _dbContext.AnalysisResults
                .FirstOrDefaultAsync(r => r.FileId == fileId);
        }

        // Count paragraphs based on double newlines
        private int CountParagraphs(string text)
        {
            var paragraphs = Regex.Split(text, @"(\r\n|\n){2,}");
            return paragraphs.Count(p => !string.IsNullOrWhiteSpace(p));
        }

        // Count words using regex for word boundaries
        private int CountWords(string text)
        {
            var words = Regex.Matches(text, @"\b[\w\d]+\b");
            return words.Count;
        }
    }
}
