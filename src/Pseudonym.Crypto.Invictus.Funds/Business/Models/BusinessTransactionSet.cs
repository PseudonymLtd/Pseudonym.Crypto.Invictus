using System.Collections.Generic;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal sealed class BusinessTransactionSet : BusinessTransaction, ITransactionSet
    {
        public IReadOnlyList<IOperation> Operations { get; set; } = new List<IOperation>();
    }
}
