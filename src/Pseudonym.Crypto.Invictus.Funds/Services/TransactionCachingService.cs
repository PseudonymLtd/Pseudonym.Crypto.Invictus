﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Bloxy;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Ethplorer;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Data.Models;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Services
{
    internal sealed class TransactionCachingService : BackgroundService
    {
        private static readonly DateTime InvictusStartDate = new DateTime(2018, 01, 01);

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

                    var bloxyClient = scope.ServiceProvider.GetRequiredService<IBloxyClient>();
                    var ethplorerClient = scope.ServiceProvider.GetRequiredService<IEthplorerClient>();
                    var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
                    var operationService = scope.ServiceProvider.GetRequiredService<IOperationRepository>();

                    foreach (var fund in appSettings.Value.Funds)
                    {
                        try
                        {
                            var latestDate = await transactionService.GetLatestDateAsync(fund.Address)
                                ?? InvictusStartDate;

                            await UpdateTransactionAsync(
                                ethplorerClient,
                                bloxyClient,
                                transactionService,
                                operationService,
                                fund.Address,
                                latestDate.AddDays(-7),
                                DateTime.UtcNow);

                            var lowestDate = await transactionService.GetLowestDateAsync(fund.Address)
                                ?? DateTime.UtcNow;

                            await UpdateTransactionAsync(
                                ethplorerClient,
                                bloxyClient,
                                transactionService,
                                operationService,
                                fund.Address,
                                InvictusStartDate,
                                lowestDate.AddDays(1));
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
            IBloxyClient bloxyClient,
            ITransactionRepository transactionService,
            IOperationRepository operationService,
            EthereumAddress contractAddress,
            DateTime startDate,
            DateTime endDate)
        {
            await foreach (var transactionSummary in bloxyClient.ListTransactionsAsync(contractAddress, startDate, endDate))
            {
                Console.WriteLine($"[{contractAddress}] Processing Batch: {startDate} -> {endDate}");

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

            Console.WriteLine($"[{contractAddress}] Finished Batch: {startDate} -> {endDate}");
        }

        private DataTransaction MapTransaction(
            EthereumTransactionHash hash,
            EthereumAddress address,
            BloxyTokenTransfer summary,
            EthplorerTransaction transaction)
        {
            return new DataTransaction()
            {
                Address = address,
                BlockNumber = transaction.BlockNumber,
                Confirmations = transaction.Confirmations,
                Hash = hash,
                ConfirmedAt = summary.ConfirmedAt.UtcDateTime,
                Sender = string.IsNullOrWhiteSpace(transaction.From)
                    ? EthereumAddress.Empty.Address
                    : transaction.From,
                Recipient = string.IsNullOrWhiteSpace(transaction.To)
                    ? EthereumAddress.Empty.Address
                    : transaction.To,
                Gas = transaction.GasUsed,
                GasLimit = transaction.GasLimit,
                Eth = transaction.Value,
                Success = transaction.Success
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
    }
}
