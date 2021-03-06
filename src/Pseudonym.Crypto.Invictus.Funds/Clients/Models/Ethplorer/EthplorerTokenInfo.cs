﻿using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.Ethplorer
{
    public sealed class EthplorerTokenInfo
    {
        [JsonProperty("address")]
        public string ContractAddress { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("decimals")]
        public string Decimals { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("totalSupply")]
        public string TotalSupply { get; set; }

        [JsonProperty("owner")]
        public string Owner { get; set; }

        [JsonProperty("transfersCount")]
        public int TransferCount { get; set; }

        [JsonProperty("lastUpdated")]
        public int LastUpdated { get; set; }

        [JsonProperty("issuancesCount")]
        public int IssuanceCount { get; set; }

        [JsonProperty("holdersCount")]
        public int HolderCount { get; set; }

        [JsonProperty("image")]
        public string ImageUri { get; set; }

        [JsonProperty("website")]
        public string WebsiteUri { get; set; }

        [JsonProperty("facebook")]
        public string FacebookHandle { get; set; }

        [JsonProperty("telegram")]
        public string TelegramUri { get; set; }

        [JsonProperty("twitter")]
        public string TwitterHandle { get; set; }

        [JsonProperty("reddit")]
        public string RedditHandle { get; set; }

        [JsonProperty("coingecko")]
        public string CoingeckoHandle { get; set; }

        [JsonProperty("ethTransfersCount")]
        public int EthTransferCount { get; set; }

        [JsonProperty("countOps")]
        public int OperationCount { get; set; }

        [JsonIgnore]
        public EthplorerPriceSummary Price { get; set; }

        [JsonExtensionData]
        private IDictionary<string, JToken> Extensions { get; set; } = new Dictionary<string, JToken>();

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (Extensions.ContainsKey(nameof(Price).ToLower()))
            {
                var opToken = Extensions[nameof(Price).ToLower()];
                if (opToken is JObject jObj)
                {
                    Price = jObj.ToObject<EthplorerPriceSummary>();
                }
            }
        }
    }
}
