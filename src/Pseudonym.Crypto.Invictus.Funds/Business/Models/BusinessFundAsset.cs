using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal sealed class BusinessFundAsset : IFundAsset
    {
        public IHolding Holding { get; set; }

        public decimal Quantity { get; set; }

        public decimal PricePerToken { get; set; }

        public decimal Total { get; set; }

        public decimal Share { get; set; }
    }
}
