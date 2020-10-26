using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiAsset
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("value")]
        public decimal Value { get; set; }

        [JsonProperty("share")]
        public decimal Share { get; set; }
    }
}
