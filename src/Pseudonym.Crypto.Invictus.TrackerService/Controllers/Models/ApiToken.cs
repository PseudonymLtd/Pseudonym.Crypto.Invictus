using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;

namespace Pseudonym.Crypto.Invictus.TrackerService.Controllers.Models
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
