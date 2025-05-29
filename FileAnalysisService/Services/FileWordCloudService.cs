using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using FileAnalysisService.Models;

namespace FileAnalysisService.Services
{
    public class FileWordCloudService : IFileWordCloudService
    {
        private readonly HttpClient _httpClient;
        private readonly IFileAnalysisStorageService _storageService;

        public FileWordCloudService(HttpClient httpClient, IFileAnalysisStorageService storageService)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        }

        // Generates a word cloud image from the given text and returns the relative URL to the saved image
        public async Task<string> CreateWordCloudAsync(string text)
        {
            try
            {
                // Filter out common words to improve the quality of the word cloud
                var filteredWords = FilterCommonWords(text);

                // Prepare the payload for the external word cloud service API
                var requestData = new
                {
                    format = "png",
                    width = 1920,
                    height = 1080,
                    fontScale = 15,
                    text = filteredWords
                };

                // Send POST request to the QuickChart API for generating word cloud
                var response = await _httpClient.PostAsJsonAsync("https://quickchart.io/wordcloud", requestData);

                if (!response.IsSuccessStatusCode)
                    return null;

                // Read the response content as a byte array (image binary)
                var contentBytes = await response.Content.ReadAsByteArrayAsync();

                // Save the image bytes to storage and get the relative file path
                var filePath = await _storageService.SaveWordCloudAsync(contentBytes);

                // Return the URL path to the saved word cloud image
                return filePath;
            }
            catch
            {
                // On any failure, return null (could be extended with logging)
                return null;
            }
        }

        // Retrieves the saved word cloud image URL by file ID if it exists
        public async Task<string> GetWordCloudAsync(Guid fileId)
        {
            try
            {
                // Get the stored path for the word cloud image
                var analysisResult = await _storageService.GetWordCloudPathAsync(fileId);

                // Return null if path is missing or file does not exist
                if (string.IsNullOrEmpty(analysisResult) || !await _storageService.FileExistsAsync(analysisResult))
                {
                    return null;
                }
                return analysisResult;
            }
            catch
            {
                return null;
            }
        }

        // Filters out common, unimportant words and short words from the text
        private string FilterCommonWords(string text)
        {
            var commonWords = new HashSet<string> { "к", "под", "через", "для", "на", "у", "с", "а", "по", "в", "за", "от", "над", "о", "при", "и", "что", "из" };

            var words = Regex.Matches(text.ToLower(), @"\b[\w\d]+\b")
                .Cast<Match>()
                .Select(m => m.Value)
                .Where(word => !commonWords.Contains(word) && word.Length > 2)
                .ToList();

            // Join filtered words with space to prepare for word cloud generation
            return string.Join(" ", words);
        }

        // This class could be used if API returned a JSON response with path (not used here)
        private class WordCloudResponse
        {
            public string Path { get; set; }
        }
    }
}
