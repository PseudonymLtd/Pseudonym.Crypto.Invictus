using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Data.Models;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface IFundPerformanceRepository
    {
        Task<DataFundPerformance> GetPerformanceAsync(EthereumAddress contractAddress, DateTime timeStamp);

        Task UploadItemsAsync(params DataFundPerformance[] perfs);

        IAsyncEnumerable<DataFundPerformance> ListPerformancesAsync(
            EthereumAddress contractAddress,
            DateTime from,
            DateTime to);

        Task<DateTime?> GetLatestDateAsync(EthereumAddress address);

        Task<DateTime?> GetLowestDateAsync(EthereumAddress address);
    }
}
