using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    public sealed class BusinessNav : INav
    {
        public decimal Value { get; set; }

        public decimal ValuePerToken { get; set; }

        public decimal DiffDaily { get; set; }

        public decimal DiffWeekly { get; set; }

        public decimal DiffMonthly { get; set; }
    }
}
