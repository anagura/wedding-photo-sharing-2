
using System.Text.Json.Serialization;

namespace NetStandardLibraries.Model
{
    public class ImageConversionResponse
    {
        [JsonPropertyName("imageData")]
        public byte[] ImageData { get; set; }

        [JsonPropertyName("imageData")]
        public string ErrorMessage { get; set; }
    }
}
