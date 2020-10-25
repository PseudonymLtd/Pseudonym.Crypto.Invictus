using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models
{
    internal sealed class ListFundsResponse
    {
        [JsonProperty("data")]
        public List<InvictusFund> Funds { get; set; } = new List<InvictusFund>();
    }
}
