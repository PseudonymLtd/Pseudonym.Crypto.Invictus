using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Bloxy;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Ethplorer;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Configuration.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Data.Models;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Services
{
    internal sealed class TransactionCachingService : BaseCachingService
    {
        public TransactionCachingService(
            IOptions<AppSettings> appSettings,
            IServiceProvider serviceProvider)
            : base(appSettings, serviceProvider)
        {
        }

        protected override TimeSpan Interval => TimeSpan.FromHours(1);

        protected override async Task ProcessAsync(IServiceScope scope, CancellationToken cancellationToken)
        {
            var bloxyClient = scope.ServiceProvider.GetRequiredService<IBloxyClient>();
            var ethplorerClient = scope.ServiceProvider.GetRequiredService<IEthplorerClient>();
            var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
            var operationService = scope.ServiceProvider.GetRequiredService<IOperationRepository>();

            var jobs = AppSettings.Funds
                .Cast<IFundSettings>()
                .Select(x => new TransactionJob()
                {
                    Symbol = x.Symbol,
                    ContractAddress = x.ContractAddress,
                    InceptionDate = x.InceptionDate.UtcDateTime
                })
                .Union(AppSettings.Stakes
                    .Cast<IStakeSettings>()
                    .Select(x => new TransactionJob()
                    {
                        Symbol = x.Symbol,
                        ContractAddress = x.ContractAddress,
                        InceptionDate = x.InceptionDate
                    }))
                .ToList();

            foreach (var job in jobs)
            {
                try
                {
                    var latestDate = await transactionService.GetLatestDateAsync(job.ContractAddress)
                        ?? job.InceptionDate;

                    await SyncTransactionsAsync(
                        ethplorerClient,
                        bloxyClient,
                        transactionService,
                        operationService,
                        job.ContractAddress,
                        latestDate.AddDays(-7),
                        DateTime.UtcNow);

                    var lowestDate = await transactionService.GetLowestDateAsync(job.ContractAddress)
                        ?? DateTime.UtcNow;

                    await SyncTransactionsAsync(
                        ethplorerClient,
                        bloxyClient,
                        transactionService,
                        operationService,
                        job.ContractAddress,
                        job.InceptionDate,
                        lowestDate.AddDays(1));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error getting transactions for {job.Symbol}.");
                    Console.WriteLine(e);
                }
            }
        }

        private async Task SyncTransactionsAsync(
            IEthplorerClient ethplorerClient,
            IBloxyClient bloxyClient,
            ITransactionRepository transactionService,
            IOperationRepository operationService,
            EthereumAddress contractAddress,
            DateTime startDate,
            DateTime endDate)
        {
            Console.WriteLine($"[{contractAddress}] Processing Batch: {startDate} -> {endDate}");

            await foreach (var transactionSummary in bloxyClient.ListTransactionsAsync(contractAddress, startDate, endDate))
            {
                var hash = new EthereumTransactionHash(transactionSummary.Hash);

                var transaction = await ethplorerClient.GetTransactionAsync(hash);
                if (transaction != null)
                {
                    Console.WriteLine($"[{contractAddress}] Discovering Hash {hash} with ({transaction.Operations.Count}) operations.");

                    var dynamoTransaction = MapTransaction(hash, contractAddress, transactionSummary, transaction);

                    await transactionService.UploadItemsAsync(dynamoTransaction);

                    var dynamoOperations = Enumerable.Range(0, transaction.Operations.Count)
                        .Select(i => MapOperation(hash, transaction.Operations[i], i))
                        .ToArray();

                    await operationService.UploadItemsAsync(dynamoOperations);
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
                Input = transaction.Input,
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
                Sender = string.IsNullOrWhiteSpace(operation.From)
                    ? EthereumAddress.Empty.Address
                    : operation.From,
                Recipient = string.IsNullOrWhiteSpace(operation.To)
                    ? EthereumAddress.Empty.Address
                    : operation.To,
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

        private class TransactionJob
        {
            public Symbol Symbol { get; set; }

            public EthereumAddress ContractAddress { get; set; }

            public DateTime InceptionDate { get; set; }
        }
    }
}
