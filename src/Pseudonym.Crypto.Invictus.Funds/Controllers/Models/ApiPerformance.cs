using System;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Funds.Controllers.Models
{
    public sealed class ApiPerformance
    {
        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("nav_per_token")]
        public decimal NetAssetValuePerToken { get; set; }

        [JsonProperty("net_value")]
        public decimal NetValue { get; set; }
    }
}
