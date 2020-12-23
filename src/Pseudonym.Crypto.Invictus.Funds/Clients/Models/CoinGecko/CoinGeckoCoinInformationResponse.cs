using System;
using System.Collections.Generic;
using Newtonsoft.Json;

#pragma warning disable SA1402

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.CoinGecko
{
    public sealed class CoinGeckoCoinInformationResponse
    {
        private string symbol;

        [JsonProperty("id")]
        public string CoinId { get; set; }

        [JsonProperty("symbol")]
        public string Symbol
        {
            get => symbol;
            set => symbol = value.ToUpper();
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("categories")]
        public List<string> Categories { get; set; }

        [JsonProperty("description")]
        public CoinGeckoCoinDescription Description { get; set; }

        [JsonProperty("links")]
        public CoinGeckoCoinLinks Links { get; set; }

        [JsonProperty("image")]
        public CoinGeckoCoinImage Images { get; set; }

        [JsonProperty("contract_address")]
        public string ContractAddress { get; set; }

        [JsonProperty("market_data")]
        public CoinGeckoCoinMarketData Market { get; set; }

        [JsonProperty("last_updated")]
        public DateTime UpdatedAt { get; set; }
    }

    public class CoinGeckoCoinDescription
    {
        [JsonProperty("en")]
        public string Text { get; set; }
    }

    public class CoinGeckoCoinLinks
    {
        private string subredditUrl;
        private string twitterHandle;
        private string facebookHandle;
        private string telegramHandle;

        [JsonProperty("homepage")]
        public List<string> ProjectLinks { get; set; }

        [JsonProperty("blockchain_site")]
        public List<string> BlockchainLinks { get; set; }

        [JsonProperty("twitter_screen_name")]
        public string TwitterHandle
        {
            get => twitterHandle;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    twitterHandle = value;
                }
            }
        }

        [JsonProperty("facebook_username")]
        public string FacebookHandle
        {
            get => facebookHandle;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    facebookHandle = value;
                }
            }
        }

        [JsonProperty("telegram_channel_identifier")]
        public string TelegramHandle
        {
            get => telegramHandle;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    telegramHandle = value;
                }
            }
        }

        [JsonProperty("subreddit_url")]
        public string SubredditUrl
        {
            get => subredditUrl;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    subredditUrl = value;
                }
            }
        }

        [JsonProperty("repos_url")]
        public CoinGeckoCoinRepo Repos { get; set; }
    }

    public class CoinGeckoCoinRepo
    {
        [JsonProperty("github")]
        public List<string> GitHubRepos { get; set; }

        [JsonProperty("bitbucket")]
        public List<string> BitBucketRepos { get; set; }
    }

    public class CoinGeckoCoinImage
    {
        [JsonProperty("thumb")]
        public string Thumb { get; set; }

        [JsonProperty("small")]
        public string Small { get; set; }

        [JsonProperty("large")]
        public string Large { get; set; }
    }

    public class CoinGeckoCoinMarketData
    {
        [JsonProperty("last_updated")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("market_cap_rank")]
        public int Rank { get; set; }

        [JsonProperty("total_supply")]
        public decimal TotalSupply { get; set; }

        [JsonProperty("circulating_supply")]
        public decimal CirculatingSupply { get; set; }

        [JsonProperty("current_price")]
        public CoinGeckoCoinCurrencyValue<decimal> Price { get; set; }

        [JsonProperty("high_24h")]
        public CoinGeckoCoinCurrencyValue<decimal> PriceHigh { get; set; }

        [JsonProperty("low_24h")]
        public CoinGeckoCoinCurrencyValue<decimal> PriceLow { get; set; }

        [JsonProperty("total_volume")]
        public CoinGeckoCoinCurrencyValue<decimal> Volume { get; set; }

        [JsonProperty("market_cap")]
        public CoinGeckoCoinCurrencyValue<decimal> MarketCap { get; set; }

        [JsonProperty("market_cap_change_24h")]
        public decimal MarketCapChangeDaily { get; set; }

        [JsonProperty("market_cap_change_percentage_24h")]
        public decimal MarketCapChangePercentageDaily { get; set; }

        [JsonProperty("price_change_24h")]
        public decimal PriceChangeDaily { get; set; }

        [JsonProperty("price_change_percentage_24h")]
        public decimal PriceChangePercentageDaily { get; set; }

        [JsonProperty("price_change_percentage_7d")]
        public decimal PriceChangePercentageWeekly { get; set; }

        [JsonProperty("price_change_percentage_30d")]
        public decimal PriceChangePercentageMonthly { get; set; }

        [JsonProperty("price_change_percentage_1y")]
        public decimal PriceChangePercentageYearly { get; set; }

        [JsonProperty("roi")]
        public CoinGeckoCoinROI ROI { get; set; }
    }

    public sealed class CoinGeckoCoinCurrencyValue<TValue>
        where TValue : struct
    {
        [JsonProperty("usd")]
        public TValue Value { get; set; }
    }

    public class CoinGeckoCoinROI
    {
        [JsonProperty("times")]
        public decimal Times { get; set; }

        [JsonProperty("percentage")]
        public decimal Percentage { get; set; }
    }
}
