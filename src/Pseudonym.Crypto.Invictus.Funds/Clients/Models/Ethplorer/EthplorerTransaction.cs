using System.Collections.Generic;
using Newtonsoft.Json;

#pragma warning disable SA1402

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.Ethplorer
{
    public sealed class EthplorerTransaction
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("timestamp")]
        public long UnixTimestamp { get; set; }

        [JsonProperty("blockNumber")]
        public long BlockNumber { get; set; }

        [JsonProperty("confirmations")]
        public long Confirmations { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("value")]
        public decimal Value { get; set; }

        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("input")]
        public string Input { get; set; }

        [JsonProperty("gasLimit")]
        public long GasLimit { get; set; }

        [JsonProperty("gasUsed")]
        public long GasUsed { get; set; }

        [JsonProperty("operations")]
        public IReadOnlyList<EthplorerOperation> Operations { get; set; } = new List<EthplorerOperation>();
    }

    public sealed class EthplorerOperation
    {
        [JsonProperty("timestamp")]
        public long UnixTimestamp { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("usdPrice")]
        public decimal Price { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("isEth")]
        public bool IsEth { get; set; }

        [JsonProperty("priority")]
        public int Priority { get; set; }

        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("addresses")]
        public List<string> Addresses { get; set; } = new List<string>();

        [JsonProperty("tokenInfo")]
        public EthplorerTokenInfo TokenInfo { get; set; }
    }
}
