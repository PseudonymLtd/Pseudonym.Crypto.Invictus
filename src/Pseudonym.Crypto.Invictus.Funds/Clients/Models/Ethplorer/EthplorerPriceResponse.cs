using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Shared.Enums;

#pragma warning disable SA1402

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.Ethplorer
{
    public sealed class EthplorerPriceResponse
    {
        [JsonProperty("history")]
        public EthplorerPriceData Data { get; set; }
    }

    public sealed class EthplorerPriceData
    {
        [JsonProperty("current")]
        public EthplorerPriceSummary Summary { get; set; }

        [JsonProperty("countTxs")]
        public IReadOnlyList<EthplorerTransactionHistory> Transactions { get; set; }

        [JsonProperty("prices")]
        public IReadOnlyList<EthplorerPrice> Prices { get; set; }
    }

    public sealed class EthplorerTransactionHistory
    {
        [JsonProperty("_id")]
        public EthplorerDate Date { get; set; }

        [JsonProperty("ts")]
        public int Total { get; set; }

        [JsonProperty("cnt")]
        public int Count { get; set; }
    }

    public sealed class EthplorerDate
    {
        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("month")]
        public int Month { get; set; }

        [JsonProperty("day")]
        public int Day { get; set; }

        [JsonIgnore]
        public DateTime Date => new DateTime(Year, Month, Day, 0, 0, 0, DateTimeKind.Utc);
    }

    public sealed class EthplorerPrice
    {
        [JsonProperty("ts")]
        public int TransactionCount { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("hour")]
        public int Hour { get; set; }

        [JsonProperty("open")]
        public decimal Open { get; set; }

        [JsonProperty("close")]
        public decimal Close { get; set; }

        [JsonProperty("high")]
        public decimal High { get; set; }

        [JsonProperty("low")]
        public decimal Low { get; set; }

        [JsonProperty("volume")]
        public decimal TokenVolume { get; set; }

        [JsonProperty("volumeConverted")]
        public decimal Volume { get; set; }

        [JsonProperty("cap")]
        public decimal MarketCap { get; set; }

        [JsonProperty("average")]
        public decimal Average { get; set; }

        public decimal GetMarketPrice(PriceMode priceMode)
        {
            return priceMode switch
            {
                PriceMode.Avg => Average,
                PriceMode.Open => Open,
                PriceMode.Close => Close,
                PriceMode.High => High,
                PriceMode.Low => Low,
                _ => throw new ArgumentException($"Arg not handled: {priceMode}", nameof(priceMode)),
            };
        }
    }
}
