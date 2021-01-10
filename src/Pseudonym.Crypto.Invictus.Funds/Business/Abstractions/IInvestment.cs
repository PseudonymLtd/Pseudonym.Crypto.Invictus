using System.Collections.Generic;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface IInvestment
    {
        IFund Fund { get; }

        decimal Held { get; }

        bool Legacy { get; }

        decimal RealValue { get; }

        decimal? MarketValue { get; }

        decimal Share { get; }

        IReadOnlyList<ISubInvestment> SubInvestments { get; }

        IReadOnlyList<IStakeEvent> Stakes { get; }
    }
}
