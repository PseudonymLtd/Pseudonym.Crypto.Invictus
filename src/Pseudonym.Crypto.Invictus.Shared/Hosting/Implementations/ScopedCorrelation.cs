using Pseudonym.Crypto.Invictus.Shared.Abstractions;

namespace Pseudonym.Crypto.Invictus.Shared.Hosting.Models
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
