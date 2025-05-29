namespace FileAnalysisService.Models
{
	public class FileAnalysisResultModel
	{
		public Guid Id { get; set; }
		public Guid FileId { get; set; }
		public int ParagraphCount { get; set; }
		public int WordCount { get; set; }
		public int CharCount { get; set; }
		public string? WordCloudImagePath { get; set; }
		public DateTime AnalysisDate { get; set; }
	}
}