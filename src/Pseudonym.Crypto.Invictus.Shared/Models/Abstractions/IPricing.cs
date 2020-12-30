namespace Pseudonym.Crypto.Invictus.Shared.Models.Abstractions
{
    public interface IPricing
    {
        decimal Total { get; }

        decimal PricePerToken { get; }

        decimal DiffDaily { get; }

        decimal DiffWeekly { get; }

        decimal DiffMonthly { get; }
    }
}
