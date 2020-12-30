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
using Pseudonym.Crypto.Invictus.Funds.Data.Models;
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
                if (Enum.TryParse(fund.Symbol, out Symbol symbol))
                {
                    var fundInfo = GetFundInfo(symbol);

                    yield return await GetFundAsync(fundInfo, fund, currencyCode);
                }
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

            var perfs = await fundPerformanceRepository
                .ListPerformancesAsync(fundInfo.Address, from, to)
                .ToListAsync(CancellationToken);

            if (priceMode == PriceMode.Raw)
            {
                foreach (var perf in perfs.OrderBy(x => x.Date))
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

            foreach (var perfGroup in perfs.GroupBy(p => p.Date.Date).OrderBy(x => x.Key))
            {
                yield return new BusinessPerformance()
                {
                    Date = perfGroup.Key,
                    NetAssetValuePerToken = CurrencyConverter.Convert(FormatData(perfGroup, x => x.Nav, priceMode).Value, currencyCode),
                    MarketCap = CurrencyConverter.Convert(FormatData(perfGroup, x => x.MarketCap, priceMode), currencyCode),
                    MarketAssetValuePerToken = CurrencyConverter.Convert(FormatData(perfGroup, x => x.Price, priceMode), currencyCode),
                    Volume = CurrencyConverter.Convert(FormatData(perfGroup, x => x.Volume, priceMode), currencyCode),
                };
            }

            decimal? FormatData(IEnumerable<DataFundPerformance> data, Func<DataFundPerformance, decimal> selector, PriceMode mode)
            {
                var val = priceMode switch
                {
                    PriceMode.Avg => data.Average(selector),
                    PriceMode.Open => data.OrderBy(x => x.Date).Select(selector).First(),
                    PriceMode.Close => data.OrderBy(x => x.Date).Select(selector).Last(),
                    PriceMode.High => data.Select(selector).Max(),
                    PriceMode.Low => data.Select(selector).Min(),
                    _ => throw new ArgumentException($"Arg not handled: {priceMode}", nameof(priceMode)),
                };

                return val != -1
                    ? val
                    : default(decimal?);
            }
        }

        public Task<bool> DeletePerformanceAsync(Symbol symbol, DateTime date)
        {
            var fundInfo = GetFundInfo(symbol);

            return fundPerformanceRepository.DeletePerformanceAsync(fundInfo.Address, date);
        }

        public async Task<ITransactionSet> GetTransactionAsync(Symbol symbol, EthereumTransactionHash hash, CurrencyCode currencyCode)
        {
            var fundInfo = GetFundInfo(symbol);
            var transaction = await Transactions.GetTransactionAsync(fundInfo.Address, hash)
                ?? throw new PermanentException($"Transaction not found {hash}");

            var operations = new List<IOperation>();
            var transactionSet = MapTransaction<BusinessTransactionSet>(transaction);

            await foreach (var operation in Operations
                .ListOperationsAsync(hash)
                .WithCancellation(CancellationToken))
            {
                operations.Add(MapOperation(operation, currencyCode));
            }

            transactionSet.Operations = operations;

            return transactionSet;
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
                .ListTransactionsAsync(fundInfo.Address, startHash, offset, from, to)
                .WithCancellation(CancellationToken))
            {
                yield return MapTransaction<BusinessTransaction>(transaction);
            }
        }

        private async Task<IFund> GetFundAsync(FundSettings settings, InvictusFund fund, CurrencyCode currencyCode)
        {
            var now = DateTime.UtcNow;

            var priceData = settings.Tradable
                ? await ethplorerClient.GetTokenInfoAsync(settings.Address)
                : null;

            var navData = await ListPerformanceAsync(settings.Symbol, PriceMode.Raw, now.AddDays(-29), now, currencyCode)
                .ToListAsync(CancellationToken);

            return MapFund(
                fund,
                navData
                    .Select(x => x.NetAssetValuePerToken)
                    .ToList(),
                priceData,
                settings.Symbol,
                currencyCode);
        }

        private IFund MapFund(InvictusFund fund, IReadOnlyList<decimal> navs, EthplorerPriceSummary priceData, Symbol symbol, CurrencyCode currencyCode)
        {
            var dailyNavs = navs.TakeLast(2);
            var weeklyNavs = navs.TakeLast(8);
            var netVal = fund.NetValue.FromPythonString();
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
                    Value = CurrencyConverter.Convert(netVal, currencyCode),
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
                    .Where(x => x.Value > 0)
                    .Union(fundInfo.Assets.Select(a => MapFundAsset(a, netVal, currencyCode)))
                    .ToList()
            };

            BusinessFundAsset FromInvictusAsset(InvictusAsset asset)
            {
                var value = asset.Value.FromPythonString();
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
                    Coin = new BusinessCoin()
                    {
                        Name = sanitisedName,
                        Symbol = asset.Symbol,
                        HexColour = GetAssetInfo(asset.Symbol)?.Colour,
                        Link = fund?.Links?.External
                            ?? (isFiat
                                ? new Uri(string.Format(FiatTemplate, asset.Symbol.ToUpper()), UriKind.Absolute)
                                : new Uri(string.Format(LinkTemplate, coinMarketCapId), UriKind.Absolute)),
                        ImageLink = fund != null
                            ? new Uri($"https://{HostUrl.Host}/resources/{symbol}.png", UriKind.Absolute)
                            : new Uri(string.Format(ImageTemplate, coinloreId), UriKind.Absolute),
                        MarketLink = (fund == null || fund.Tradable) && !isFiat
                            ? new Uri(string.Format(MarketTemplate, coinloreId, currencyCode), UriKind.Absolute)
                            : null
                    },
                    Value = CurrencyConverter.Convert(value, currencyCode),
                    Share = value / netVal * 100
                };
            }
        }
    }
}
