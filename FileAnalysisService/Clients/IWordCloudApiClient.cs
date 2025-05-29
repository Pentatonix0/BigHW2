namespace FileAnalysisService.Clients;

public class WordCloudParameters
{
	public string? Format { get; set; } = "png";
	public int? Width { get; set; } = 3840;
	public int? Height { get; set; } = 2160;
	public string? BackgroundColor { get; set; } = "transparent";
	public string? FontColor { get; set; }
	public float? FontScale { get; set; } = 14;
	public bool? RemoveStopwords { get; set; } = true;
	public string? Language { get; set; }
	public bool? UseWordList { get; set; }
}

public interface IWordCloudApiClient
{
	Task<Stream?> CreateWordCloudAsync(string text, WordCloudParameters? parameters = null);
}