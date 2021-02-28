using System.Text.Json.Serialization;
using Pseudonym.Crypto.Invictus.Funds.Configuration.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Configuration
{
    public sealed class AssetSettings : IAssetSettings
    {
        public string Symbol { get; set; }

        public string CoinMarketCap { get; set; }

        public string CoinLore { get; set; }

        public string Colour { get; set; }

        public string PoolAddress { get; set; }

        public bool IsUSDStableCoin { get; set; }

        [JsonIgnore]
        EthereumAddress? IAssetSettings.PoolAddress => !string.IsNullOrWhiteSpace(PoolAddress)
            ? new EthereumAddress(PoolAddress)
            : default(EthereumAddress?);
    }
}
