using System;
using Pseudonym.Crypto.Invictus.Funds.Data.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Data.Models
{
    public sealed class DataFundPerformance : IDataAggregatable
    {
        public string Address { get; set; }

        public DateTime Date { get; set; }

        public decimal Nav { get; set; }

        public decimal Price { get; set; }

        public decimal MarketCap { get; set; }

        public decimal Volume { get; set; }
    }
}
