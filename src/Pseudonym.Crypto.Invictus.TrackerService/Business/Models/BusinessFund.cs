using System.Collections.Generic;
using Pseudonym.Crypto.Investments.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.TrackerService.Business.Models
{
    internal sealed class BusinessFund : IFund
    {
        public string Name { get; set; }

        public IToken Token { get; set; }

        public decimal CirculatingSupply { get; set; }

        public decimal NetAssetValue { get; set; }

        public decimal NetAssetValuePerToken { get; set; }

        public decimal? MarketValuePerToken { get; set; }

        public IReadOnlyList<IAsset> Assets { get; set; } = new List<IAsset>();
    }
}
