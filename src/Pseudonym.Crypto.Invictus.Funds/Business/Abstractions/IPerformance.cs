﻿using System;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface IPerformance
    {
        DateTime Date { get; }

        decimal NetAssetValuePerToken { get; }

        decimal NetValue { get; }

        decimal? MarketAssetValuePerToken { get; }

        decimal? MarketCap { get; }
    }
}
