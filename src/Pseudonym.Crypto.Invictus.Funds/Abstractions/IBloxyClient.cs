using System;
using System.Collections.Generic;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Bloxy;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface IBloxyClient
    {
        IAsyncEnumerable<BloxyTokenTransfer> ListTransactionsAsync(EthereumAddress contractAddress, DateTime startDate, DateTime endDate);
    }
}
