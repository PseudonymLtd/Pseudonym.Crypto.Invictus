using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Nethereum.Util;
using Nethereum.Web3;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Etherscan;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Ethplorer;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Data.Models;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Services
{
    internal sealed class TransactionCachingService : BackgroundService
    {
        private readonly IOptions<AppSettings> appSettings;
        private readonly IServiceProvider serviceProvider;

        public TransactionCachingService(
            IOptions<AppSettings> appSettings,
            IServiceProvider serviceProvider)
        {
            this.appSettings = appSettings;
            this.serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = serviceProvider.CreateScope(cancellationToken);

                    var etherscanClient = scope.ServiceProvider.GetRequiredService<IEtherscanClient>();
                    var ethplorerClient = scope.ServiceProvider.GetRequiredService<IEthplorerClient>();
                    var etherClient = scope.ServiceProvider.GetRequiredService<IEtherClient>();
                    var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
                    var operationService = scope.ServiceProvider.GetRequiredService<IOperationRepository>();

                    foreach (var fund in appSettings.Value.Funds)
                    {
                        try
                        {
                            var blockNumber = await etherClient.GetCurrentBlockNumberAsync();
                            var latestBlockNumber = await GetStartBlockAsync(transactionService, fund.Address);
                            var lowestBlockNumber = await GetLowestBlockAsync(transactionService, fund.Address);

                            await UpdateTransactionAsync(
                                ethplorerClient,
                                etherscanClient,
                                transactionService,
                                operationService,
                                fund.Address,
                                latestBlockNumber,
                                blockNumber);

                            await UpdateTransactionAsync(
                                ethplorerClient,
                                etherscanClient,
                                transactionService,
                                operationService,
                                fund.Address,
                                lowestBlockNumber,
                                0);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Error getting transactions for {fund.Symbol}.");
                            Console.WriteLine(e);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromHours(1), cancellationToken);
                }
            }
        }

        private async Task UpdateTransactionAsync(
            IEthplorerClient ethplorerClient,
            IEtherscanClient etherscanClient,
            ITransactionRepository transactionService,
            IOperationRepository operationService,
            EthereumAddress contractAddress,
            long startBlock,
            long endBlock)
        {
            await foreach (var transactionSummary in etherscanClient.ListTransactionsAsync(contractAddress, startBlock, endBlock))
            {
                Console.WriteLine($"[{contractAddress}] Processing Batch: {startBlock} -> {endBlock}");

                var hash = new EthereumTransactionHash(transactionSummary.Hash);

                var transaction = await ethplorerClient.GetTransactionAsync(hash);
                if (transaction != null)
                {
                    Console.WriteLine($"[{contractAddress}] Discovering Hash {hash}  with ({transaction.Operations.Count}) operations.");

                    var dynamoTransaction = MapTransaction(hash, contractAddress, transactionSummary, transaction);

                    await transactionService.UploadTransactionAsync(dynamoTransaction);

                    for (var i = 0; i < transaction.Operations.Count; i++)
                    {
                        var dynamoOperation = MapOperation(hash, transaction.Operations[i], i);

                        await operationService.UploadOperationAsync(dynamoOperation);
                    }
                }
            }

            Console.WriteLine($"[{contractAddress}] Finished Batch: {startBlock} -> {endBlock}");
        }

        private DataTransaction MapTransaction(
            EthereumTransactionHash hash,
            EthereumAddress address,
            EtherscanTransaction summary,
            EthplorerTransaction transaction)
        {
            return new DataTransaction()
            {
                Address = address,
                BlockNumber = transaction.BlockNumber,
                Confirmations = transaction.Confirmations,
                Hash = hash,
                ConfirmedAt = long.Parse(summary.UnixTimeStamp).ToDateTime(),
                Sender = string.IsNullOrWhiteSpace(transaction.From)
                    ? EthereumAddress.Empty.Address
                    : transaction.From,
                Recipient = string.IsNullOrWhiteSpace(transaction.To)
                    ? EthereumAddress.Empty.Address
                    : transaction.To,
                Gas = transaction.GasUsed,
                GasLimit = transaction.GasLimit,
                Eth = transaction.Value,
                Success = transaction.Success,
                Input = transaction.Input,
                BlockHash = summary.BlockHash,
                Nonce = long.Parse(summary.Nonce),
                GasPrice = Web3.Convert.FromWei(BigInteger.Parse(summary.GasPrice))
            };
        }

        private DataOperation MapOperation(EthereumTransactionHash hash, EthplorerOperation operation, int order)
        {
            return new DataOperation()
            {
                Hash = hash,
                Order = order,
                Type = operation.Type.ToUpper(),
                Address = operation.Address,
                Sender = operation.From,
                Recipient = operation.To,
                Addresses = operation.Addresses,
                Value = operation.Value,
                Price = operation.Price,
                IsEth = operation.IsEth,
                Priority = operation.Priority,
                ContractAddress = operation.TokenInfo.ContractAddress,
                ContractName = operation.TokenInfo.Name,
                ContractSymbol = operation.TokenInfo.Symbol,
                ContractDecimals = int.Parse(operation.TokenInfo.Decimals),
                ContractHolders = operation.TokenInfo.HolderCount,
                ContractIssuances = operation.TokenInfo.IssuanceCount,
                ContractLink = operation.TokenInfo.WebsiteUri
            };
        }

        private async Task<long> GetStartBlockAsync(ITransactionRepository transactionService, EthereumAddress address)
        {
            var latestBlockNumber = await transactionService.GetLatestBlockNumberAsync(address);

            latestBlockNumber = latestBlockNumber - 10000;

            if (latestBlockNumber < 0)
            {
                latestBlockNumber = 0;
            }

            return latestBlockNumber;
        }

        private async Task<long> GetLowestBlockAsync(ITransactionRepository transactionService, EthereumAddress address)
        {
            var lowestblockNumber = await transactionService.GetLowestBlockNumberAsync(address);

            return lowestblockNumber + 1;
        }
    }
}
