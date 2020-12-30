using System;
using System.Collections.Generic;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface IFund
    {
        string Name { get; }

        string DisplayName { get; }

        string Description { get; }

        Uri InvictusUri { get; }

        Uri FactSheetUri { get; }

        Uri LitepaperUri { get; }

        IToken Token { get; }

        decimal CirculatingSupply { get; set; }

        INav Nav { get; }

        IMarket Market { get; }

        IReadOnlyList<IFundAsset> Assets { get; }
    }
}
