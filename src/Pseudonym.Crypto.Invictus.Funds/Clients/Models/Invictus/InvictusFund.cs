using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.Invictus
{
    public sealed class InvictusFund : IInvictusFund
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

        [JsonIgnore]
        public IReadOnlyList<InvictusAsset> Assets => new List<InvictusAsset>();

        [JsonIgnore]
        Symbol IInvictusFund.Symbol => Enum.Parse<Symbol>(Symbol);
    }
}
