using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Pseudonym.Crypto.Invictus.Funds.Configuration.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Configuration
{
    public sealed class FundSettings : IFundSettings
    {
        public string FundName { get; set; }

        public string Description { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Symbol Symbol { get; set; }

        public DateTimeOffset InceptionDate { get; set; }

        public int Decimals { get; set; }

        public bool Tradable { get; set; }

        public bool Burnable { get; set; }

        public string ContractAddress { get; set; }

        public string CoinGeckoId { get; set; }

        public FundLinks Links { get; set; }

        public List<FundAsset> Assets { get; set; } = new List<FundAsset>();

        [JsonIgnore]
        EthereumAddress IFundSettings.ContractAddress => new EthereumAddress(ContractAddress);

        [JsonIgnore]
        IReadOnlyList<FundAsset> IFundSettings.Assets => Assets;

        public sealed class FundAsset
        {
            public string Name { get; set; }

            public string Symbol { get; set; }

            public decimal Value { get; set; }

            public bool Tradable { get; set; }

            public bool IsCoin { get; set; }

            public string ContractAddress { get; set; }

            public decimal? FixedValuePerCoin { get; set; }

            public int? Decimals { get; set; }

            public Uri Link { get; set; }

            public Uri ImageLink { get; set; }
        }

        public sealed class FundLinks
        {
            public Uri Lite { get; set; }

            public Uri Fact { get; set; }

            public Uri External { get; set; }
        }
    }
}
