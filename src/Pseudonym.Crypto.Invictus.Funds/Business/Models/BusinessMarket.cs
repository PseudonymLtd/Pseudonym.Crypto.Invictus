using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal sealed class BusinessMarket : IMarket
    {
        public bool IsTradable { get; set; }

        public decimal Cap { get; set; }

        public decimal Total { get; set; }

        public decimal PricePerToken { get; set; }

        public decimal DiffDaily { get; set; }

        public decimal DiffWeekly { get; set; }

        public decimal DiffMonthly { get; set; }

        public decimal Volume { get; set; }

        public decimal VolumeDiffDaily { get; set; }

        public decimal VolumeDiffWeekly { get; set; }

        public decimal VolumeDiffMonthly { get; set; }
    }
}
