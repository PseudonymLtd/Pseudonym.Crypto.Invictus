namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface ISubInvestment
    {
        IHolding Holding { get; }

        decimal Held { get; }

        decimal MarketValue { get; }
    }
}
