using System.Text.RegularExpressions;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;

namespace FileAnalysisService.Clients
{
	public class WordCloudApiClient : IWordCloudApiClient
	{
		private readonly HttpClient _httpClient;

		public WordCloudApiClient(HttpClient httpClient)
		{
			_httpClient = httpClient;

			if (_httpClient.BaseAddress == null)
			{
				throw new InvalidOperationException("HttpClient BaseAddress is not configured for WordCloudApiClient.");
			}
		}

		public async Task<Stream?> CreateWordCloudAsync(string fileText, WordCloudParameters? parameters = null)
		{
			parameters ??= new WordCloudParameters();

			var requestBody = new
			{
				text = fileText,
				format = parameters.Format,
				width = parameters.Width,
				height = parameters.Height,
				backgroundColor = parameters.BackgroundColor,
				fontColor = parameters.FontColor,
				fontScale = parameters.FontScale,
				removeStopwords = parameters.RemoveStopwords,
				language = parameters.Language,
				useWordList = parameters.UseWordList
			};

			try
			{
				var options = new JsonSerializerOptions
				{
					DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
				};

				var response = await _httpClient.PostAsJsonAsync("", requestBody, options);

				if (response.IsSuccessStatusCode)
				{
					return await response.Content.ReadAsStreamAsync();
				}
				else
				{
					return null;
				}
			}
			catch
			{
				return null;
			}
		}
	}
}
