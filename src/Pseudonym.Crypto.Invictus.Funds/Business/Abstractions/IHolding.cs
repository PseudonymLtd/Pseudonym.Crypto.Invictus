using System;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface IHolding
    {
        string Name { get; }

        string Symbol { get; }

        int? Decimals { get; }

        bool IsCoin { get; }

        EthereumAddress? ContractAddress { get; }

        decimal? FixedValuePerCoin { get; }

        string HexColour { get; }

        Uri Link { get; }

        Uri ImageLink { get; }

        Uri MarketLink { get; }
    }
}
