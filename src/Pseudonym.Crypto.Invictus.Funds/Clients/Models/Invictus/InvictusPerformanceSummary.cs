using System;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.Invictus
{
    public class InvictusPerformanceSummary
    {
        public DateTime Date { get; set; }

        public decimal NetValue { get; set; }

        public decimal Open { get; set; }

        public decimal Close { get; set; }

        public decimal High { get; set; }

        public decimal Low { get; set; }

        public decimal Average { get; set; }

        public decimal GetNav(PriceMode priceMode)
        {
            return priceMode switch
            {
                PriceMode.Avg => Average,
                PriceMode.Open => Open,
                PriceMode.Close => Close,
                PriceMode.High => High,
                PriceMode.Low => Low,
                _ => throw new ArgumentException($"Arg not handled: {priceMode}", nameof(priceMode)),
            };
        }
    }
}
