using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Data.Models;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface IStakingPowerRepository : IRepository<DataStakingPower>
    {
        Task<DataStakingPower> GetStakingPowerAsync(EthereumAddress stakeAddress, DateTime timeStamp);

        IAsyncEnumerable<DataStakingPower> ListStakingPowersAsync(
            EthereumAddress contractAddress,
            DateTime from,
            DateTime to);

        Task<DataStakingPower> GetLatestAsync(EthereumAddress stakeAddress);

        Task<DataStakingPower> GetLowestAsync(EthereumAddress stakeAddress);
    }
}
