using System.Collections.Generic;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Data.Models;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface IOperationRepository
    {
        IAsyncEnumerable<DataOperation> ListOperationsAsync(EthereumTransactionHash hash);

        IAsyncEnumerable<EthereumTransactionHash> ListInboundHashesAsync(EthereumAddress contractAddress, EthereumAddress address, string type);

        IAsyncEnumerable<EthereumTransactionHash> ListOutboundHashesAsync(EthereumAddress contractAddress, EthereumAddress address, string type);

        Task UploadOperationAsync(DataOperation operation);
    }
}
