using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Models;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Data.Models;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Models;

namespace Pseudonym.Crypto.Invictus.Funds.Business
{
    internal sealed class InvestmentService : AbstractService, IInvestmentService
    {
        private readonly IFundService fundService;
        private readonly IStakeService stakeService;
        private readonly IEtherClient etherClient;
        private readonly ILightstreamClient lightstreamClient;
        private readonly IEthplorerClient ethplorerClient;

        public InvestmentService(
            IOptions<AppSettings> appSettings,
            IFundService fundService,
            IStakeService stakeService,
            IEtherClient etherClient,
            ILightstreamClient lightstreamClient,
            IEthplorerClient ethplorerClient,
            ICurrencyConverter currencyConverter,
            ITransactionRepository transactionRepository,
            IOperationRepository operationRepository,
            IHttpContextAccessor httpContextAccessor,
            IScopedCancellationToken scopedCancellationToken)
            : base(appSettings, currencyConverter, transactionRepository, operationRepository, httpContextAccessor, scopedCancellationToken)
        {
            this.fundService = fundService;
            this.stakeService = stakeService;
            this.etherClient = etherClient;
            this.lightstreamClient = lightstreamClient;
            this.ethplorerClient = ethplorerClient;
        }

        public async IAsyncEnumerable<IInvestment> ListInvestmentsAsync(EthereumAddress address, CurrencyCode currencyCode)
        {
            await foreach (var fund in fundService
                .ListFundsAsync(currencyCode)
                .WithCancellation(CancellationToken))
            {
                var tokenCount = await etherClient.GetContractBalanceAsync(fund.Token.ContractAddress, address, fund.Token.Decimals);
                var stakes = await stakeService.ListStakesAsync(address, fund.Token.Symbol, currencyCode)
                    .ToListAsync(CancellationToken);

                tokenCount += stakes
                    .Where(s => s.ExpiresAt > DateTime.UtcNow)
                    .Sum(s => s.Quantity);

                if (tokenCount > 0)
                {
                    yield return new BusinessInvestment()
                    {
                        Fund = fund,
                        Held = tokenCount
                    };
                }
            }
        }

        public async Task<IInvestment> GetInvestmentAsync(EthereumAddress address, Symbol symbol, CurrencyCode currencyCode)
        {
            var fundInfo = GetFundInfo(symbol);
            var fund = await fundService.GetFundAsync(symbol, currencyCode);
            var tokenCount = await etherClient.GetContractBalanceAsync(fundInfo.Address, address, fund.Token.Decimals);
            var stakes = await stakeService.ListStakesAsync(address, fund.Token.Symbol, currencyCode)
                .ToListAsync(CancellationToken);

            tokenCount += stakes
                .Where(s => s.ExpiresAt > DateTime.UtcNow)
                .Sum(s => s.Quantity);

            var subInvestments = new List<ISubInvestment>();

            foreach (var asset in fund.Assets.Where(a => a.Coin.ContractAddress.HasValue))
            {
                var held = await etherClient.GetContractBalanceAsync(asset.Coin.ContractAddress.Value, address, asset.Coin.Decimals.Value);
                if (held > 0)
                {
                    var marketValuePerToken = asset.Coin.FixedValuePerCoin.HasValue
                        ? asset.Coin.FixedValuePerCoin.Value
                        : (await ethplorerClient.GetTokenInfoAsync(asset.Coin.ContractAddress.Value)).MarketValuePerToken;

                    subInvestments.Add(new BusinessSubInvestment()
                    {
                        Coin = asset.Coin,
                        Held = held,
                        MarketValue = CurrencyConverter.Convert(marketValuePerToken, currencyCode) * held
                    });
                }
            }

            var lightStreamAsset = fund.Assets.SingleOrDefault(a =>
                a.Coin.Name.Equals(nameof(Dependencies.Lightstreams), StringComparison.OrdinalIgnoreCase));

            if (lightStreamAsset != null)
            {
                var held = await lightstreamClient.GetEthBalanceAsync(address);
                if (held > 0)
                {
                    var marketValuePerToken = lightStreamAsset.Coin.FixedValuePerCoin.HasValue
                        ? lightStreamAsset.Coin.FixedValuePerCoin.Value
                        : decimal.Zero;

                    subInvestments.Add(new BusinessSubInvestment()
                    {
                        Coin = lightStreamAsset.Coin,
                        Held = held,
                        MarketValue = CurrencyConverter.Convert(marketValuePerToken, currencyCode) * held
                    });
                }
            }

            return new BusinessInvestment()
            {
                Fund = fund,
                Held = tokenCount,
                SubInvestments = subInvestments,
                Stakes = stakes
            };
        }

        public async IAsyncEnumerable<ITransactionSet> ListTransactionsAsync(EthereumAddress address, Symbol symbol, CurrencyCode currencyCode)
        {
            var fundInfo = GetFundInfo(symbol);

            var transactionHashes = new List<EthereumTransactionHash>();
            var transactions = new List<DataTransaction>();

            await foreach (var transaction in Transactions
                .ListInboundTransactionsAsync(fundInfo.Address, address)
                .WithCancellation(CancellationToken))
            {
                transactions.Add(transaction);
            }

            await foreach (var transaction in Transactions
                .ListOutboundTransactionsAsync(fundInfo.Address, address)
                .WithCancellation(CancellationToken))
            {
                transactions.Add(transaction);
            }

            await foreach (var hash in Operations
                .ListInboundHashesAsync(address, OperationTypes.Transfer)
                .WithCancellation(CancellationToken))
            {
                transactionHashes.Add(hash);
            }

            await foreach (var hash in Operations
                .ListOutboundHashesAsync(address, OperationTypes.Transfer)
                .WithCancellation(CancellationToken))
            {
                transactionHashes.Add(hash);
            }

            foreach (var hash in transactionHashes.Distinct())
            {
                var transaction = transactions
                    .SingleOrDefault(t => t.Address == address && t.Hash == hash)
                        ?? await Transactions.GetTransactionAsync(fundInfo.Address, hash);

                if (transaction != null &&
                    transaction.Address == fundInfo.Address)
                {
                    var businessTransaction = MapTransaction<BusinessTransactionSet>(transaction);

                    var operations = new List<IOperation>();

                    await foreach (var operation in Operations
                        .ListOperationsAsync(hash)
                        .WithCancellation(CancellationToken))
                    {
                        operations.Add(MapOperation(operation, currencyCode));
                    }

                    businessTransaction.Operations = operations;

                    yield return businessTransaction;
                }
            }
        }
    }
}
