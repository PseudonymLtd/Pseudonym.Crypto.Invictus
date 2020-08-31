using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.TrackerService.Clients.Models
{
    public class InvictusAsset
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("ticker")]
        public string Symbol { get; set; }

        [JsonProperty("usd_value")]
        public decimal Value { get; set; }
    }
}
