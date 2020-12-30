namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface INav
    {
        decimal Value { get; }

        decimal ValuePerToken { get; }

        decimal DiffDaily { get; }

        decimal DiffWeekly { get; }

        decimal DiffMonthly { get; }
    }
}
