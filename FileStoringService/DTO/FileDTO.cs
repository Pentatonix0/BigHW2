namespace FileStoringService.DTOs;

public class FileDto
{
	public Guid Id { get; set; }
	public string FileName { get; set; }
	public long Size { get; set; }
	public DateTime UploadDate { get; set; }
	public string Hash { get; set; }
}