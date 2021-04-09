
using System.Text.Json.Serialization;

namespace NetStandardLibraries.Model
{
    public class MessageEntity
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
