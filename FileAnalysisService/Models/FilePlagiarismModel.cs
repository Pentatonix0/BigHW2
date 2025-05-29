namespace FileAnalysisService.Models
{
	public class PlagiarismModel
	{
		public Guid Id { get; set; }
		public Guid FileId { get; set; }
		public Guid OtherFileId { get; set; }
		public double SimilarityPercentage { get; set; }
		public DateTime ComparisonDate { get; set; }
	}
}