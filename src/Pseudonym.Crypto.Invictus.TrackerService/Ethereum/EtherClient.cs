using System;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.JsonRpc.Client;
using Nethereum.Web3;
using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Ethereum.Functions;

namespace Pseudonym.Crypto.Invictus.TrackerService.Ethereum
{
    internal sealed class EtherClient : IEtherClient
    {
        private readonly IWeb3 web3;
        private readonly HttpClient innerClient;

        public EtherClient(Uri baseUri, HttpClient client)
        {
            innerClient = client;
            web3 = new Web3(new RpcClient(baseUri, client, client.DefaultRequestHeaders.Authorization));
        }

        public async Task<decimal> GetEthBalance(EthereumAddress address)
        {
            var balance = await web3.Eth.GetBalance.SendRequestAsync(address);

            return Web3.Convert.FromWei(balance.Value);
        }

        public async Task<decimal> GetContractBalance(EthereumAddress contractAddress, EthereumAddress address)
        {
            var function = new BalanceOfFunction()
            {
                Owner = address,
            };

            var balanceHandler = web3.Eth.GetContractQueryHandler<BalanceOfFunction>();

            var balance = await balanceHandler.QueryAsync<BigInteger>(contractAddress, function);

            return Web3.Convert.FromWei(balance);
        }

        public void Dispose()
        {
            innerClient?.Dispose();
        }
    }
}
