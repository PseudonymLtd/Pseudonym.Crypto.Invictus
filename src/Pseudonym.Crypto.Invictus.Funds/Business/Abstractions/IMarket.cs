namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface IMarket
    {
        bool IsTradable { get; }

        decimal Cap { get; }

        decimal Total { get; }

        decimal PricePerToken { get; }

        decimal DiffDaily { get; }

        decimal DiffWeekly { get; }

        decimal DiffMonthly { get; }

        decimal Volume { get; }

        decimal VolumeDiffDaily { get; }

        decimal VolumeDiffWeekly { get; }

        decimal VolumeDiffMonthly { get; }
    }
}
