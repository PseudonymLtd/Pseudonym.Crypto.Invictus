using System;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface IProduct
    {
        string Name { get; }

        string DisplayName { get; }

        string Category { get; }

        string Description { get; }

        Uri InvictusUri { get; }

        Uri FactSheetUri { get; }

        IToken Token { get; }

        decimal CirculatingSupply { get; }

        IMarket Market { get; }
    }
}
