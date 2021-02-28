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

namespace Pseudonym.Crypto.Invictus.Funds.Business
{
    internal sealed class InvestmentService : AbstractService, IInvestmentService
    {
        private readonly IFundService fundService;
        private readonly IStakeService stakeService;
        private readonly IEtherClient etherClient;
        private readonly ILightstreamClient lightstreamClient;

        public InvestmentService(
            IOptions<AppSettings> appSettings,
            IFundService fundService,
            IStakeService stakeService,
            IEtherClient etherClient,
            ILightstreamClient lightstreamClient,
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

            await foreach (var stake in stakeService
                .ListStakesAsync(currencyCode)
                .WithCancellation(CancellationToken))
            {
                var tokenCount = await etherClient.GetContractBalanceAsync(stake.Token.ContractAddress, address, stake.Token.Decimals);
                if (tokenCount > 0)
                {
                    yield return new BusinessInvestment()
                    {
                        Fund = MapStakeToFund(stake),
                        Held = tokenCount
                    };
                }
            }
        }

        public async Task<IInvestment> GetInvestmentAsync(EthereumAddress address, Symbol symbol, CurrencyCode currencyCode)
        {
            if (symbol.IsFund())
            {
                var fund = await fundService.GetFundAsync(symbol, currencyCode);
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

                var subInvestments = new List<ISubInvestment>();

                foreach (var asset in fund.Assets)
                {
                    var held = asset.Holding.ContractAddress.HasValue
                        ? await etherClient.GetContractBalanceAsync(asset.Holding.ContractAddress.Value, address, asset.Holding.Decimals.Value)
                        : asset.Holding.Symbol == "PHT"
                            ? await lightstreamClient.GetEthBalanceAsync(address)
                            : decimal.Zero;

                    if (held > decimal.Zero)
                    {
                        subInvestments.Add(new BusinessSubInvestment()
                        {
                            Holding = asset.Holding,
                            Held = held,
                            MarketValue = CurrencyConverter.Convert(asset.PricePerToken, currencyCode) * held
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
            else
            {
                var stake = await stakeService.GetStakeAsync(symbol, currencyCode);

                var tokenCount = await etherClient.GetContractBalanceAsync(stake.Token.ContractAddress, address, stake.Token.Decimals);

                return new BusinessInvestment()
                {
                    Fund = MapStakeToFund(stake),
                    Held = tokenCount
                };
            }
        }

        public async IAsyncEnumerable<ITransactionSet> ListTransactionsAsync(EthereumAddress address, Symbol symbol, CurrencyCode currencyCode)
        {
            var contractAddress = symbol.IsFund()
                ? GetFundInfo(symbol).ContractAddress
                : GetStakeInfo(symbol).ContractAddress;

            var transactionHashes = new List<EthereumTransactionHash>();
            var transactions = new List<DataTransaction>();

            await foreach (var transaction in Transactions
                .ListInboundTransactionsAsync(contractAddress, address)
                .WithCancellation(CancellationToken))
            {
                transactions.Add(transaction);
            }

            await foreach (var transaction in Transactions
                .ListOutboundTransactionsAsync(contractAddress, address)
                .WithCancellation(CancellationToken))
            {
                transactions.Add(transaction);
            }

            await foreach (var hash in Operations
                .ListInboundHashesAsync(address, contractAddress)
                .WithCancellation(CancellationToken))
            {
                transactionHashes.Add(hash);
            }

            await foreach (var hash in Operations
                .ListOutboundHashesAsync(address, contractAddress)
                .WithCancellation(CancellationToken))
            {
                transactionHashes.Add(hash);
            }

            foreach (var hash in transactionHashes.Distinct())
            {
                var transaction = transactions
                    .SingleOrDefault(t => t.Address == contractAddress && t.Hash == hash)
                        ?? await Transactions.GetTransactionAsync(contractAddress, hash);

                if (transaction != null &&
                    transaction.Address == contractAddress)
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

        private IFund MapStakeToFund(IStake stake)
        {
            return new BusinessFund()
            {
                Name = stake.Name,
                Category = stake.Category,
                Description = stake.Description,
                InvictusUri = stake.InvictusUri,
                FactSheetUri = stake.FactSheetUri,
                LitepaperUri = stake.PoolUri,
                Market = stake.Market,
                Nav = new BusinessNav()
                {
                    Value = stake.Market.Total,
                    ValuePerToken = stake.Market.PricePerToken,
                    DiffDaily = stake.Market.DiffDaily,
                    DiffWeekly = stake.Market.DiffWeekly,
                    DiffMonthly = stake.Market.DiffMonthly
                },
                Token = stake.Token,
                CirculatingSupply = stake.CirculatingSupply
            };
        }
    }
}
