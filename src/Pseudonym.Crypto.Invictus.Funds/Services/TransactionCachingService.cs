using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Models;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Ethplorer;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
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
                    var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
                    var operationService = scope.ServiceProvider.GetRequiredService<IOperationService>();

                    foreach (var fund in appSettings.Value.Funds)
                    {
                        try
                        {
                            var contractAddress = new EthereumAddress(fund.ContractAddress);
                            var blockNumber = await etherClient.GetCurrentBlockNumberAsync();
                            var lastblockNumber = await GetStopBlockAsync(transactionService, contractAddress);

                            await UpdateTransactionAsync(
                                ethplorerClient,
                                cypherClient,
                                transactionService,
                                operationService,
                                contractAddress,
                                fund.Decimals,
                                blockNumber,
                                lastblockNumber);
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
                    await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
                }
            }
        }

        private async Task UpdateTransactionAsync(
            IEthplorerClient ethplorerClient,
            IBlockCypherClient cypherClient,
            ITransactionService transactionService,
            IOperationService operationService,
            EthereumAddress contractAddress,
            int decimals,
            long beforeBlock,
            long afterBlock)
        {
            var response = await cypherClient.GetAddressInformationAsync(contractAddress, beforeBlock, afterBlock);
            if (response.Transactions.Any())
            {
                foreach (var transactionSummary in response.Transactions)
                {
                    var hash = new EthereumTransactionHash(transactionSummary.Hash);

                    var transaction = await ethplorerClient.GetTransactionAsync(hash);
                    if (transaction != null)
                    {
                        var businessTransaction = MapTransaction(hash, contractAddress, transactionSummary.ConfirmedAt, transaction);

                        await transactionService.UploadTransactionAsync(businessTransaction);

                        foreach (var operation in businessTransaction.Operations)
                        {
                            await operationService.UploadOperationAsync(operation);
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
            }
        }

        private ITransaction MapTransaction(
            EthereumTransactionHash hash,
            EthereumAddress address,
            DateTime confirmedAt,
            EthplorerTransaction transaction)
        {
            return new BusinessTransaction()
            {
                Address = address,
                BlockNumber = transaction.BlockNumber,
                Confirmations = transaction.Confirmations,
                Hash = hash,
                ConfirmedAt = confirmedAt,
                Sender = string.IsNullOrWhiteSpace(transaction.From)
                    ? EthereumAddress.Empty
                    : new EthereumAddress(transaction.From),
                Recipient = string.IsNullOrWhiteSpace(transaction.To)
                    ? EthereumAddress.Empty
                    : new EthereumAddress(transaction.To),
                GasUsed = transaction.GasUsed,
                GasLimit = transaction.GasLimit,
                EthValue = transaction.Value,
                Success = transaction.Success,
                Input = transaction.Input,
                Operations = transaction.Operations
                    .Select(o => new BusinessOperation()
                    {
                        Hash = hash,
                        Type = o.Type.ToUpper(),
                        Address = string.IsNullOrEmpty(o.Address)
                            ? EthereumAddress.Empty
                            : new EthereumAddress(o.Address),
                        Sender = string.IsNullOrEmpty(o.From)
                            ? EthereumAddress.Empty
                            : new EthereumAddress(o.From),
                        Recipient = string.IsNullOrEmpty(o.To)
                            ? EthereumAddress.Empty
                            : new EthereumAddress(o.To),
                        Addresses = o.Addresses
                            .Select(a => new EthereumAddress(a))
                            .ToList(),
                        Value = o.Value,
                        Price = o.Price,
                        IsEth = o.IsEth,
                        Priority = o.Priority,
                        ContractAddress = new EthereumAddress(o.TokenInfo.ContractAddress),
                        ContractName = o.TokenInfo.Name,
                        ContractSymbol = o.TokenInfo.Symbol,
                        ContractDecimals = int.Parse(o.TokenInfo.Decimals),
                        ContractHolders = o.TokenInfo.HolderCount,
                        ContractIssuances = o.TokenInfo.IssuanceCount,
                        ContractLink = string.IsNullOrEmpty(o.TokenInfo.WebsiteUri)
                                ? null
                                : new Uri(o.TokenInfo.WebsiteUri, UriKind.Absolute)
                    })
                    .ToList()
            };
        }

        private async Task<long> GetStopBlockAsync(ITransactionService transactionService, EthereumAddress address)
        {
            return 0;

            var lastblockNumber = await transactionService.GetLastBlockNumberAsync(address);

            lastblockNumber = lastblockNumber - 10000;

            if (lastblockNumber < 0)
            {
                lastblockNumber = 0;
            }

            return lastblockNumber;
        }
    }
}
