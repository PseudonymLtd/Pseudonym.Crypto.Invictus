using Pseudonym.Crypto.Investments.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.TrackerService.Business.Models
{
    internal class BusinessAsset : IAsset
    {
        public string Name { get; set; }

        public string Symbol { get; set; }

        public decimal Value { get; set; }
    }
}
