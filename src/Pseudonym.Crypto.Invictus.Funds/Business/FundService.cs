using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Models;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Ethplorer;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Invictus;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Configuration.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Exceptions;

namespace Pseudonym.Crypto.Invictus.Funds.Business
{
    internal sealed class FundService : AbstractService, IFundService
    {
        private readonly IInvictusClient invictusClient;
        private readonly IEthplorerClient ethplorerClient;
        private readonly IFundPerformanceRepository fundPerformanceRepository;

        public FundService(
            IOptions<AppSettings> appSettings,
            IInvictusClient invictusClient,
            IEthplorerClient ethplorerClient,
            ICurrencyConverter currencyConverter,
            IFundPerformanceRepository fundPerformanceRepository,
            ITransactionRepository transactionRepository,
            IOperationRepository operationRepository,
            IHttpContextAccessor httpContextAccessor,
            IScopedCancellationToken scopedCancellationToken)
            : base(appSettings, currencyConverter, transactionRepository, operationRepository, httpContextAccessor, scopedCancellationToken)
        {
            this.invictusClient = invictusClient;
            this.ethplorerClient = ethplorerClient;
            this.fundPerformanceRepository = fundPerformanceRepository;
        }

        public async IAsyncEnumerable<IFund> ListFundsAsync(CurrencyCode currencyCode)
        {
            await foreach (var fund in invictusClient.ListFundsAsync())
            {
                var fundInfo = GetFundInfo(fund.Symbol);

                yield return await GetFundAsync(fundInfo, fund, currencyCode);
            }
        }

        public async Task<IFund> GetFundAsync(Symbol symbol, CurrencyCode currencyCode)
        {
            var fundInfo = GetFundInfo(symbol);

            var fund = await invictusClient.GetFundAsync(symbol);

            return await GetFundAsync(fundInfo, fund, currencyCode);
        }

        public async IAsyncEnumerable<IPerformance> ListPerformanceAsync(Symbol symbol, PriceMode priceMode, DateTime from, DateTime to, CurrencyCode currencyCode)
        {
            var fundInfo = GetFundInfo(symbol);

            var start = from < fundInfo.InceptionDate.UtcDateTime
                ? fundInfo.InceptionDate.UtcDateTime
                : from;

            if (to <= fundInfo.InceptionDate.UtcDateTime)
            {
                yield break;
            }

            if (priceMode == PriceMode.Raw)
            {
                await foreach (var perf in fundPerformanceRepository
                    .ListPerformancesAsync(fundInfo.ContractAddress, start, to)
                    .WithCancellation(CancellationToken))
                {
                    yield return new BusinessPerformance()
                    {
                        Date = perf.Date,
                        NetAssetValuePerToken = CurrencyConverter.Convert(perf.Nav, currencyCode),
                        MarketCap = CurrencyConverter.Convert(
                            perf.MarketCap != -1
                                ? perf.MarketCap
                                : default(decimal?),
                            currencyCode),
                        MarketAssetValuePerToken = CurrencyConverter.Convert(
                            perf.Price != -1
                                ? perf.Price
                                : default(decimal?),
                            currencyCode),
                        Volume = CurrencyConverter.Convert(
                            perf.Volume != -1
                                ? perf.Volume
                                : default(decimal?),
                            currencyCode),
                    };
                }

                yield break;
            }

            var perfs = await fundPerformanceRepository
                .ListPerformancesAsync(fundInfo.ContractAddress, start, to)
                .ToListAsync(CancellationToken);

            foreach (var perfGroup in perfs.GroupBy(p => p.Date.Date).OrderBy(x => x.Key))
            {
                yield return new BusinessPerformance()
                {
                    Date = perfGroup.Key,
                    NetAssetValuePerToken = CurrencyConverter.Convert(Aggregate(perfGroup, x => x.Nav, priceMode).Value, currencyCode),
                    MarketCap = CurrencyConverter.Convert(Aggregate(perfGroup, x => x.MarketCap, priceMode), currencyCode),
                    MarketAssetValuePerToken = CurrencyConverter.Convert(Aggregate(perfGroup, x => x.Price, priceMode), currencyCode),
                    Volume = CurrencyConverter.Convert(Aggregate(perfGroup, x => x.Volume, priceMode), currencyCode),
                };
            }
        }

        public Task<bool> DeletePerformanceAsync(Symbol symbol, DateTime date)
        {
            var fundInfo = GetFundInfo(symbol);

            return fundPerformanceRepository.DeletePerformanceAsync(fundInfo.ContractAddress, date);
        }

        public async Task<ITransactionSet> GetTransactionAsync(Symbol symbol, EthereumTransactionHash hash, CurrencyCode currencyCode)
        {
            var fundInfo = GetFundInfo(symbol);
            var transaction = await Transactions.GetTransactionAsync(fundInfo.ContractAddress, hash)
                ?? throw new PermanentException($"Transaction not found {hash}");

            var transactionSet = MapTransaction<BusinessTransactionSet>(transaction);

            var operations = await Operations
                .ListOperationsAsync(hash)
                .ToListAsync(CancellationToken);

            transactionSet.Operations = operations
                .Select(o => MapOperation(o, currencyCode))
                .ToList();

            return transactionSet;
        }

        public async IAsyncEnumerable<ITransactionSet> ListBurnsAsync(Symbol symbol, CurrencyCode currencyCode)
        {
            var fundInfo = GetFundInfo(symbol);

            if (fundInfo.Burnable)
            {
                var transactionHashes = new List<EthereumTransactionHash>();

                await foreach (var hash in Operations
                    .ListInboundHashesAsync(EthereumAddress.Empty, fundInfo.ContractAddress)
                    .WithCancellation(CancellationToken))
                {
                    transactionHashes.Add(hash);
                }

                foreach (var hash in transactionHashes.Distinct())
                {
                    var transaction = await Transactions.GetTransactionAsync(fundInfo.ContractAddress, hash);
                    if (transaction != null &&
                        transaction.ConfirmedAt.Year > 2019)
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

        public async IAsyncEnumerable<ITransaction> ListTransactionsAsync(
            Symbol symbol,
            EthereumTransactionHash? startHash,
            DateTime? offset,
            DateTime from,
            DateTime to,
            CurrencyCode currencyCode)
        {
            var fundInfo = GetFundInfo(symbol);

            await foreach (var transaction in Transactions
                .ListTransactionsAsync(fundInfo.ContractAddress, startHash, offset, from, to)
                .WithCancellation(CancellationToken))
            {
                yield return MapTransaction<BusinessTransaction>(transaction);
            }
        }

        private async Task<IFund> GetFundAsync(IFundSettings settings, IInvictusFund fund, CurrencyCode currencyCode)
        {
            var now = DateTime.UtcNow;

            var priceData = settings.Tradable
                ? await ethplorerClient.GetTokenInfoAsync(settings.ContractAddress)
                : null;

            var navData = await ListPerformanceAsync(settings.Symbol, PriceMode.Raw, now.AddDays(-29), now, currencyCode)
                .ToListAsync(CancellationToken);

            return MapFund(
                fund,
                navData
                    .Select(x => x.NetAssetValuePerToken)
                    .ToList(),
                priceData?.Price,
                settings.Symbol,
                currencyCode);
        }

        private IFund MapFund(IInvictusFund fund, IReadOnlyList<decimal> navs, EthplorerPriceSummary priceData, Symbol symbol, CurrencyCode currencyCode)
        {
            var dailyNavs = navs.TakeLast(2);
            var weeklyNavs = navs.TakeLast(8);
            var netVal = fund.Assets.Any()
                ? CurrencyConverter.Convert(fund.Assets.Select(x => x.Total.FromPythonString()).Where(x => x > 0).Sum(), currencyCode)
                : CurrencyConverter.Convert(fund.NetValue.FromPythonString(), currencyCode);
            var circulatingSupply = fund.CirculatingSupply.FromPythonString();
            var fundInfo = GetFundInfo(symbol);

            return new BusinessFund()
            {
                Name = fund.Name,
                Description = fundInfo.Description,
                FactSheetUri = fundInfo.Links.Fact,
                LitepaperUri = fundInfo.Links.Lite,
                InvictusUri = fundInfo.Links.External,
                Token = new BusinessToken()
                {
                    Symbol = fundInfo.Symbol,
                    ContractAddress = new EthereumAddress(fundInfo.ContractAddress),
                    Decimals = fundInfo.Decimals
                },
                CirculatingSupply = circulatingSupply,
                Nav = new BusinessNav()
                {
                    Value = CurrencyConverter.Convert(fund.NetValue.FromPythonString(), currencyCode),
                    ValuePerToken = CurrencyConverter.Convert(fund.NetAssetValuePerToken.FromPythonString(), currencyCode),
                    DiffDaily = dailyNavs.First().PercentageDiff(dailyNavs.Last()),
                    DiffWeekly = weeklyNavs.First().PercentageDiff(weeklyNavs.Last()),
                    DiffMonthly = navs.First().PercentageDiff(navs.Last()),
                },
                Market = new BusinessMarket()
                {
                    IsTradable = priceData != null,
                    Cap = priceData?.MarketCap ?? 0,
                    Total = CurrencyConverter.Convert((priceData?.MarketValuePerToken ?? 0) * circulatingSupply, currencyCode),
                    PricePerToken = CurrencyConverter.Convert(priceData?.MarketValuePerToken ?? 0, currencyCode),
                    DiffDaily = priceData?.DiffDaily ?? 0,
                    DiffWeekly = priceData?.DiffWeekly ?? 0,
                    DiffMonthly = priceData?.DiffMonthly ?? 0,
                    Volume = CurrencyConverter.Convert(priceData?.Volume ?? 0, currencyCode),
                    VolumeDiffDaily = priceData?.VolumeDiffDaily ?? 0,
                    VolumeDiffWeekly = priceData?.VolumeDiffWeekly ?? 0,
                    VolumeDiffMonthly = priceData?.VolumeDiffMonthly ?? 0
                },
                Assets = fund.Assets
                    .Select(FromInvictusAsset)
                    .Where(x => x.Total > 0)
                    .Union(fundInfo.Assets.Select(a => MapFundAsset(a, netVal, currencyCode)))
                    .OrderByDescending(x => x.Total)
                    .ToList()
            };

            BusinessFundAsset FromInvictusAsset(InvictusAsset asset)
            {
                var total = CurrencyConverter.Convert(asset.Total.FromPythonString(), currencyCode);
                var holding = GetHoldingInfo(asset.Symbol);
                var isFiat = Enum.IsDefined(typeof(CurrencyCode), asset.Symbol);
                var sanitisedId = asset.Name.Replace(" ", "-").Replace(".", "-").ToLower().Trim();
                var sanitisedName = string.Join(" ", asset.Name.Trim()
                    .Replace("-", " ")
                    .Split(' ')
                    .Select(x => x.Length > 1 ? x.Substring(0, 1).ToUpper() + x.Substring(1) : x.ToUpper()));
                var coinloreId = GetAssetInfo(asset.Symbol)?.CoinLore ?? sanitisedId;
                var coinMarketCapId = GetAssetInfo(asset.Symbol)?.CoinMarketCap ?? sanitisedId;
                var fund = Enum.TryParse(asset.Symbol, out Symbol symbol)
                    ? GetFundInfo(symbol)
                    : null;

                return new BusinessFundAsset()
                {
                    Holding = new BusinessHolding()
                    {
                        Name = sanitisedName,
                        Symbol = asset.Symbol,
                        HexColour = holding?.Colour ?? GetAssetInfo(asset.Symbol)?.Colour,
                        IsCoin = holding == null,
                        Link = holding?.Link
                            ?? fund?.Links?.External
                            ?? (isFiat
                                ? new Uri(string.Format(FiatTemplate, asset.Symbol.ToUpper()), UriKind.Absolute)
                                : new Uri(string.Format(LinkTemplate, coinMarketCapId), UriKind.Absolute)),
                        ImageLink = holding?.ImageLink ?? (fund != null || isFiat
                            ? new Uri($"https://{HostUrl.Host}/resources/{asset.Symbol}.png", UriKind.Absolute)
                            : new Uri(string.Format(ImageTemplate, coinloreId), UriKind.Absolute)),
                        MarketLink = (fund == null || fund.Tradable) && !isFiat && holding == null
                            ? new Uri(string.Format(MarketTemplate, coinloreId, currencyCode), UriKind.Absolute)
                            : null
                    },
                    Quantity = CurrencyConverter.Convert(asset.Quantity.FromPythonString(), currencyCode),
                    PricePerToken = CurrencyConverter.Convert(asset.PricePerToken.FromPythonString(), currencyCode),
                    Total = total,
                    Share = total / netVal * 100
                };
            }
        }
    }
}
