using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Models;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Ethplorer;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Invictus;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
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

        public FundService(
            IOptions<AppSettings> appSettings,
            IInvictusClient invictusClient,
            IEthplorerClient ethplorerClient,
            ICurrencyConverter currencyConverter,
            ITransactionRepository transactionRepository,
            IOperationRepository operationRepository,
            IScopedCancellationToken scopedCancellationToken)
            : base(appSettings, currencyConverter, transactionRepository, operationRepository, scopedCancellationToken)
        {
            this.invictusClient = invictusClient;
            this.ethplorerClient = ethplorerClient;
        }

        public async IAsyncEnumerable<IFund> ListFundsAsync(CurrencyCode currencyCode)
        {
            await foreach (var fund in invictusClient.ListFundsAsync())
            {
                if (Enum.TryParse(fund.Symbol, out Symbol symbol))
                {
                    var fundInfo = GetFundInfo(symbol);

                    var priceData = fundInfo.Tradable
                        ? await ethplorerClient.GetTokenInfoAsync(fundInfo.Address)
                        : null;

                    yield return MapFund(fund, priceData, symbol, currencyCode);
                }
            }
        }

        public async Task<IFund> GetFundAsync(Symbol symbol, CurrencyCode currencyCode)
        {
            var fundInfo = GetFundInfo(symbol);

            var fund = await invictusClient.GetFundAsync(symbol);

            var priceData = fundInfo.Tradable
                ? await ethplorerClient.GetTokenInfoAsync(fundInfo.Address)
                : null;

            return MapFund(fund, priceData, symbol, currencyCode);
        }

        public async IAsyncEnumerable<IPerformance> ListPerformanceAsync(Symbol symbol, PriceMode priceMode, DateTime from, DateTime to, CurrencyCode currencyCode)
        {
            var fundInfo = GetFundInfo(symbol);

            var priceData = fundInfo.Tradable
                ? await ethplorerClient.GetTokenPricingAsync(fundInfo.Address)
                : null;

            await foreach (var perf in invictusClient
                .ListPerformanceAsync(symbol, from, to)
                .WithCancellation(CancellationToken))
            {
                var marketData = priceData?.Prices?.SingleOrDefault(x => x.Date.Date == perf.Date.Date)
                    ?? priceData?.Prices?.OrderBy(x => x.Date)?.FirstOrDefault();

                yield return new BusinessPerformance()
                {
                    Date = perf.Date,
                    NetValue = CurrencyConverter.Convert(perf.NetValue, currencyCode),
                    NetAssetValuePerToken = CurrencyConverter.Convert(perf.GetNav(priceMode), currencyCode),
                    MarketCap = CurrencyConverter.Convert(marketData?.MarketCap, currencyCode),
                    MarketAssetValuePerToken = CurrencyConverter.Convert(marketData?.GetMarketPrice(priceMode), currencyCode),
                };
            }
        }

        public async Task<ITransactionSet> GetTransactionAsync(Symbol symbol, EthereumTransactionHash hash, CurrencyCode currencyCode)
        {
            var fundInfo = GetFundInfo(symbol);
            var transaction = await Transactions.GetTransactionsAsync(fundInfo.Address, hash)
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

        public async IAsyncEnumerable<ITransaction> ListTransactionsAsync(Symbol symbol, CurrencyCode currencyCode)
        {
            var fundInfo = GetFundInfo(symbol);

            await foreach (var transaction in Transactions
                .ListTransactionsAsync(fundInfo.Address)
                .WithCancellation(CancellationToken))
            {
                yield return MapTransaction<BusinessTransaction>(transaction);
            }
        }

        private IFund MapFund(InvictusFund fund, EthplorerPriceSummary priceData, Symbol symbol, CurrencyCode currencyCode)
        {
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
                NetValue = CurrencyConverter.Convert(netVal, currencyCode),
                NetAssetValuePerToken = CurrencyConverter.Convert(fund.NetAssetValuePerToken.FromPythonString(), currencyCode),
                Market = new BusinessMarket()
                {
                    IsTradable = priceData != null,
                    Cap = priceData?.MarketCap ?? 0,
                    Total = CurrencyConverter.Convert(priceData?.MarketValuePerToken * circulatingSupply, currencyCode),
                    PricePerToken = CurrencyConverter.Convert(priceData?.MarketValuePerToken, currencyCode),
                    DiffDaily = priceData?.DiffDaily ?? 0,
                    DiffWeekly = priceData?.DiffWeekly ?? 0,
                    DiffMonthly = priceData?.DiffMonthly ?? 0,
                    Volume = CurrencyConverter.Convert(priceData?.Volume, currencyCode),
                    VolumeDiffDaily = priceData?.VolumeDiffDaily ?? 0,
                    VolumeDiffWeekly = priceData?.VolumeDiffWeekly ?? 0,
                    VolumeDiffMonthly = priceData?.VolumeDiffMonthly ?? 0
                },
                Assets = fund.Assets
                    .Select(a => new BusinessFundAsset()
                    {
                        Symbol = a.Symbol,
                        Name = a.Name,
                        Value = CurrencyConverter.Convert(a.Value.FromPythonString(), currencyCode),
                        Share = CurrencyConverter.Convert(a.Value.FromPythonString() / netVal * 100, currencyCode),
                        Link = Enum.TryParse(a.Symbol, out Symbol symbol)
                            ? GetFundInfo(symbol).Links.External
                            : new Uri($"https://coinmarketcap.com/currencies/{a.Name.Replace(" ", "-").ToLower().Trim()}", UriKind.Absolute)
                    })
                    .Where(x => x.Value > 0)
                    .Union(fundInfo.Assets
                        .Select(a => new BusinessFundAsset()
                        {
                            Symbol = a.Symbol,
                            Name = a.Name,
                            Value = CurrencyConverter.Convert(a.Value, currencyCode),
                            Share = a.Share,
                            Link = a.Link
                        }))
                    .ToList()
            };
        }
    }
}
