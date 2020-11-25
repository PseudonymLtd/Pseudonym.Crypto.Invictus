using System.Collections.Generic;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface ITransactionSet : ITransaction
    {
        IReadOnlyList<IOperation> Operations { get; }
    }
}
