using System;
using System.Collections.Generic;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface IFund
    {
        string Name { get; }

        string DisplayName { get; }

        string Description { get; }

        Uri FactSheetUri { get; }

        Uri LitepaperUri { get; }

        IToken Token { get; }

        bool IsTradeable { get; }

        decimal CirculatingSupply { get; set; }

        decimal NetValue { get; }

        decimal NetAssetValuePerToken { get; }

        decimal? MarketValue { get; }

        decimal? MarketValuePerToken { get; }

        IReadOnlyList<IAsset> Assets { get; }
    }
}
