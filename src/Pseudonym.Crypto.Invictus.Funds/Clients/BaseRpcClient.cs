using System;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Funds.Ethereum.Functions;
using Pseudonym.Crypto.Invictus.Shared.Exceptions;

namespace Pseudonym.Crypto.Invictus.Funds.Clients
{
    internal abstract class BaseRpcClient : IRpcClient
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly string networkName;
        private readonly bool useAuthHeader;

        protected BaseRpcClient(
            IHttpClientFactory httpClientFactory,
            string networkName,
            bool useAuthHeader)
        {
            this.httpClientFactory = httpClientFactory;
            this.networkName = networkName;
            this.useAuthHeader = useAuthHeader;
        }

        public async Task<long> GetCurrentBlockNumberAsync()
        {
            var block = await ExecuteAsync(web3 =>
            {
                return web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            });

            return long.Parse(block.Value.ToString());
        }

        public async Task<decimal> GetEthBalanceAsync(EthereumAddress address)
        {
            var balance = await ExecuteAsync(web3 =>
            {
                return web3.Eth.GetBalance.SendRequestAsync(address);
            });

            return Web3.Convert.FromWei(balance.Value);
        }

        public async Task<decimal> GetContractBalanceAsync(EthereumAddress contractAddress, EthereumAddress address, int decimals)
        {
            var function = new BalanceOfFunction()
            {
                Owner = address,
            };

            var balance = await ExecuteAsync(web3 =>
            {
                var balanceHandler = web3.Eth.GetContractQueryHandler<BalanceOfFunction>();

                return balanceHandler.QueryAsync<BigInteger>(contractAddress, function);
            });

            return Web3.Convert.FromWei(balance, decimals);
        }

        public Task<TransactionReceipt> GetTransactionAsync(EthereumTransactionHash hash)
        {
            return ExecuteAsync(web3 =>
            {
                return web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(hash.Hash);
            });
        }

        private async Task<TResponse> ExecuteAsync<TResponse>(Func<IWeb3, Task<TResponse>> func)
        {
            try
            {
                using var client = httpClientFactory.CreateClient(networkName);

                var web3 = new Web3(new RpcClient(client.BaseAddress, client, useAuthHeader ? client.DefaultRequestHeaders.Authorization : null));

                return await func(web3);
            }
            catch (Exception e)
            {
                throw new TransientException($"{GetType().Name} Error calling web3", e);
            }
        }
    }
}
