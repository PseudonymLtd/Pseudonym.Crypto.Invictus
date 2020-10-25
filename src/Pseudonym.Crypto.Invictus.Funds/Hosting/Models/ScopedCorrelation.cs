using Pseudonym.Crypto.Invictus.Funds.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Hosting.Models
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
