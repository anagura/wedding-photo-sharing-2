
using System.Text.Json.Serialization;

namespace NetStandardLibraries.Model
{
    public class ImageConversionRequest
    {
        [JsonPropertyName("apiKey")]
        public string ApiKey { get; set; }

        [JsonPropertyName("xamlData")]
        public string XamlData { get; set; }
    }
}
