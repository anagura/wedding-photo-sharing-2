using Newtonsoft.Json;

namespace functions.Model
{
    public class VisionAdultResultDetail
    {
        [JsonProperty("isAdultContent")]
        public bool isAdultContent { get; set; }

        [JsonProperty("adultScore")]
        public double adultScore { get; set; }

        [JsonProperty("isRacyContent")]
        public bool isRacyContent { get; set; }

        [JsonProperty("racyScore")]
        public double racyScore { get; set; }
    }
}
