using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FileStoringService.Services;
using FileStoringService.Models;

namespace FileStoringService.Controllers
{
    [ApiController]
    [Route("api/files")] // Base route for this controller
    public class FilesController : ControllerBase
    {
        private readonly IFileStoringService _fileService;

        // Inject the file storage service via constructor
        public FilesController(IFileStoringService fileService)
        {
            _fileService = fileService;
        }

        // Endpoint to upload a file via HTTP POST
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                // Check if file was provided and is not empty
                if (file == null || file.Length == 0)
                    return BadRequest("File is empty or not provided.");

                // Restrict uploads to .txt files only
                if (!file.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Only .txt files are allowed.");

                // Open stream to read file content
                using var stream = file.OpenReadStream();
                // Save file and get generated file ID
                var fileId = await _fileService.SaveFileAsync(stream, file.FileName);
                // Return success with file ID
                return Ok(new { FileId = fileId });
            }
            catch (Exception ex)
            {
                // Return 500 if upload failed
                return StatusCode(500, $"Error uploading file: {ex.Message}");
            }
        }

        // Endpoint to download a file by its GUID ID
        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadFile(Guid id)
        {
            try
            {
                // Retrieve file metadata by ID
                var fileModel = await _fileService.GetFileInfoAsync(id);
                if (fileModel == null)
                    return NotFound("File not found.");

                // Get file stream by filename
                var stream = await _fileService.GetFileAsync(fileModel.FileName);
                if (stream == null)
                    return NotFound("File content not found.");

                // Return file stream with appropriate content type and filename
                return File(stream, "text/plain", fileModel.FileName);
            }
            catch (Exception ex)
            {
                // Return 500 if download failed
                return StatusCode(500, $"Error downloading file: {ex.Message}");
            }
        }

        // Endpoint to delete a file by its GUID ID
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteFile(Guid id)
        {
            try
            {
                // Get file metadata
                var fileInfo = await _fileService.GetFileInfoAsync(id);
                if (fileInfo == null)
                    return NotFound("File not found.");

                // Attempt to delete file by filename
                var deleted = await _fileService.DeleteFileAsync(fileInfo.FileName);
                // Return success or failure accordingly
                return deleted ? Ok() : NotFound("File could not be deleted.");
            }
            catch (Exception ex)
            {
                // Return 500 if deletion failed
                return StatusCode(500, $"Error deleting file: {ex.Message}");
            }
        }

        // Endpoint to retrieve metadata of a file by its ID
        [HttpGet("metadata/{fileId}")]
        public async Task<IActionResult> GetFileMetadata(Guid fileId)
        {
            try
            {
                // Get file metadata
                var fileModel = await _fileService.GetFileInfoAsync(fileId);
                if (fileModel == null)
                {
                    return NotFound("File not found.");
                }

                // Prepare anonymous object with metadata properties
                var metadata = new
                {
                    Id = fileModel.Id,
                    FileName = fileModel.FileName,
                    FileSize = fileModel.Size,
                    UploadedAt = fileModel.UploadDate,
                    Hash = fileModel.Hash
                };

                // Return metadata JSON
                return Ok(metadata);
            }
            catch (Exception ex)
            {
                // Return 500 if retrieval failed
                return StatusCode(500, $"Error retrieving file metadata: {ex.Message}");
            }
        }

        // Endpoint to get the content of a file as plain text by its ID
        [HttpGet("content/{fileId}")]
        public async Task<IActionResult> GetFileContent(Guid fileId)
        {
            try
            {
                // Retrieve file metadata
                var fileModel = await _fileService.GetFileInfoAsync(fileId);
                if (fileModel == null)
                {
                    return NotFound("File not found.");
                }

                // Get file content bytes
                var contentBytes = await _fileService.GetFileContentAsync(fileId);
                if (contentBytes == null)
                {
                    return NotFound("File content not found.");
                }

                // Convert bytes to UTF-8 string
                var content = Encoding.UTF8.GetString(contentBytes);
                return Ok(content);
            }
            catch (Exception ex)
            {
                // Return 500 if content retrieval failed
                return StatusCode(500, $"Error retrieving file content: {ex.Message}");
            }
        }

        // Endpoint to list all files metadata
        [HttpGet("all_files")]
        public async Task<IActionResult> GetAllFiles()
        {
            try
            {
                // Get all files metadata
                var files = await _fileService.GetAllFilesAsync();
                // Return 404 if none found
                if (files == null || !files.Any())
                {
                    return NotFound("No files found.");
                }

                // Project files to simplified objects
                var fileList = files.Select(f => new
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    FileSize = f.Size,
                    UploadedAt = f.UploadDate,
                    Hash = f.Hash
                }).ToList();

                // Return list of files metadata
                return Ok(fileList);
            }
            catch (Exception ex)
            {
                // Return 500 if retrieval failed
                return StatusCode(500, $"Error retrieving all files: {ex.Message}");
            }
        }
    }
}
