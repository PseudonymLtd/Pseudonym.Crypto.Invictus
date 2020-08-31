namespace Pseudonym.Crypto.Invictus.TrackerService.Abstractions
{
    public interface IScopedCorrelation
    {
        string CorrelationId { get; }
    }
}
