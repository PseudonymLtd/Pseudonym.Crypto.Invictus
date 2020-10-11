using System;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.JsonRpc.Client;
using Nethereum.Web3;
using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Ethereum.Functions;
using Pseudonym.Crypto.Invictus.TrackerService.Hosting.Models;

namespace Pseudonym.Crypto.Invictus.TrackerService.Ethereum
{
    internal sealed class EtherClient : IEtherClient
    {
        private readonly IScopedCorrelation scopedCorrelation;
        private readonly IHttpClientFactory httpClientFactory;

        public EtherClient(
            IScopedCorrelation scopedCorrelation,
            IHttpClientFactory httpClientFactory)
        {
            this.scopedCorrelation = scopedCorrelation;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<decimal> GetEthBalance(EthereumAddress address)
        {
            var balance = await ExecuteAsync(web3 =>
            {
                return web3.Eth.GetBalance.SendRequestAsync(address);
            });

            return Web3.Convert.FromWei(balance.Value);
        }

        public async Task<decimal> GetContractBalance(EthereumAddress contractAddress, EthereumAddress address)
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

            return Web3.Convert.FromWei(balance);
        }

        private async Task<TResponse> ExecuteAsync<TResponse>(Func<IWeb3, Task<TResponse>> func)
        {
            using var client = httpClientFactory.CreateClient(nameof(EtherClient));

            client.DefaultRequestHeaders.TryAddWithoutValidation(Headers.CorrelationId, scopedCorrelation.CorrelationId);

            var web3 = new Web3(new RpcClient(client.BaseAddress, client, client.DefaultRequestHeaders.Authorization));

            return await func(web3);
        }
    }
}
