using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.TrackerService.Clients.Models
{
    public sealed class InvictusFund
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("ticker")]
        public string Symbol { get; set; }

        [JsonProperty("circulating_supply")]
        public decimal CirculatingSupply { get; set; }

        [JsonProperty("net_asset_value")]
        public decimal NetAssetValue { get; set; }

        [JsonProperty("nav_per_token")]
        public decimal NetAssetValuePerToken { get; set; }

        [JsonProperty("price")]
        public decimal? MarketValuePerToken { get; set; }

        [JsonProperty("assets")]
        public IReadOnlyList<InvictusAsset> Assets { get; set; } = new List<InvictusAsset>();
    }
}
