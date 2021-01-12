using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.TheGraph
{
    public sealed class UniswapResponse<TData>
    {
        [JsonRequired]
        [JsonProperty("data")]
        public TData Data { get; set; }
    }
}
