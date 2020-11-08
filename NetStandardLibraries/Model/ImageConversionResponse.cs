
using System.Text.Json.Serialization;

namespace NetStandardLibraries.Model
{
    public class ImageConversionResponse
    {
        [JsonPropertyName("imageData")]
        public string ImageData { get; set; }
    }
}
