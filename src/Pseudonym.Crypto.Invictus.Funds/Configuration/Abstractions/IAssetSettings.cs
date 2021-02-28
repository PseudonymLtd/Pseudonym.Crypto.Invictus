using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Configuration.Abstractions
{
    public interface IAssetSettings
    {
        string Symbol { get; }

        string CoinMarketCap { get; }

        string CoinLore { get; }

        string Colour { get; }

        EthereumAddress? PoolAddress { get; }

        bool IsUSDStableCoin { get; }
    }
}
