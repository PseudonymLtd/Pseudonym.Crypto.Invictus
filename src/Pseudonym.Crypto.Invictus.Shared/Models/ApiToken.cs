using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiToken
    {
        [JsonProperty("symbol")]
        public Symbol Symbol { get; set; }

        [JsonProperty("decimals")]
        public int Decimals { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }
    }
}
