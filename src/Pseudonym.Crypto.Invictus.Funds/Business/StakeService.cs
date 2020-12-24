using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Nethereum.Web3;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Models;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Funds.Ethereum.Functions;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Exceptions;
using Pseudonym.Crypto.Invictus.Shared.Models;

namespace Pseudonym.Crypto.Invictus.Funds.Business
{
    internal sealed class StakeService : AbstractService, IStakeService
    {
        private static readonly IReadOnlyList<Symbol> StakeableSymbols = Enum.GetValues(typeof(Symbol))
            .Cast<Symbol>()
            .Where(x => x != Symbol.ICAP)
            .ToList();

        private readonly IEtherClient etherClient;

        public StakeService(
            IOptions<AppSettings> appSettings,
            IEtherClient etherClient,
            ICurrencyConverter currencyConverter,
            ITransactionRepository transactionRepository,
            IOperationRepository operationRepository,
            IHttpContextAccessor httpContextAccessor,
            IScopedCancellationToken scopedCancellationToken)
            : base(appSettings, currencyConverter, transactionRepository, operationRepository, httpContextAccessor, scopedCancellationToken)
        {
            this.etherClient = etherClient;
        }

        public async IAsyncEnumerable<IStake> ListStakesAsync(CurrencyCode currencyCode)
        {
            foreach (var symbol in StakeableSymbols)
            {
                await foreach (var stake in ListStakesAsync(symbol, currencyCode))
                {
                    yield return stake;
                }
            }
        }

        public async IAsyncEnumerable<IStake> ListStakesAsync(EthereumAddress address, CurrencyCode currencyCode)
        {
            foreach (var symbol in StakeableSymbols)
            {
                await foreach (var stake in ListStakesAsync(address, symbol, currencyCode))
                {
                    yield return stake;
                }
            }
        }

        public async IAsyncEnumerable<IStake> ListStakesAsync(Symbol symbol, CurrencyCode currencyCode)
        {
            var fundInfo = GetFundInfo(symbol);

            await foreach (var transaction in Transactions
                .ListInboundTransactionsAsync(fundInfo.Address, StakingAddress)
                .WithCancellation(CancellationToken))
            {
                var businessTransaction = MapTransaction<BusinessTransaction>(transaction);

                var stake = await GetStakeAsync(null, fundInfo.Address, businessTransaction, currencyCode);
                if (stake != null)
                {
                    yield return stake;
                }
            }
        }

        public async IAsyncEnumerable<IStake> ListStakesAsync(EthereumAddress address, Symbol symbol, CurrencyCode currencyCode)
        {
            var fundInfo = GetFundInfo(symbol);

            await foreach (var transaction in Transactions
                .ListOutboundTransactionsAsync(fundInfo.Address, address, StakingAddress)
                .WithCancellation(CancellationToken))
            {
                var businessTransaction = MapTransaction<BusinessTransaction>(transaction);

                var stake = await GetStakeAsync(address, fundInfo.Address, businessTransaction, currencyCode);
                if (stake != null)
                {
                    yield return stake;
                }
            }
        }

        public Task<IStake> GetStakeAsync(Symbol symbol, EthereumTransactionHash hash, CurrencyCode currencyCode)
        {
            return GetStakeAsync(null, symbol, hash, currencyCode);
        }

        public Task<IStake> GetStakeAsync(EthereumAddress address, Symbol symbol, EthereumTransactionHash hash, CurrencyCode currencyCode)
        {
            return GetStakeAsync(address, symbol, hash, currencyCode);
        }

        public async Task<IStake> GetStakeAsync(EthereumAddress? address, Symbol symbol, EthereumTransactionHash hash, CurrencyCode currencyCode)
        {
            var fundInfo = GetFundInfo(symbol);

            var transaction = await Transactions.GetTransactionAsync(fundInfo.Address, hash);
            if (transaction != null)
            {
                var stake = await GetStakeAsync(address, fundInfo.Address, MapTransaction<BusinessTransaction>(transaction), currencyCode);
                if (stake != null)
                {
                    return stake;
                }
            }

            throw new PermanentException($"No transaction found with hash {hash}{(address.HasValue ? $" for address {address}." : ".")}");
        }

        private async Task<IStake> GetStakeAsync(
            EthereumAddress? address, EthereumAddress contractAddress, ITransaction transaction, CurrencyCode currencyCode)
        {
            var operations = await Operations
                .ListOperationsAsync(transaction.Hash)
                .ToListAsync(CancellationToken);

            var operation = operations.LastOrDefault(x =>
                x.Type == OperationTypes.Transfer &&
                x.Recipient.Equals(StakingAddress, StringComparison.OrdinalIgnoreCase) &&
                (!address.HasValue || x.Sender.Equals(address, StringComparison.OrdinalIgnoreCase)));

            if (operation != null)
            {
                var businessOperation = MapOperation(operation, currencyCode);
                var duration = await GetStakeDurationAsync(contractAddress, transaction.Input);

                return new BusinessStake()
                {
                    Hash = transaction.Hash,
                    ContractAddress = contractAddress,
                    StakedAt = transaction.ConfirmedAt,
                    Duration = duration,
                    ExpiresAt = transaction.ConfirmedAt.Add(duration).Round(),
                    PricePerToken = businessOperation.PricePerToken,
                    Quantity = businessOperation.Quantity
                };
            }

            return null;
        }

        private async Task<TimeSpan> GetStakeDurationAsync(EthereumAddress contractAddress, string data)
        {
            var stake = await etherClient.GetDataAsync<StakeFunction>(contractAddress, data);

            var seconds = (double)Web3.Convert.FromWei(stake.Length, 0);

            return TimeSpan.FromSeconds(seconds);
        }
    }
}
