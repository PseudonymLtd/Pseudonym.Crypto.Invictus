using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.Invictus
{
    public sealed class InvictusFund
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("ticker")]
        public string Symbol { get; set; }

        [JsonProperty("circulating_supply")]
        public string CirculatingSupply { get; set; }

        [JsonProperty("net_asset_value")]
        public string NetValue { get; set; }

        [JsonProperty("nav_per_token")]
        public string NetAssetValuePerToken { get; set; }

        [JsonProperty("assets")]
        public IReadOnlyList<InvictusAsset> Assets { get; set; } = new List<InvictusAsset>();
    }
}
