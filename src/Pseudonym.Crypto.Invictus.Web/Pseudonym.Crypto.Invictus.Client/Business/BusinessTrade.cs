using Pseudonym.Crypto.Invictus.Web.Client.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.Web.Client.Business
{
    public class BusinessTrade : ITrade
    {
        public decimal Quantity { get; set; }

        public decimal SignedQuantity => IsInbound
            ? Quantity
            : -Quantity;

        public decimal NetSnapshotPrice => NetSnapshotPricePerToken * Quantity;

        public decimal NetSnapshotPricePerToken { get; set; }

        public decimal NetCurrentPrice => NetCurrentPricePerToken * Quantity;

        public decimal NetCurrentPricePerToken { get; set; }

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
    }
}
