using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal sealed class BusinessTimeMultiplier : ITimeMultiplier
    {
        public int RangeMin { get; set; }

        public int RangeMax { get; set; }

        public decimal Multiplier { get; set; }
    }
}
