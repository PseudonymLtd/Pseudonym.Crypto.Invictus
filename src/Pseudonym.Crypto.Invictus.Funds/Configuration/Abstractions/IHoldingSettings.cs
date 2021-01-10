using System;

namespace Pseudonym.Crypto.Invictus.Funds.Configuration.Abstractions
{
    public interface IHoldingSettings
    {
        string Symbol { get; }

        string Colour { get; }

        Uri Link { get; }

        Uri ImageLink { get; }
    }
}
