using Microsoft.EntityFrameworkCore;
using FileAnalysisService.Database;
using FileAnalysisService.Models;

namespace FileAnalysisService.Services
{
    public class FilePlagiarismService : IFilesPlagiarismService
    {
        private readonly FileAnalysisDBContext _dbContext;

        public FilePlagiarismService(FileAnalysisDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PlagiarismModel> CompareTwoFilesAsync(Guid fileId, Guid otherFileId, string fileContent, string otherFContent)
        {
            // Check if a comparison between these two files already exists
            var existingEntry = await _dbContext.PlagiarismResults
                .FirstOrDefaultAsync(r => r.FileId == fileId && r.OtherFileId == otherFileId);

            if (existingEntry != null)
            {
                return existingEntry;
            }

            // Calculate similarity score based on text overlap
            var overlapScore = EvaluateTextOverlap(fileContent, otherFContent);

            var entry = new PlagiarismModel
            {
                Id = Guid.NewGuid(),
                FileId = fileId,
                OtherFileId = otherFileId,
                SimilarityPercentage = overlapScore,
                ComparisonDate = DateTime.UtcNow
            };

            _dbContext.PlagiarismResults.Add(entry);
            await _dbContext.SaveChangesAsync();

            return entry;
        }

        // Calculates the similarity percentage between two texts using normalized edit distance
        private double EvaluateTextOverlap(string textA, string textB)
        {
            int editDist = DetermineTextDistance(textA, textB);
            double longestLength = Math.Max(textA.Length, textB.Length);
            return longestLength == 0 ? 100.0 : (1.0 - editDist / longestLength) * 100.0;
        }

        // Computes the Levenshtein distance between two strings
        private int DetermineTextDistance(string strA, string strB)
        {
            int lenA = strA.Length;
            int lenB = strB.Length;

            if (lenA == 0) return lenB;
            if (lenB == 0) return lenA;

            var previousRow = new int[lenB + 1];
            for (int j = 0; j <= lenB; j++)
            {
                previousRow[j] = j;
            }

            for (int i = 1; i <= lenA; i++)
            {
                var currentRow = new int[lenB + 1];
                currentRow[0] = i;

                for (int j = 1; j <= lenB; j++)
                {
                    int cost = strA[i - 1] == strB[j - 1] ? 0 : 1;
                    currentRow[j] = Math.Min(
                        Math.Min(currentRow[j - 1] + 1, previousRow[j] + 1),
                        previousRow[j - 1] + cost
                    );
                }

                previousRow = currentRow;
            }

            return previousRow[lenB];
        }
    }
}
