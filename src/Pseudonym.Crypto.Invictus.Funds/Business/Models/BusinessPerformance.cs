using System;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal sealed class BusinessPerformance : IPerformance
    {
        public DateTime Date { get; set; }

        public decimal NetAssetValuePerToken { get; set; }

        public decimal? MarketAssetValuePerToken { get; set; }

        public decimal? MarketCap { get; set; }

        public decimal? Volume { get; set; }
    }
}
