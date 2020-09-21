namespace Pseudonym.Crypto.Investments.Business.Abstractions
{
    public interface IAsset
    {
        string Name { get; }

        string Symbol { get; }

        decimal Value { get; }

        decimal Share { get; }
    }
}
