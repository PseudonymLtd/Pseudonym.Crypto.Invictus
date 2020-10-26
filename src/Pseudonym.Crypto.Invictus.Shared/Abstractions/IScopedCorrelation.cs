namespace Pseudonym.Crypto.Invictus.Shared.Abstractions
{
    public interface IScopedCorrelation
    {
        string CorrelationId { get; }
    }
}
