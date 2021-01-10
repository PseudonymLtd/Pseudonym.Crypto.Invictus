using System;

namespace Pseudonym.Crypto.Invictus.Web.Client.Business.Abstractions
{
    public interface ITrade
    {
        DateTime Date { get; }

        decimal Quantity { get; }

        decimal SignedQuantity { get; }

        decimal NetSnapshotPrice { get; }

        decimal NetSnapshotPricePerToken { get; }

        decimal NetCurrentPrice { get; }

        decimal NetCurrentPricePerToken { get; }

        decimal NetDiff { get; }

        decimal NetDiffPerToken { get; }

        decimal? MarketSnapshotPrice { get; }

        decimal? MarketSnapshotPricePerToken { get; }

        decimal? MarketCurrentPrice { get; }

        decimal? MarketCurrentPricePerToken { get; }

        decimal? MarketDiff { get; }

        decimal? MarketDiffPerToken { get; }

        decimal BurnGain { get; }

        bool IsInbound { get; }

        bool IsTradeable { get; }

        bool IsStake { get; }

        bool IsStakeLockup { get; }

        bool IsStakeRelease { get; }

        bool IsOwned { get; }
    }
}
