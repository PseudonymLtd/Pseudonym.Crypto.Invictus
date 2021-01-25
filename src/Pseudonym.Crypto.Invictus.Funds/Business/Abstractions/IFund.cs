using System;
using System.Collections.Generic;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface IFund : IProduct
    {
        Uri LitepaperUri { get; }

        INav Nav { get; }

        IReadOnlyList<IFundAsset> Assets { get; }
    }
}
