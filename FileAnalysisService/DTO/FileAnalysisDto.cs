namespace FileAnalysisService.DTO
{


	public class FileDto
	{
		public Guid Id { get; set; }
		public string Content { get; set; }
		public string FileName { get; set; }
	}
	public class FileAnalysisResponceDto
	{
		public Guid AnalysisId { get; set; }
		public Guid FileId { get; set; }
		public int ParagraphCount { get; set; }
		public int WordCount { get; set; }
		public int CharCount { get; set; }
		public string? WordCloudImagePath { get; set; }
	}
}