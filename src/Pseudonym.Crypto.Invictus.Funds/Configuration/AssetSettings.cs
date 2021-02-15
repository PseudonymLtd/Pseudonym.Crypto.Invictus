using Pseudonym.Crypto.Invictus.Funds.Configuration.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Configuration
{
    public sealed class AssetSettings : IAssetSettings
    {
        public string Symbol { get; set; }

        public string CoinMarketCap { get; set; }

        public string CoinLore { get; set; }

        public string Colour { get; set; }

        public bool IsUSDStableCoin { get; set; }
    }
}
