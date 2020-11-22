using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.Invictus
{
    internal sealed class ListFundsResponse
    {
        [JsonProperty("data")]
        public List<InvictusFund> Funds { get; set; } = new List<InvictusFund>();
    }
}
