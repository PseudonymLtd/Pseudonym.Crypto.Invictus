using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Investments.Business.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Business.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Business.Models;
using Pseudonym.Crypto.Invictus.TrackerService.Clients.Models;
using Pseudonym.Crypto.Invictus.TrackerService.Configuration;
using Pseudonym.Crypto.Invictus.TrackerService.Ethereum;

namespace Pseudonym.Crypto.Invictus.TrackerService.Business
{
    internal sealed class FundService : IFundService
    {
        private readonly AppSettings appSettings;
        private readonly IInvictusClient invictusClient;
        private readonly ICurrencyConverter currencyConverter;

        public FundService(
            IOptions<AppSettings> appSettings,
            IInvictusClient invictusClient,
            ICurrencyConverter currencyConverter)
        {
            this.appSettings = appSettings.Value;
            this.invictusClient = invictusClient;
            this.currencyConverter = currencyConverter;
        }

        public async IAsyncEnumerable<IFund> ListFundsAsync(CurrencyCode currencyCode, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var fund in invictusClient.ListFundsAsync(cancellationToken))
            {
                if (Enum.TryParse(fund.Symbol, out Symbol symbol))
                {
                    yield return Map(fund, symbol, currencyCode);
                }
            }
        }

        public async Task<IFund> GetFundAsync(Symbol symbol, CurrencyCode currencyCode, CancellationToken cancellationToken)
        {
            var fund = await invictusClient.GetFundAsync(symbol, cancellationToken);

            return Map(fund, symbol, currencyCode);
        }

        public async IAsyncEnumerable<IPerformance> ListPerformanceAsync(
            Symbol symbol,
            DateTime from,
            DateTime to,
            CurrencyCode currencyCode,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var perf in invictusClient.ListPerformanceAsync(symbol, from, to, cancellationToken))
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
                Name = string.Join(" ", fund.Name
                    .Trim()
                    .Split('-')
                    .Select(x =>
                    {
                        var chars = x.ToCharArray();
                        chars[0] = char.ToUpperInvariant(chars[0]);
                        return new string(chars);
                    })),
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
