using System;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models
{
    public sealed class InvictusPerformance
    {
        [JsonProperty("x")]
        public DateTime Date { get; set; }

        [JsonProperty("token_y")]
        public string NetAssetValuePerToken { get; set; }

        [JsonProperty("fund_y")]
        public string NetValue { get; set; }
    }
}
