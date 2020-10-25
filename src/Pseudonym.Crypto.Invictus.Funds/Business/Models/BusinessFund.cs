using System.Collections.Generic;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal sealed class BusinessFund : IFund
    {
        public string Name { get; set; }

        public IToken Token { get; set; }

        public bool IsTradeable => MarketValuePerToken.HasValue;

        public decimal CirculatingSupply { get; set; }

        public decimal NetValue { get; set; }

        public decimal NetAssetValuePerToken { get; set; }

        public decimal? MarketValue => MarketValuePerToken.HasValue
            ? MarketValuePerToken * CirculatingSupply
            : default;

        public decimal? MarketValuePerToken { get; set; }

        public IReadOnlyList<IAsset> Assets { get; set; } = new List<IAsset>();
    }
}
