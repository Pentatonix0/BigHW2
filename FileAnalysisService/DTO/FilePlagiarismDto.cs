namespace FileAnalysisService.DTO
{
	public class FilePlagiarismRequestDTO
	{
		public Guid FileId { get; set; }
		public Guid OtherFileId { get; set; }

	}

	public class FilePlagiarismResponceDto
	{
		public Guid FileId { get; set; }
		public Guid OtherFileId { get; set; }
		public double SimilarityPercentage { get; set; }

	}
}