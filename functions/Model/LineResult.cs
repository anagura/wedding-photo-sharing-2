using Newtonsoft.Json;

namespace functions.Model
{
    public class LineResult
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("messageType")]
        public int MessageType { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }
    }
}
