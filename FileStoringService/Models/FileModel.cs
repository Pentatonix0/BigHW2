namespace FileStoringService.Models
{
	public class FileModel
	{
		public Guid Id { get; set; }
		public string FileName { get; set; }
		public long Size { get; set; }
		public string Path { get; set; }
		public DateTime UploadDate { get; set; }
		public string Hash { get; set; }
	}
}