using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
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

                    var cypherClient = scope.ServiceProvider.GetRequiredService<IBlockCypherClient>();
                    var ethplorerClient = scope.ServiceProvider.GetRequiredService<IEthplorerClient>();
                    var etherClient = scope.ServiceProvider.GetRequiredService<IEtherClient>();
                    var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
                    var operationService = scope.ServiceProvider.GetRequiredService<IOperationRepository>();

                    foreach (var fund in appSettings.Value.Funds)
                    {
                        try
                        {
                            var blockNumber = await etherClient.GetCurrentBlockNumberAsync();
                            var latestBlockNumber = await GetStopBlockAsync(transactionService, fund.Address);
                            var lowestBlockNumber = await GetLowestBlockAsync(transactionService, fund.Address);

                            await UpdateTransactionAsync(
                                ethplorerClient,
                                cypherClient,
                                transactionService,
                                operationService,
                                fund.Address,
                                fund.Decimals,
                                blockNumber,
                                latestBlockNumber);

                            await UpdateTransactionAsync(
                                ethplorerClient,
                                cypherClient,
                                transactionService,
                                operationService,
                                fund.Address,
                                fund.Decimals,
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
            IBlockCypherClient cypherClient,
            ITransactionRepository transactionService,
            IOperationRepository operationService,
            EthereumAddress contractAddress,
            int decimals,
            long beforeBlock,
            long afterBlock)
        {
            var response = await cypherClient.GetAddressInformationAsync(contractAddress, beforeBlock, afterBlock);
            if (response.Transactions.Any())
            {
                Console.WriteLine($"[{contractAddress}] Processing Batch ({response.Transactions.Count}) Range: {beforeBlock} -> {afterBlock}");

                foreach (var transactionSummary in response.Transactions)
                {
                    var hash = new EthereumTransactionHash(transactionSummary.Hash);

                    var transaction = await ethplorerClient.GetTransactionAsync(hash);
                    if (transaction != null)
                    {
                        Console.WriteLine($"[{contractAddress}] Discovering Hash {hash} with ({transaction.Operations.Count}) operations.");

                        var dynamoTransaction = MapTransaction(hash, contractAddress, transactionSummary.ConfirmedAt, transaction);

                        await transactionService.UploadTransactionAsync(dynamoTransaction);

                        for (var i = 0; i < transaction.Operations.Count; i++)
                        {
                            var dynamoOperation = MapOperation(hash, transaction.Operations[i], i);

                            await operationService.UploadOperationAsync(dynamoOperation);
                        }
                    }
                }

                if (response.HasMoreTransactions)
                {
                    var minBlock = response.Transactions.Select(t => t.BlockNumber).Min();

                    await UpdateTransactionAsync(
                        ethplorerClient,
                        cypherClient,
                        transactionService,
                        operationService,
                        contractAddress,
                        decimals,
                        minBlock,
                        afterBlock);
                }
                else
                {
                    Console.WriteLine($"[{contractAddress}] Finished Processing.");
                }
            }
        }

        private DataTransaction MapTransaction(
            EthereumTransactionHash hash,
            EthereumAddress address,
            DateTime confirmedAt,
            EthplorerTransaction transaction)
        {
            return new DataTransaction()
            {
                Address = address,
                BlockNumber = transaction.BlockNumber,
                Confirmations = transaction.Confirmations,
                Hash = hash,
                ConfirmedAt = confirmedAt,
                Sender = transaction.From,
                Recipient = transaction.To,
                GasUsed = transaction.GasUsed,
                GasLimit = transaction.GasLimit,
                Eth = transaction.Value,
                Success = transaction.Success,
                Input = transaction.Input
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

        private async Task<long> GetStopBlockAsync(ITransactionRepository transactionService, EthereumAddress address)
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
