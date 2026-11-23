using System.Text.Json.Serialization;

namespace CorporateHub.Models
{
	public class OpenMeteoResponse
	{
		[JsonPropertyName("current")]
		public CurrentWeather? Current { get; set; }
	}

	public class CurrentWeather
	{
		[JsonPropertyName("temperature_2m")]
		public double Temperature2m { get; set; }
	}
}
