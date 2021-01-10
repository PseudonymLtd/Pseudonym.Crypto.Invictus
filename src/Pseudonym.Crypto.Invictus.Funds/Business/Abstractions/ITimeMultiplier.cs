namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface ITimeMultiplier
    {
        int RangeMin { get; }

        int RangeMax { get; }

        decimal Multiplier { get; }
    }
}
