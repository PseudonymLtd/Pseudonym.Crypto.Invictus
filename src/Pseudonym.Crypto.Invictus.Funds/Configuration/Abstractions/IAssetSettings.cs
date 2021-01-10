namespace Pseudonym.Crypto.Invictus.Funds.Configuration.Abstractions
{
    public interface IAssetSettings
    {
        string Symbol { get; }

        string CoinMarketCap { get; }

        string CoinLore { get; }

        string Colour { get; }
    }
}
