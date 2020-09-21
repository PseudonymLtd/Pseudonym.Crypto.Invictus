using System;

namespace Pseudonym.Crypto.Invictus.TrackerService.Business.Abstractions
{
    public interface IPerformance
    {
        DateTime Date { get; }

        decimal NetAssetValuePerToken { get; }

        decimal NetValue { get; }
    }
}
