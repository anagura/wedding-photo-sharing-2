
using System.Text.Json.Serialization;

namespace NetStandardLibraries.Model
{
    public class ImageConversionRequest
    {
        [JsonPropertyName("xamlData")]
        public string XamlData { get; set; }
    }
}
