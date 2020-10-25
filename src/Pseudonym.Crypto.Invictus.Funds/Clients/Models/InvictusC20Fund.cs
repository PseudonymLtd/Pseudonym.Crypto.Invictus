using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models
{
    public sealed class InvictusC20Fund
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("usd_value")]
        public string NetValue { get; set; }

        [JsonProperty("nav_per_token")]
        public string NetAssetValuePerToken { get; set; }

        [JsonProperty("circulating_supply")]
        public string CirculatingSupply { get; set; }

        [JsonProperty("holdings")]
        public List<Holding> Holdings { get; set; }

        public sealed class Holding
        {
            [JsonProperty("full_name")]
            public string Name { get; set; }

            [JsonProperty("name")]
            public string Symbol { get; set; }

            [JsonProperty("value")]
            public string Value { get; set; }
        }
    }
}
