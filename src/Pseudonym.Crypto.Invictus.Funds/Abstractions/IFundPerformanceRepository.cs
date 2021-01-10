using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Data.Models;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface IFundPerformanceRepository : IRepository<DataFundPerformance>
    {
        Task<DataFundPerformance> GetPerformanceAsync(EthereumAddress contractAddress, DateTime timeStamp);

        IAsyncEnumerable<DataFundPerformance> ListPerformancesAsync(
            EthereumAddress contractAddress,
            DateTime from,
            DateTime to);

        Task<bool> DeletePerformanceAsync(EthereumAddress contractAddress, DateTime date);

        Task<DateTime?> GetLatestDateAsync(EthereumAddress address);

        Task<DateTime?> GetLowestDateAsync(EthereumAddress address);
    }
}
