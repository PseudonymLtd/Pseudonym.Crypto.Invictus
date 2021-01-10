using System;
using Pseudonym.Crypto.Invictus.Web.Client.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.Web.Client.Business
{
    public class BusinessTrade : ITrade
    {
        public DateTime Date { get; set; }

        public decimal Quantity { get; set; }

        public decimal SignedQuantity => IsInbound
            ? Quantity
            : -Quantity;

        public decimal NetSnapshotPrice => NetSnapshotPricePerToken * Quantity;

        public decimal NetSnapshotPricePerToken { get; set; }

        public decimal NetCurrentPrice => NetCurrentPricePerToken * Quantity;

        public decimal NetCurrentPricePerToken { get; set; }

        public decimal BurnGain => NetCurrentPrice - MarketSnapshotPrice ?? decimal.Zero;

        public decimal NetDiff => IsInbound
            ? NetCurrentPrice - NetSnapshotPrice
            : NetSnapshotPrice - NetCurrentPrice;

        public decimal NetDiffPerToken => IsInbound
            ? NetCurrentPricePerToken - NetSnapshotPricePerToken
            : NetSnapshotPricePerToken - NetCurrentPricePerToken;

        public decimal? MarketSnapshotPrice => MarketSnapshotPricePerToken * Quantity;

        public decimal? MarketSnapshotPricePerToken { get; set; }

        public decimal? MarketCurrentPrice => MarketCurrentPricePerToken * Quantity;

        public decimal? MarketCurrentPricePerToken { get; set; }

        public decimal? MarketDiff => IsInbound
            ? MarketCurrentPrice - MarketSnapshotPrice
            : MarketSnapshotPrice - MarketCurrentPrice;

        public decimal? MarketDiffPerToken => IsInbound
            ? MarketCurrentPricePerToken - MarketSnapshotPricePerToken
            : MarketSnapshotPricePerToken - MarketCurrentPricePerToken;

        public bool IsInbound { get; set; }

        public bool IsTradeable { get; set; }

        public bool IsStake => IsStakeLockup || IsStakeRelease;

        public bool IsStakeLockup { get; set; }

        public bool IsStakeRelease { get; set; }

        public bool IsOwned { get; set; }

        public bool IsBurn { get; set; }

        public string GetTradeName()
        {
            return IsInbound
                ? IsOwned
                    ? "Transfer In"
                    : IsStakeRelease
                        ? "Stake Withdrawal"
                        : "Buy"
                : IsOwned
                    ? "Transfer Out"
                    : IsStakeLockup
                        ? "Stake Lockup"
                        : IsBurn
                            ? "Burn"
                            : "Sell";
        }
    }
}
