using System;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    public class BusinessPerformance : IPerformance
    {
        public DateTime Date { get; set; }

        public decimal NetAssetValuePerToken { get; set; }

        public decimal NetValue { get; set; }
    }
}
