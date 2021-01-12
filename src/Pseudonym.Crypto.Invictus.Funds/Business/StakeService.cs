using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Nethereum.Web3;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Models;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Configuration.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Data.Models;
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
        private readonly IEtherClient etherClient;
        private readonly IGraphClient graphClient;
        private readonly IEthplorerClient ethplorerClient;
        private readonly IFundService fundService;
        private readonly IStakingPowerRepository stakingPowerRepository;

        public StakeService(
            IOptions<AppSettings> appSettings,
            IEtherClient etherClient,
            IGraphClient graphClient,
            IEthplorerClient ethplorerClient,
            IFundService fundService,
            ICurrencyConverter currencyConverter,
            IStakingPowerRepository stakingPowerRepository,
            ITransactionRepository transactionRepository,
            IOperationRepository operationRepository,
            IHttpContextAccessor httpContextAccessor,
            IScopedCancellationToken scopedCancellationToken)
            : base(appSettings, currencyConverter, transactionRepository, operationRepository, httpContextAccessor, scopedCancellationToken)
        {
            this.etherClient = etherClient;
            this.graphClient = graphClient;
            this.ethplorerClient = ethplorerClient;
            this.fundService = fundService;
            this.stakingPowerRepository = stakingPowerRepository;
        }

        public async IAsyncEnumerable<IStake> ListStakesAsync(CurrencyCode currencyCode)
        {
            foreach (var stakeAddress in GetStakes().Select(x => x.Symbol))
            {
                yield return await GetStakeAsync(stakeAddress, currencyCode);
            }
        }

        public async Task<IStake> GetStakeAsync(Symbol stakeSymbol, CurrencyCode currencyCode)
        {
            var stakeInfo = GetStakeInfo(stakeSymbol);

            var stakePower = await stakingPowerRepository.GetLatestAsync(stakeInfo.ContractAddress);
            var tokenInfo = await ethplorerClient.GetTokenInfoAsync(stakeInfo.ContractAddress);
            var tokenPair = await graphClient.GetUniswapPairAsync(stakeInfo.PoolAddress);

            var circulatingSupply = tokenInfo.TotalSupply.FromBigInteger(int.Parse(tokenInfo.Decimals));
            var token = tokenPair.Tokens
                .First(x => !x.Symbol.Equals(stakeInfo.Symbol.ToString(), StringComparison.OrdinalIgnoreCase));

            var now = DateTime.UtcNow;

            var priceHistory = await fundService.ListPerformanceAsync(stakeSymbol, PriceMode.Raw, now.AddDays(-29), now, currencyCode)
                .ToListAsync(CancellationToken);

            var dailyNavs = priceHistory.TakeLast(2);
            var weeklyNavs = priceHistory.TakeLast(8);

            return new BusinessStake()
            {
                Name = stakeInfo.Name,
                Description = stakeInfo.Description,
                InvictusUri = stakeInfo.Links.External,
                FactSheetUri = stakeInfo.Links.Fact,
                PoolUri = new Uri(string.Format(PoolTemplate, stakeInfo.PoolAddress), UriKind.Absolute),
                Token = new BusinessToken()
                {
                    Symbol = stakeInfo.Symbol,
                    ContractAddress = stakeInfo.ContractAddress,
                    Decimals = stakeInfo.Decimals
                },
                CirculatingSupply = circulatingSupply,
                Market = new BusinessMarket()
                {
                    IsTradable = true,
                    Cap = CurrencyConverter.Convert(priceHistory.Last().MarketCap.Value, currencyCode),
                    PricePerToken = CurrencyConverter.Convert(token.PricePerToken, currencyCode),
                    Total = CurrencyConverter.Convert(circulatingSupply * token.PricePerToken, currencyCode),
                    DiffDaily = dailyNavs.First().MarketAssetValuePerToken.Value.PercentageDiff(dailyNavs.Last().MarketAssetValuePerToken.Value),
                    DiffWeekly = weeklyNavs.First().MarketAssetValuePerToken.Value.PercentageDiff(weeklyNavs.Last().MarketAssetValuePerToken.Value),
                    DiffMonthly = priceHistory.First().MarketAssetValuePerToken.Value.PercentageDiff(priceHistory.Last().MarketAssetValuePerToken.Value),
                    Volume = CurrencyConverter.Convert(tokenPair.Volume, currencyCode),
                    VolumeDiffDaily = dailyNavs.First().Volume.Value.PercentageDiff(dailyNavs.Last().Volume.Value),
                    VolumeDiffWeekly = weeklyNavs.First().Volume.Value.PercentageDiff(weeklyNavs.Last().Volume.Value),
                    VolumeDiffMonthly = priceHistory.First().Volume.Value.PercentageDiff(priceHistory.Last().Volume.Value),
                },
                StakingAddress = stakeInfo.StakingAddress,
                FundMultipliers = stakeInfo.FundMultipliers,
                TimeMultipliers = stakeInfo.TimeMultipliers
                    .Select(tm => new BusinessTimeMultiplier()
                    {
                        RangeMin = tm.RangeMin,
                        RangeMax = tm.RangeMax,
                        Multiplier = tm.Multiplier
                    })
                    .ToList(),
                Power = MapStakingPower(stakeInfo, stakePower, currencyCode)
            };
        }

        public async IAsyncEnumerable<IStakingPower> ListStakePowersAsync(Symbol stakeSymbol, PriceMode priceMode, DateTime from, DateTime to, CurrencyCode currencyCode)
        {
            var stakeInfo = GetStakeInfo(stakeSymbol);

            var start = from < stakeInfo.InceptionDate
                ? stakeInfo.InceptionDate
                : from;

            if (to <= stakeInfo.InceptionDate)
            {
                yield break;
            }

            if (priceMode == PriceMode.Raw)
            {
                await foreach (var stakePower in stakingPowerRepository
                    .ListStakingPowersAsync(stakeInfo.ContractAddress, start, to)
                    .WithCancellation(CancellationToken))
                {
                    yield return MapStakingPower(stakeInfo, stakePower, currencyCode);
                }

                yield break;
            }

            var stakePowers = await stakingPowerRepository
                .ListStakingPowersAsync(stakeInfo.ContractAddress, start, to)
                .ToListAsync(CancellationToken);

            foreach (var stakePowerGroup in stakePowers.GroupBy(p => p.Date.Date).OrderBy(x => x.Key))
            {
                var mapped = stakePowerGroup
                    .Select(sp => MapStakingPower(stakeInfo, sp, currencyCode))
                    .ToList();

                yield return new BusinessStakingPower()
                {
                    Symbol = stakeInfo.Symbol,
                    Date = stakePowerGroup.Key,
                    Power = CurrencyConverter.Convert(Aggregate(stakePowerGroup, x => x.Power, priceMode) ?? decimal.Zero, currencyCode),
                    Summary = mapped.Last().Summary
                        .Select(fs =>
                        {
                            var fund = GetFunds().Single(x => x.Symbol == fs.Symbol);

                            return new BusinessStakingPowerSummary()
                            {
                                Symbol = fund.Symbol,
                                Power = CurrencyConverter.Convert(
                                    Aggregate(
                                         stakePowerGroup,
                                         x => x.Summary
                                            .SingleOrDefault(y => y.ContractAddress.Equals(fund.ContractAddress.Address, StringComparison.OrdinalIgnoreCase))
                                            ?.Power ?? decimal.Zero,
                                         priceMode),
                                    currencyCode)
                                        ?? decimal.Zero
                            };
                        })
                        .ToList()
                    ?? new List<BusinessStakingPowerSummary>()
                };
            }
        }

        public async IAsyncEnumerable<IStakeEvent> ListStakeEventsAsync(Symbol stakeSymbol, DateTime from, DateTime to)
        {
            var stakeInfo = GetStakeInfo(stakeSymbol);

            foreach (var symbol in stakeInfo.FundMultipliers.Keys)
            {
                await foreach (var stake in ListStakeEventsAsync(stakeSymbol, symbol, from, to))
                {
                    yield return stake;
                }
            }
        }

        public async IAsyncEnumerable<IStakeEvent> ListStakeEventsAsync(Symbol stakeSymbol, EthereumAddress address)
        {
            var stakeInfo = GetStakeInfo(stakeSymbol);

            foreach (var symbol in stakeInfo.FundMultipliers.Keys)
            {
                await foreach (var stake in ListStakeEventsAsync(stakeSymbol, address, symbol))
                {
                    yield return stake;
                }
            }
        }

        public async IAsyncEnumerable<IStakeEvent> ListStakeEventsAsync(Symbol stakeSymbol, Symbol symbol, DateTime from, DateTime to)
        {
            var fundInfo = GetFundInfo(symbol);
            var stakeInfo = GetStakeInfo(stakeSymbol);

            var stakeEvents = new List<IStakeEvent>();

            await foreach (var transaction in Transactions
                .ListInboundTransactionsAsync(fundInfo.ContractAddress, stakeInfo.StakingAddress, from: from, to: to)
                .WithCancellation(CancellationToken))
            {
                var businessTransaction = MapTransaction<BusinessTransaction>(transaction);

                var stake = await GetStakeLockupAsync(fundInfo, stakeInfo, businessTransaction)
                    ?? await GetStakeReleaseAsync(fundInfo, stakeInfo, businessTransaction);

                if (stake != null)
                {
                    stakeEvents.Add(stake);
                }
            }

            await foreach (var transaction in Transactions
                .ListOutboundTransactionsAsync(fundInfo.ContractAddress, stakeInfo.StakingAddress, from: from, to: to)
                .WithCancellation(CancellationToken))
            {
                var businessTransaction = MapTransaction<BusinessTransaction>(transaction);

                var stake = await GetStakeReleaseAsync(fundInfo, stakeInfo, businessTransaction);
                if (stake != null)
                {
                    stakeEvents.Add(stake);
                }
            }

            foreach (var stakeEvent in stakeEvents.OrderBy(x => x.ConfirmedAt))
            {
                yield return stakeEvent;
            }
        }

        public async IAsyncEnumerable<IStakeEvent> ListStakeEventsAsync(Symbol stakeSymbol, EthereumAddress address, Symbol symbol)
        {
            var fundInfo = GetFundInfo(symbol);
            var stakeInfo = GetStakeInfo(stakeSymbol);

            var stakeEvents = new List<IStakeEvent>();

            await foreach (var transaction in Transactions
                .ListOutboundTransactionsAsync(fundInfo.ContractAddress, address, stakeInfo.StakingAddress)
                .WithCancellation(CancellationToken))
            {
                var businessTransaction = MapTransaction<BusinessTransaction>(transaction);

                var stake = await GetStakeLockupAsync(fundInfo, stakeInfo, businessTransaction)
                    ?? await GetStakeReleaseAsync(fundInfo, stakeInfo, businessTransaction);

                if (stake != null)
                {
                    stakeEvents.Add(stake);
                }
            }

            await foreach (var transaction in Transactions
                .ListInboundTransactionsAsync(fundInfo.ContractAddress, address, stakeInfo.StakingAddress)
                .WithCancellation(CancellationToken))
            {
                var businessTransaction = MapTransaction<BusinessTransaction>(transaction);

                var stake = await GetStakeReleaseAsync(fundInfo, stakeInfo, businessTransaction);
                if (stake != null)
                {
                    stakeEvents.Add(stake);
                }
            }

            foreach (var stakeEvent in stakeEvents.OrderBy(x => x.ConfirmedAt))
            {
                yield return stakeEvent;
            }
        }

        public Task<IStakeEvent> GetStakeEventAsync(Symbol stakeSymbol, Symbol symbol, EthereumTransactionHash hash)
        {
            return GetStakeEventAsync(stakeSymbol, null, symbol, hash);
        }

        public Task<IStakeEvent> GetStakeEventAsync(Symbol stakeSymbol, EthereumAddress address, Symbol symbol, EthereumTransactionHash hash)
        {
            return GetStakeEventAsync(stakeSymbol, new EthereumAddress?(address), symbol, hash);
        }

        public async Task<IStakeEvent> GetStakeEventAsync(Symbol stakeSymbol, EthereumAddress? address, Symbol symbol, EthereumTransactionHash hash)
        {
            var fundInfo = GetFundInfo(symbol);
            var stakeInfo = GetStakeInfo(stakeSymbol);

            var transaction = await Transactions.GetTransactionAsync(fundInfo.ContractAddress, hash);
            if (transaction != null)
            {
                var stake = transaction.Sender.Equals(stakeInfo.StakingAddress, StringComparison.OrdinalIgnoreCase)
                    ? await GetStakeReleaseAsync(fundInfo, stakeInfo, MapTransaction<BusinessTransaction>(transaction))
                    : await GetStakeLockupAsync(fundInfo, stakeInfo, MapTransaction<BusinessTransaction>(transaction))
                        ?? await GetStakeReleaseAsync(fundInfo, stakeInfo, MapTransaction<BusinessTransaction>(transaction));

                if (stake != null)
                {
                    return stake;
                }
            }

            throw new PermanentException($"No transaction found with hash {hash}{(address.HasValue ? $" for address {address}." : ".")}");
        }

        private async Task<IStakeEvent> GetStakeLockupAsync(IFundSettings fundSettings, IStakeSettings stakeSettings, ITransaction transaction)
        {
            var stakeData = await etherClient.GetDataAsync<StakeFunction>(stakeSettings.StakingAddress, transaction.Input);
            if (stakeData != null)
            {
                var seconds = (double)Web3.Convert.FromWei(stakeData.Length, 0);
                var quantity = Web3.Convert.FromWei(stakeData.Quantity, fundSettings.Decimals);

                return new BusinessStakeEvent()
                {
                    Hash = transaction.Hash,
                    ConfirmedAt = transaction.ConfirmedAt,
                    UserAddress = transaction.Sender,
                    StakeAddress = stakeSettings.ContractAddress,
                    ContractAddress = fundSettings.ContractAddress,
                    Type = StakeEventType.Lockup,
                    Lock = new BusinessStakeLock()
                    {
                        Duration = TimeSpan.FromSeconds(seconds),
                        ExpiresAt = transaction.ConfirmedAt.AddSeconds(seconds),
                        Quantity = quantity,
                    }
                };
            }

            return null;
        }

        private async Task<IStakeEvent> GetStakeReleaseAsync(IFundSettings fundSettings, IStakeSettings stakeSettings, ITransaction transaction)
        {
            var operations = await Operations
                .ListOperationsAsync(transaction.Hash)
                .ToListAsync(CancellationToken);

            var feeOperation = operations.SingleOrDefault(x =>
                x.Type == OperationTypes.Transfer &&
                x.Sender.Equals(stakeSettings.StakingAddress, StringComparison.OrdinalIgnoreCase) &&
                stakeSettings.FeeAddresses.Select(x => x.Address.ToLower()).Contains(x.Recipient.ToLower()));

            var releaseOperation = operations.LastOrDefault(x =>
                x.Type == OperationTypes.Transfer &&
                x.Sender.Equals(stakeSettings.StakingAddress, StringComparison.OrdinalIgnoreCase) &&
                (feeOperation == null || x.Order != feeOperation.Order));

            if (releaseOperation != null)
            {
                return new BusinessStakeEvent()
                {
                    Hash = transaction.Hash,
                    ConfirmedAt = transaction.ConfirmedAt,
                    UserAddress = new EthereumAddress(releaseOperation.Recipient),
                    StakeAddress = stakeSettings.ContractAddress,
                    ContractAddress = fundSettings.ContractAddress,
                    Type = feeOperation != null
                        ? StakeEventType.EarlyWithdrawal
                        : StakeEventType.Release,
                    Release = new BusinessStakeRelease()
                    {
                        Quantity = Web3.Convert.FromWei(BigInteger.Parse(releaseOperation.Value), fundSettings.Decimals),
                        FeeQuantity = feeOperation != null
                            ? Web3.Convert.FromWei(BigInteger.Parse(feeOperation.Value), fundSettings.Decimals)
                            : default(decimal?)
                    }
                };
            }

            return null;
        }

        private IStakingPower MapStakingPower(IStakeSettings stakeInfo, DataStakingPower stakePower, CurrencyCode currencyCode)
        {
            return new BusinessStakingPower()
            {
                Symbol = stakeInfo.Symbol,
                Date = stakePower?.Date ?? stakeInfo.InceptionDate,
                Power = CurrencyConverter.Convert(stakePower?.Power ?? decimal.Zero, currencyCode),
                Summary = stakePower?.Summary
                    .Select(fs => new BusinessStakingPowerSummary()
                    {
                        Symbol = GetFunds()
                            .Single(x =>
                                x.ContractAddress.Address.Equals(fs.ContractAddress, StringComparison.OrdinalIgnoreCase))
                            .Symbol,
                        Power = CurrencyConverter.Convert(fs.Power, currencyCode),
                    })
                    .ToList()
                    ?? new List<BusinessStakingPowerSummary>(),
                Breakdown = stakePower?.Breakdown
                    .Select(fp => new BusinessStakingPowerFund()
                    {
                        Symbol = GetFunds()
                            .Single(x =>
                                x.ContractAddress.Address.Equals(fp.ContractAddress, StringComparison.OrdinalIgnoreCase))
                            .Symbol,
                        FundModifier = fp.FundModifier,
                        Quantity = fp.Events.Sum(x => x.Quantity),
                        ModifiedQuantity = fp.Events.Sum(x => x.Quantity * x.TimeModifier * fp.FundModifier),
                        Power = CurrencyConverter.Convert(fp.PricePerToken * fp.Events.Sum(x => x.Quantity * x.TimeModifier * fp.FundModifier), currencyCode),
                    })
                    .ToList()
                    ?? new List<BusinessStakingPowerFund>()
            };
        }
    }
}
