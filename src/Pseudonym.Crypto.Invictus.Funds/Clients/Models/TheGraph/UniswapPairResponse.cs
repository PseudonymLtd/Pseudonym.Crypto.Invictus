using Newtonsoft.Json;

#pragma warning disable SA1402

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.TheGraph
{
    public sealed class UniswapPairResponse
    {
        [JsonRequired]
        [JsonProperty("data")]
        public UniswapData Data { get; set; }
    }

    public sealed class UniswapData
    {
        [JsonRequired]
        [JsonProperty("pair")]
        public UniswapPair Pair { get; set; }
    }

    public sealed class UniswapPair
    {
        [JsonRequired]
        [JsonProperty("price0")]
        public string Price0 { get; set; }

        [JsonRequired]
        [JsonProperty("price1")]
        public string Price1 { get; set; }

        [JsonRequired]
        [JsonProperty("supply0")]
        public string Supply0 { get; set; }

        [JsonRequired]
        [JsonProperty("supply1")]
        public string Supply1 { get; set; }

        [JsonRequired]
        [JsonProperty("token0")]
        public UniswapToken Token0 { get; set; }

        [JsonRequired]
        [JsonProperty("token1")]
        public UniswapToken Token1 { get; set; }

        [JsonRequired]
        [JsonProperty("volume")]
        public string Volume { get; set; }
    }

    public sealed class UniswapToken
    {
        [JsonRequired]
        [JsonProperty("id")]
        public string ContractAddress { get; set; }

        [JsonRequired]
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonRequired]
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonRequired]
        [JsonProperty("decimals")]
        public string Decimals { get; set; }
    }
}
