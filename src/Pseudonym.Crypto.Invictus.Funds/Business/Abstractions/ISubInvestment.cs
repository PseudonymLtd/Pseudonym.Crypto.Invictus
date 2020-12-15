namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface ISubInvestment
    {
        ICoin Coin { get; }

        decimal Held { get; }

        decimal MarketValue { get; }
    }
}
