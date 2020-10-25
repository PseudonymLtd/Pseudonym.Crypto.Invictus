using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models
{
    public sealed class CurrencyRates
    {
        [JsonProperty("time_next_update_utc")]
        public DateTimeOffset NextUpdate { get; set; }

        [JsonProperty("rates")]
        public Dictionary<string, decimal> Rates { get; set; }
    }
}
