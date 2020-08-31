using System.Collections.Generic;

namespace Pseudonym.Crypto.Investments.Business.Abstractions
{
    public interface IFund
    {
        string Name { get; }

        IToken Token { get; }

        decimal CirculatingSupply { get; set; }

        public decimal NetAssetValue => NetAssetValuePerToken * CirculatingSupply;

        public decimal MarketValue => (MarketValuePerToken ?? NetAssetValuePerToken) * CirculatingSupply;

        decimal NetAssetValuePerToken { get; }

        decimal? MarketValuePerToken { get; }

        IReadOnlyList<IAsset> Assets { get; }
    }
}
