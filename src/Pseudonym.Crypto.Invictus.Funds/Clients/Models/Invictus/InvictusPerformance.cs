using System;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.Invictus
{
    public sealed class InvictusPerformance
    {
        private DateTimeOffset date;

        [JsonProperty("x")]
        public DateTimeOffset Date
        {
            get => date;
            set => date = value.Round();
        }

        [JsonProperty("token_y")]
        public string NetAssetValuePerToken { get; set; }

        [JsonProperty("fund_y")]
        public string NetValue { get; set; }
    }
}
