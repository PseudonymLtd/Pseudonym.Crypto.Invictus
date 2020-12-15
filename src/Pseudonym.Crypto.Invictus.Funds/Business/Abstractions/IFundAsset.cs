﻿namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface IFundAsset
    {
        ICoin Coin { get; }

        decimal Value { get; }

        decimal Share { get; }
    }
}
