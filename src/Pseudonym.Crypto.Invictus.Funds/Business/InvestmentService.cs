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

                var stakeEvents = new List<IStakeEvent>();

                foreach (var stake in GetStakes())
                {
                    stakeEvents.AddRange(
                        await stakeService
                            .ListStakeEventsAsync(stake.Symbol, address, fund.Token.Symbol)
                            .ToListAsync(CancellationToken));
                }

                tokenCount += stakeEvents.Sum(s => s.Change);

                if (tokenCount > 0)
                {
                    yield return new BusinessInvestment()
                    {
                        Fund = fund,
                        Held = tokenCount
                    };
                }
                else
                {
                    var isLegacy = false;

                    await foreach (var transaction in Transactions
                        .ListInboundTransactionsAsync(fund.Token.ContractAddress, address)
                        .WithCancellation(CancellationToken))
                    {
                        isLegacy = true;
                        break;
                    }

                    if (!isLegacy)
                    {
                        await foreach (var transaction in Transactions
                            .ListOutboundTransactionsAsync(fund.Token.ContractAddress, address)
                            .WithCancellation(CancellationToken))
                        {
                            isLegacy = true;
                            break;
                        }
                    }

                    if (isLegacy)
                    {
                        yield return new BusinessInvestment()
                        {
                            Fund = fund,
                            Held = decimal.Zero,
                            Legacy = isLegacy
                        };
                    }
                }
            }
        }

        public async Task<IInvestment> GetInvestmentAsync(EthereumAddress address, Symbol symbol, CurrencyCode currencyCode)
        {
            var fundInfo = GetFundInfo(symbol);
            var fund = await fundService.GetFundAsync(symbol, currencyCode);
            var tokenCount = await etherClient.GetContractBalanceAsync(fundInfo.ContractAddress, address, fund.Token.Decimals);
            var stakeEvents = new List<IStakeEvent>();

            foreach (var stake in GetStakes())
            {
                stakeEvents.AddRange(
                    await stakeService
                        .ListStakeEventsAsync(stake.Symbol, address, fund.Token.Symbol)
                        .ToListAsync(CancellationToken));
            }

            tokenCount += stakeEvents.Sum(s => s.Change);

            var subInvestments = new List<ISubInvestment>();

            foreach (var asset in fund.Assets.Where(a => a.Holding.ContractAddress.HasValue))
            {
                var held = await etherClient.GetContractBalanceAsync(asset.Holding.ContractAddress.Value, address, asset.Holding.Decimals.Value);
                if (held > 0)
                {
                    var marketValuePerToken = asset.Holding.FixedValuePerCoin.HasValue
                        ? asset.Holding.FixedValuePerCoin.Value
                        : (await ethplorerClient.GetTokenInfoAsync(asset.Holding.ContractAddress.Value)).Price?.MarketValuePerToken;

                    subInvestments.Add(new BusinessSubInvestment()
                    {
                        Holding = asset.Holding,
                        Held = held,
                        MarketValue = (CurrencyConverter.Convert(marketValuePerToken, currencyCode) ?? decimal.Zero) * held
                    });
                }
            }

            var lightStreamAsset = fund.Assets.SingleOrDefault(a =>
                a.Holding.Name.Equals(nameof(Dependencies.Lightstreams), StringComparison.OrdinalIgnoreCase));

            if (lightStreamAsset != null)
            {
                var held = await lightstreamClient.GetEthBalanceAsync(address);
                if (held > 0)
                {
                    var marketValuePerToken = lightStreamAsset.Holding.FixedValuePerCoin.HasValue
                        ? lightStreamAsset.Holding.FixedValuePerCoin.Value
                        : decimal.Zero;

                    subInvestments.Add(new BusinessSubInvestment()
                    {
                        Holding = lightStreamAsset.Holding,
                        Held = held,
                        MarketValue = CurrencyConverter.Convert(marketValuePerToken, currencyCode) * held
                    });
                }
            }

            return new BusinessInvestment()
            {
                Fund = fund,
                Held = tokenCount,
                Legacy = tokenCount == default,
                SubInvestments = subInvestments,
                Stakes = stakeEvents
            };
        }

        public async IAsyncEnumerable<ITransactionSet> ListTransactionsAsync(EthereumAddress address, Symbol symbol, CurrencyCode currencyCode)
        {
            var fundInfo = GetFundInfo(symbol);

            var transactionHashes = new List<EthereumTransactionHash>();
            var transactions = new List<DataTransaction>();

            await foreach (var transaction in Transactions
                .ListInboundTransactionsAsync(fundInfo.ContractAddress, address)
                .WithCancellation(CancellationToken))
            {
                transactions.Add(transaction);
            }

            await foreach (var transaction in Transactions
                .ListOutboundTransactionsAsync(fundInfo.ContractAddress, address)
                .WithCancellation(CancellationToken))
            {
                transactions.Add(transaction);
            }

            await foreach (var hash in Operations
                .ListInboundHashesAsync(address, fundInfo.ContractAddress)
                .WithCancellation(CancellationToken))
            {
                transactionHashes.Add(hash);
            }

            await foreach (var hash in Operations
                .ListOutboundHashesAsync(address, fundInfo.ContractAddress)
                .WithCancellation(CancellationToken))
            {
                transactionHashes.Add(hash);
            }

            foreach (var hash in transactionHashes.Distinct())
            {
                var transaction = transactions
                    .SingleOrDefault(t => t.Address == fundInfo.ContractAddress && t.Hash == hash)
                        ?? await Transactions.GetTransactionAsync(fundInfo.ContractAddress, hash);

                if (transaction != null &&
                    transaction.Address == fundInfo.ContractAddress)
                {
                    var businessTransaction = MapTransaction<BusinessTransactionSet>(transaction);

                    var operations = await Operations
                        .ListOperationsAsync(hash)
                        .ToListAsync(CancellationToken);

                    businessTransaction.Operations = operations
                        .Select(o => MapOperation(o, currencyCode))
                        .ToList();

                    yield return businessTransaction;
                }
            }
        }
    }
}
