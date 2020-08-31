using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;

namespace Pseudonym.Crypto.Invictus.TrackerService.Hosting.Models
{
    internal sealed class ScopedCorrelation : IScopedCorrelation
    {
        public string CorrelationId { get; private set; }

        public void SetCorrelationId(string correlationId)
        {
            CorrelationId = correlationId;
        }
    }
}
