using Newtonsoft.Json;

namespace functions.Model
{
    public class VisionAdultResult
    {
        [JsonProperty("adult")]
        public VisionAdultResultDetail Adult { get; set; }
    }
}
