using System;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface IAsset
    {
        string Name { get; }

        string Symbol { get; }

        decimal Value { get; }

        decimal Share { get; }

        Uri Link { get; }
    }
}
