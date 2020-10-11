namespace Pseudonym.Crypto.Invictus.TrackerService.Business.Abstractions
{
    public interface IInvestment
    {
        IFund Fund { get; }

        decimal Held { get; }

        decimal RealValue { get; }

        decimal? MarketValue { get; }

        decimal Share { get; }
    }
}
