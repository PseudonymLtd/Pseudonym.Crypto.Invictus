using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Nethereum.Web3;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Models;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Configuration.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Data.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Data.Models;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Exceptions;
using Pseudonym.Crypto.Invictus.Shared.Models;

namespace Pseudonym.Crypto.Invictus.Funds.Business
{
    internal abstract class AbstractService
    {
        protected const string FiatTemplate = "https://www.tradingview.com/symbols/ETH{0}";
        protected const string LinkTemplate = "https://coinmarketcap.com/currencies/{0}";
        protected const string ImageTemplate = "https://c2.coinlore.com/img/{0}.png";
        protected const string MarketTemplate = "https://widget.coinlore.com/widgets/new-single/?id={0}&cur={1}";
        protected const string PoolTemplate = "https://info.uniswap.org/pair/{0}";

        private readonly AppSettings appSettings;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IScopedCancellationToken scopedCancellationToken;

        public AbstractService(
            IOptions<AppSettings> appSettings,
            ICurrencyConverter currencyConverter,
            ITransactionRepository transactionRepository,
            IOperationRepository operationRepository,
            IHttpContextAccessor httpContextAccessor,
            IScopedCancellationToken scopedCancellationToken)
        {
            this.appSettings = appSettings.Value;
            this.httpContextAccessor = httpContextAccessor;
            this.scopedCancellationToken = scopedCancellationToken;

            CurrencyConverter = currencyConverter;
            Transactions = transactionRepository;
            Operations = operationRepository;
        }

        protected ICurrencyConverter CurrencyConverter { get; }

        protected ITransactionRepository Transactions { get; }

        protected IOperationRepository Operations { get; }

        protected CancellationToken CancellationToken => scopedCancellationToken.Token;

        protected Uri HostUrl => appSettings.HostUrl;

        protected IReadOnlyList<IFundSettings> GetFunds()
        {
            return appSettings.Funds;
        }

        protected IReadOnlyList<IStakeSettings> GetStakes()
        {
            return appSettings.Stakes;
        }

        protected IFundSettings GetFundInfo(Symbol symbol)
        {
            var fundSettings = appSettings.Funds.SingleOrDefault(x => x.Symbol == symbol)
                ?? throw new PermanentException($"Symbol `{symbol}` is not a fund contract");

            if (httpContextAccessor.HttpContext != null)
            {
                if (httpContextAccessor.HttpContext.Response.Headers.ContainsKey(Headers.Contract))
                {
                    httpContextAccessor.HttpContext.Response.Headers[Headers.Contract] = fundSettings.ContractAddress;
                }
                else
                {
                    httpContextAccessor.HttpContext.Response.Headers.Add(Headers.Contract, fundSettings.ContractAddress);
                }
            }

            return fundSettings;
        }

        protected IStakeSettings GetStakeInfo(Symbol symbol)
        {
            var stakeSettings = appSettings.Stakes.SingleOrDefault(x => x.Symbol == symbol)
                ?? throw new PermanentException($"Address `{symbol}` is not a stake contract");

            if (httpContextAccessor.HttpContext != null)
            {
                if (httpContextAccessor.HttpContext.Response.Headers.ContainsKey(Headers.StakeContract))
                {
                    httpContextAccessor.HttpContext.Response.Headers[Headers.StakeContract] = stakeSettings.ContractAddress;
                }
                else
                {
                    httpContextAccessor.HttpContext.Response.Headers.Add(Headers.StakeContract, stakeSettings.ContractAddress);
                }
            }

            return stakeSettings;
        }

        protected IAssetSettings GetAssetInfo(string symbol)
        {
            return appSettings.Assets.SingleOrDefault(a =>
                a.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
        }

        protected IHoldingSettings GetHoldingInfo(string symbol)
        {
            return appSettings.Holdings.SingleOrDefault(h =>
                h.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
        }

        protected TBusinessTransaction MapTransaction<TBusinessTransaction>(DataTransaction transaction)
            where TBusinessTransaction : BusinessTransaction, new()
        {
            return new TBusinessTransaction()
            {
                Address = new EthereumAddress(transaction.Address),
                Hash = new EthereumTransactionHash(transaction.Hash),
                Success = transaction.Success,
                BlockNumber = transaction.BlockNumber,
                Sender = new EthereumAddress(transaction.Sender),
                Recipient = new EthereumAddress(transaction.Recipient),
                Input = transaction.Input,
                ConfirmedAt = transaction.ConfirmedAt,
                Confirmations = transaction.Confirmations,
                Eth = transaction.Eth,
                Gas = transaction.Gas,
                GasLimit = transaction.GasLimit
            };
        }

        protected BusinessOperation MapOperation(DataOperation operation, CurrencyCode currencyCode)
        {
            var sanitisedId = operation.ContractName?.Replace(" ", "-").Replace(".", "-").ToLower().Trim() ?? string.Empty;
            var isInvictus = Enum.TryParse(operation.ContractSymbol, out Symbol symbol);

            var assetInfo = GetAssetInfo(operation.ContractSymbol);
            var coinloreId = assetInfo?.CoinLore ?? sanitisedId;
            var coinMarketCapId = assetInfo?.CoinMarketCap ?? sanitisedId;
            var isUSDStableCoin = assetInfo?.IsUSDStableCoin ?? false;

            var fund = isInvictus && symbol.IsFund()
                ? GetFundInfo(symbol)
                : null;

            var stake = isInvictus && symbol.IsStake()
                ? GetStakeInfo(symbol)
                : null;

            return new BusinessOperation()
            {
                Hash = new EthereumTransactionHash(operation.Hash),
                Order = operation.Order,
                Type = operation.Type,
                Address = string.IsNullOrEmpty(operation.Address)
                    ? default(EthereumAddress?)
                    : new EthereumAddress(operation.Address),
                Sender = string.IsNullOrEmpty(operation.Sender)
                    ? default(EthereumAddress?)
                    : new EthereumAddress(operation.Sender),
                Recipient = string.IsNullOrEmpty(operation.Recipient)
                    ? default(EthereumAddress?)
                    : new EthereumAddress(operation.Recipient),
                Addresses = operation.Addresses
                    .Select(a => new EthereumAddress(a))
                    .ToList(),
                IsEth = operation.IsEth,
                PricePerToken = operation.Price == default && isUSDStableCoin
                    ? CurrencyConverter.Convert(1, currencyCode)
                    : CurrencyConverter.Convert(operation.Price, currencyCode),
                Quantity = operation.Type == OperationTypes.Transfer
                    ? Web3.Convert.FromWei(BigInteger.Parse(operation.Value), fund?.Decimals ?? stake?.Decimals ?? operation.ContractDecimals)
                    : 0,
                Value = operation.Value,
                Priority = operation.Priority,
                ContractAddress = new EthereumAddress(operation.ContractAddress),
                ContractSymbol = operation.ContractSymbol,
                ContractDecimals = operation.ContractDecimals,
                ContractHolders = operation.ContractHolders,
                ContractIssuances = operation.ContractIssuances,
                ContractName = fund?.Name ?? stake?.Name ?? operation.ContractName,
                ContractLink = fund?.Links?.External
                    ?? stake?.Links?.External
                    ?? (!string.IsNullOrEmpty(operation.ContractLink)
                        ? new Uri(operation.ContractLink, UriKind.Absolute)
                        : new Uri(string.Format(LinkTemplate, coinMarketCapId), UriKind.Absolute)),
                ContractImageLink = isInvictus
                    ? new Uri($"https://{HostUrl.Host}/resources/{symbol}.png", UriKind.Absolute)
                    : new Uri(string.Format(ImageTemplate, coinloreId), UriKind.Absolute),
                ContractMarketLink = !string.IsNullOrEmpty(coinloreId) && !symbol.IsStake() && (fund == null || fund.Tradable)
                    ? new Uri(string.Format(MarketTemplate, coinloreId, currencyCode), UriKind.Absolute)
                    : null
            };
        }

        protected decimal? Aggregate<T>(IEnumerable<T> data, Func<T, decimal> selector, PriceMode mode)
            where T : IDataAggregatable
        {
            var val = mode switch
            {
                PriceMode.Avg => data.Average(selector),
                PriceMode.Open => data.OrderBy(x => x.Date).Select(selector).First(),
                PriceMode.Close => data.OrderBy(x => x.Date).Select(selector).Last(),
                PriceMode.High => data.Select(selector).Max(),
                PriceMode.Low => data.Select(selector).Min(),
                _ => throw new ArgumentException($"Arg not handled: {mode}", nameof(mode)),
            };

            return val != -1
                ? val
                : default(decimal?);
        }
    }
}
