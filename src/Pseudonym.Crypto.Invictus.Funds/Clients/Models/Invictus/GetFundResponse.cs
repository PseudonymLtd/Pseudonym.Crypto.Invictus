using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.Invictus
{
    public sealed class GetFundResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("data")]
        public IReadOnlyList<InvictusFundData> Data { get; set; }
    }
}
