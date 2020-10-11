using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Numerics;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Nethereum.JsonRpc.Client;
using Nethereum.Web3;
using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Business.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Business.Models;
using Pseudonym.Crypto.Invictus.TrackerService.Clients.Models;
using Pseudonym.Crypto.Invictus.TrackerService.Ethereum;
using Pseudonym.Crypto.Invictus.TrackerService.Ethereum.Events;
using Pseudonym.Crypto.Invictus.TrackerService.Ethereum.Functions;
using Pseudonym.Crypto.Invictus.TrackerService.Hosting.Models;

namespace Pseudonym.Crypto.Invictus.TrackerService.Clients
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

        public async Task<decimal> GetEthBalanceAsync(EthereumAddress address)
        {
            var balance = await ExecuteAsync(web3 =>
            {
                return web3.Eth.GetBalance.SendRequestAsync(address);
            });

            return Web3.Convert.FromWei(balance.Value);
        }

        public async Task<decimal> GetContractBalanceAsync(EthereumAddress contractAddress, EthereumAddress address)
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

        public async IAsyncEnumerable<EtherTransaction> ListContractTransactionsAsync(EthereumAddress contractAddress, EthereumAddress address)
        {
            var transactions = await ExecuteAsync(web3 =>
            {
                var transferEventHandler = web3.Eth.GetEvent<TransferEvent>(contractAddress);

                var filter = transferEventHandler.CreateFilterInput(null, new[] { address.Address });

                return transferEventHandler.GetAllChanges(filter);
            });

            foreach (var transaction in transactions)
            {
                yield return new EtherTransaction()
                {
                    Sender = new EthereumAddress(transaction.Event.From),
                    Recipient = new EthereumAddress(transaction.Event.To),
                    Amount = Web3.Convert.FromWei(transaction.Event.Value)
                };
            }
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
