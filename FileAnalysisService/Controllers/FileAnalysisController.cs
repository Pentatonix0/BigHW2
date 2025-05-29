using Microsoft.AspNetCore.Mvc;
using FileAnalysisService.Services;
using FileAnalysisService.DTO;
using FileAnalysisService.Clients;

[ApiController]
[Route("api/analysis")]
public class AnalysisController : ControllerBase
{
    private readonly IFileAnalysisService _analysisService;
    private readonly IFilesPlagiarismService _similarityService;
    private readonly IFileWordCloudService _wordCloudService;
    private readonly IFileStoringServiceClient _fileClient;

    public AnalysisController(
        IFileAnalysisService analysisService,
        IFilesPlagiarismService similarityService,
        IFileWordCloudService wordCloudService,
        IFileStoringServiceClient fileClient)
    {
        _analysisService = analysisService;
        _similarityService = similarityService;
        _wordCloudService = wordCloudService;
        _fileClient = fileClient;
    }

    // Analyze a file and return statistical and visual data
    [HttpPost("{fileId}")]
    public async Task<IActionResult> AnalyzeFile(Guid fileId)
    {
        try
        {
            var fileData = await _fileClient.GetFileContentAsync(fileId);
            if (fileData == null)
            {
                return NotFound(new { message = $"File with ID {fileId} not found." });
            }

            var analysisResult = await _analysisService.AnalyzeFileAsync(fileId, fileData.Content);

            var response = new FileAnalysisResponceDto
            {
                AnalysisId = analysisResult.Id,
                FileId = analysisResult.FileId,
                ParagraphCount = analysisResult.ParagraphCount,
                WordCount = analysisResult.WordCount,
                CharCount = analysisResult.CharCount,
                WordCloudImagePath = await _wordCloudService.CreateWordCloudAsync(fileData.Content)
            };

            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An unexpected error occurred while analyzing the file." });
        }
    }

    // Retrieve previously saved analysis result for a given file
    [HttpGet("{fileId}")]
    public async Task<IActionResult> GetAnalysisResult(Guid fileId)
    {
        try
        {
            var analysisResult = await _analysisService.GetAnalysisResultAsync(fileId);
            if (analysisResult == null)
            {
                return NotFound(new { message = $"Analysis result for file ID {fileId} not found." });
            }

            var response = new FileAnalysisResponceDto
            {
                AnalysisId = analysisResult.Id,
                FileId = analysisResult.FileId,
                ParagraphCount = analysisResult.ParagraphCount,
                WordCount = analysisResult.WordCount,
                CharCount = analysisResult.CharCount,
                WordCloudImagePath = analysisResult.WordCloudImagePath
            };

            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An unexpected error occurred while retrieving the analysis result." });
        }
    }

    // Compare two files and return a similarity score
    [HttpPost("compare")]
    public async Task<IActionResult> CompareFiles([FromBody] FilePlagiarismRequestDTO request)
    {
        try
        {
            var fileData = await _fileClient.GetFileContentAsync(request.FileId);
            if (fileData == null)
            {
                return NotFound(new { message = $"File with ID {request.FileId} not found." });
            }

            var otherFileData = await _fileClient.GetFileContentAsync(request.OtherFileId);
            if (otherFileData == null)
            {
                return NotFound(new { message = $"File with ID {request.OtherFileId} not found." });
            }

            var plagiarismResult = await _similarityService.CompareTwoFilesAsync(
                request.FileId,
                request.OtherFileId,
                fileData.Content,
                otherFileData.Content
            );

            var response = new FilePlagiarismResponceDto
            {
                FileId = plagiarismResult.FileId,
                OtherFileId = plagiarismResult.OtherFileId,
                SimilarityPercentage = plagiarismResult.SimilarityPercentage
            };

            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An unexpected error occurred while comparing the files." });
        }
    }

    // Download the generated word cloud image for a file
    [HttpGet("download_word_cloud/{fileId}")]
    public async Task<IActionResult> DownloadWordCloud(Guid fileId)
    {
        try
        {
            var analysisResult = await _analysisService.GetAnalysisResultAsync(fileId);
            if (analysisResult == null)
            {
                return NotFound(new { message = $"Analysis result for file ID {fileId} not found." });
            }

            if (string.IsNullOrEmpty(analysisResult.WordCloudImagePath))
            {
                return NotFound(new { message = $"No word cloud image available for file ID {fileId}." });
            }

            // Convert virtual path to physical file system path
            var filePath = analysisResult.WordCloudImagePath.Replace("/wordclouds/", "/tmp/wordclouds/");
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { message = $"Word cloud file not found at {filePath}." });
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileName(filePath);

            return File(fileBytes, "image/png", fileName);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An unexpected error occurred while downloading the word cloud." });
        }
    }
}
