using System;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface IFundAsset
    {
        string Name { get; }

        string Symbol { get; }

        decimal Value { get; }

        decimal Share { get; }

        Uri Link { get; }

        string CoinId { get; }
    }
}
