namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface IFundAsset
    {
        IHolding Holding { get; }

        decimal Quantity { get; }

        decimal PricePerToken { get; }

        decimal Total { get; }

        decimal Share { get; }
    }
}
