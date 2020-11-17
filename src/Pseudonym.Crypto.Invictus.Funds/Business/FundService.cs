﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Models;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Business
{
    internal sealed class FundService : IFundService
    {
        private readonly AppSettings appSettings;
        private readonly IInvictusClient invictusClient;
        private readonly ICurrencyConverter currencyConverter;
        private readonly IScopedCancellationToken scopedCancellationToken;

        public FundService(
            IOptions<AppSettings> appSettings,
            IInvictusClient invictusClient,
            ICurrencyConverter currencyConverter,
            IScopedCancellationToken scopedCancellationToken)
        {
            this.appSettings = appSettings.Value;
            this.invictusClient = invictusClient;
            this.currencyConverter = currencyConverter;
            this.scopedCancellationToken = scopedCancellationToken;
        }

        public async IAsyncEnumerable<IFund> ListFundsAsync(CurrencyCode currencyCode)
        {
            foreach (var fundInfo in appSettings.Funds)
            {
                var fund = await GetFundAsync(fundInfo.Symbol, currencyCode);

                yield return fund;
            }
        }

        public async Task<IFund> GetFundAsync(Symbol symbol, CurrencyCode currencyCode)
        {
            var fund = await invictusClient.GetFundAsync(symbol);

            return Map(fund, symbol, currencyCode);
        }

        public async IAsyncEnumerable<IPerformance> ListPerformanceAsync(Symbol symbol, DateTime from, DateTime to, CurrencyCode currencyCode)
        {
            await foreach (var perf in invictusClient
                .ListPerformanceAsync(symbol, from, to)
                .WithCancellation(scopedCancellationToken.Token))
            {
                yield return new BusinessPerformance()
                {
                    Date = perf.Date,
                    NetValue = currencyConverter.Convert(perf.NetValue.FromPythonString(), currencyCode),
                    NetAssetValuePerToken = currencyConverter.Convert(perf.NetAssetValuePerToken.FromPythonString(), currencyCode),
                };
            }
        }

        private IFund Map(InvictusFund fund, Symbol symbol, CurrencyCode currencyCode)
        {
            var netVal = fund.NetValue.FromPythonString();
            var marketVal = fund.MarketValuePerToken.FromOptionalPythonString();
            var fundInfo = appSettings.Funds.SingleOrDefault(x => x.Symbol == symbol);

            return new BusinessFund()
            {
                Name = fund.Name,
                Description = fundInfo.Description,
                FactSheetUri = fundInfo.Links.Fact,
                LitepaperUri = fundInfo.Links.Lite,
                Token = new BusinessToken()
                {
                    Symbol = fundInfo.Symbol,
                    ContractAddress = new EthereumAddress(fundInfo.ContractAddress),
                    Decimals = fundInfo.Decimals
                },
                CirculatingSupply = fund.CirculatingSupply.FromPythonString(),
                NetValue = currencyConverter.Convert(netVal, currencyCode),
                NetAssetValuePerToken = currencyConverter.Convert(fund.NetAssetValuePerToken.FromPythonString(), currencyCode),
                MarketValuePerToken = marketVal.HasValue
                    ? currencyConverter.Convert(marketVal.Value, currencyCode)
                    : default(decimal?),
                Assets = fund.Assets
                    .Select(a => new BusinessAsset()
                    {
                        Symbol = a.Symbol,
                        Name = a.Name,
                        Value = currencyConverter.Convert(a.Value.FromPythonString(), currencyCode),
                        Share = currencyConverter.Convert(a.Value.FromPythonString() / netVal * 100, currencyCode)
                    })
                    .Where(x => x.Value > 0)
                    .Union(fundInfo.Assets
                        .Select(a => new BusinessAsset()
                        {
                            Symbol = a.Symbol,
                            Name = a.Name,
                            Value = currencyConverter.Convert(a.Value, currencyCode),
                            Share = a.Share
                        }))
                    .ToList()
            };
        }
    }
}
