
using System.Text.Json.Serialization;

namespace NetStandardLibraries.Model
{
    public class MessageEntity
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
