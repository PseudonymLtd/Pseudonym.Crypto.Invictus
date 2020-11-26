using System.Collections.Generic;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Etherscan;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface IEtherscanClient
    {
        IAsyncEnumerable<EtherscanTransaction> ListTransactionsAsync(EthereumAddress contractAddress, long startblock, long endblock);
    }
}
