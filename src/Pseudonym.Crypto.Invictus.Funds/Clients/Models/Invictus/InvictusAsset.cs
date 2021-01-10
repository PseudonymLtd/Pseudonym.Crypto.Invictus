using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.Invictus
{
    public class InvictusAsset
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("ticker")]
        public string Symbol { get; set; }

        [JsonProperty("amount")]
        public string Quantity { get; set; }

        [JsonProperty("price")]
        public string PricePerToken { get; set; }

        [JsonProperty("usd_value")]
        public string Total { get; set; }
    }
}
