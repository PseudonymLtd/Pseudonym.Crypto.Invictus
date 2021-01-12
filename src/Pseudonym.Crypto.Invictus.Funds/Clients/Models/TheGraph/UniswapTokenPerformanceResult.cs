using System;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.TheGraph
{
    public sealed class UniswapTokenPerformanceResult
    {
        public DateTime Date { get; set; }

        public decimal Price { get; set; }

        public decimal MarketCap { get; set; }

        public decimal Volume { get; set; }
    }
}
