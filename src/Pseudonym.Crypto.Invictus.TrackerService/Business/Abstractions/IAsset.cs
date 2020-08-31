namespace Pseudonym.Crypto.Investments.Business.Abstractions
{
    public interface IAsset
    {
        string Name { get; }

        decimal Value { get; }
    }
}
