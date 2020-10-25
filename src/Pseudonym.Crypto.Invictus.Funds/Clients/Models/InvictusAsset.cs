using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models
{
    public class InvictusAsset
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("ticker")]
        public string Symbol { get; set; }

        [JsonProperty("usd_value")]
        public string Value { get; set; }
    }
}
