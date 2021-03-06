﻿using System;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Rpc;
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

        public async Task<ulong> GetCurrentBlockNumberAsync()
        {
            var block = await ExecuteAsync(web3 =>
            {
                return web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            });

            return ulong.Parse(block.Value.ToString());
        }

        public async Task<EthereumBlock> GetBlockAsync(ulong blockNumber)
        {
            var block = await ExecuteAsync(web3 =>
            {
                return web3.Eth.Blocks.GetBlockWithTransactionsHashesByNumber.SendRequestAsync(new BlockParameter(blockNumber));
            });

            var sdasblock = Web3.Convert.FromWei(block.Timestamp.Value, 0);

            return new EthereumBlock()
            {
                Hash = block.BlockHash,
                MinedAt = DateTimeOffset.FromUnixTimeSeconds((long)Web3.Convert.FromWei(block.Timestamp.Value, 0)).UtcDateTime,
                Number = blockNumber,
                Miner = block.Miner
            };
        }

        public async Task<decimal> GetEthBalanceAsync(EthereumAddress address, ulong? blockNumber = null)
        {
            var balance = await ExecuteAsync(web3 =>
            {
                return blockNumber.HasValue
                    ? web3.Eth.GetBalance.SendRequestAsync(address, new BlockParameter(blockNumber.Value))
                    : web3.Eth.GetBalance.SendRequestAsync(address);
            });

            return Web3.Convert.FromWei(balance.Value);
        }

        public async Task<decimal> GetContractBalanceAsync(EthereumAddress contractAddress, EthereumAddress address, int decimals, ulong? blockNumber = null)
        {
            var function = new BalanceOfFunction()
            {
                Owner = address,
            };

            var balance = await ExecuteAsync(web3 =>
            {
                var balanceHandler = web3.Eth.GetContractQueryHandler<BalanceOfFunction>();

                return blockNumber.HasValue
                    ? balanceHandler.QueryAsync<BigInteger>(contractAddress, function, new BlockParameter(blockNumber.Value))
                    : balanceHandler.QueryAsync<BigInteger>(contractAddress, function);
            });

            return Web3.Convert.FromWei(balance, decimals);
        }

        public Task<TFunction> GetDataAsync<TFunction>(EthereumAddress contractAddress, string data)
            where TFunction : class, new()
        {
            return ExecuteAsync(web3 =>
            {
                var outputs = web3.Eth
                    .GetContractHandler(contractAddress)
                    .GetFunction<TFunction>()
                    .DecodeInput(data);

                if (outputs != null)
                {
                    var t = typeof(TFunction);
                    var function = new TFunction();

                    foreach (var prop in t.GetProperties().Where(x => x.CanWrite))
                    {
                        var attr = prop.GetCustomAttribute<ParameterAttribute>(true);
                        if (attr != null)
                        {
                            var output = outputs.SingleOrDefault(x => x.Parameter.Name.Equals(attr.Name, StringComparison.OrdinalIgnoreCase));
                            if (output != null)
                            {
                                prop.SetMethod.Invoke(function, new object[] { output.Result });
                            }
                        }
                    }

                    return Task.FromResult(function);
                }

                return Task.FromResult(default(TFunction));
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
