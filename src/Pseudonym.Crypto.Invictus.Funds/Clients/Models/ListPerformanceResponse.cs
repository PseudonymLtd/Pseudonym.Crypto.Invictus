using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models
{
    public sealed class ListPerformanceResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("data")]
        public List<InvictusPerformance> Performance { get; set; } = new List<InvictusPerformance>();
    }
}
