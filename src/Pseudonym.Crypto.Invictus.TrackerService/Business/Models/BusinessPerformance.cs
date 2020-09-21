using System;
using Pseudonym.Crypto.Invictus.TrackerService.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.TrackerService.Business.Models
{
    public class BusinessPerformance : IPerformance
    {
        public DateTime Date { get; set; }

        public decimal NetAssetValuePerToken { get; set; }

        public decimal NetValue { get; set; }
    }
}
