using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.Invictus
{
    public sealed class InvictusFundData : IInvictusFund
    {
        [JsonIgnore]
        public string Name { get; set; }

        [JsonIgnore]
        public Symbol Symbol { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("net_asset_value")]
        public string NetValue { get; set; }

        [JsonProperty("nav_per_token")]
        public string NetAssetValuePerToken { get; set; }

        [JsonProperty("number_of_tokens")]
        public string CirculatingSupply { get; set; }

        [JsonProperty("truncated_assets")]
        public IReadOnlyList<InvictusAsset> Assets { get; set; } = new List<InvictusAsset>();
    }
}
