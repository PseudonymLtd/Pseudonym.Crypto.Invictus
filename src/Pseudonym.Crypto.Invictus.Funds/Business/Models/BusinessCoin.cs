using System;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal sealed class BusinessCoin : ICoin
    {
        public string Name { get; set; }

        public string Symbol { get; set; }

        public int? Decimals { get; set; }

        public EthereumAddress? ContractAddress { get; set; }

        public decimal? FixedValuePerCoin { get; set; }

        public string HexColour { get; set; }

        public Uri Link { get; set; }

        public Uri ImageLink { get; set; }

        public Uri MarketLink { get; set; }
    }
}