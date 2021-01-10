using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.CoinGecko;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Invictus;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Configuration.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Data.Models;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Services
{
    internal sealed class FundPerformanceCachingService : BaseCachingService
    {
        private const int MaxDays = 7;

        public FundPerformanceCachingService(
            IOptions<AppSettings> appSettings,
            IServiceProvider serviceProvider)
            : base(appSettings, serviceProvider)
        {
        }

        protected override TimeSpan Interval => TimeSpan.FromHours(1);

        protected override async Task ProcessAsync(IServiceScope scope, CancellationToken cancellationToken)
        {
            var repository = scope.ServiceProvider.GetRequiredService<IFundPerformanceRepository>();
            var coinGeckoClient = scope.ServiceProvider.GetRequiredService<ICoinGeckoClient>();
            var invictusClient = scope.ServiceProvider.GetRequiredService<IInvictusClient>();

            foreach (var fund in AppSettings.Funds.Cast<IFundSettings>())
            {
                try
                {
                    var latestDate = await repository.GetLatestDateAsync(fund.ContractAddress)
                        ?? fund.InceptionDate;

                    await SyncPerformanceAsync(
                        coinGeckoClient,
                        invictusClient,
                        repository,
                        fund,
                        latestDate.AddDays(-1),
                        DateTimeOffset.UtcNow.AddDays(1).Round(),
                        cancellationToken);

                    var lowestDate = await repository.GetLowestDateAsync(fund.ContractAddress)
                        ?? DateTimeOffset.UtcNow.Round();

                    await SyncPerformanceAsync(
                        coinGeckoClient,
                        invictusClient,
                        repository,
                        fund,
                        fund.InceptionDate,
                        lowestDate.AddDays(1),
                        cancellationToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error getting market data for {fund.Symbol}.");
                    Console.WriteLine(e);
                }
            }
        }

        private async Task SyncPerformanceAsync(
            ICoinGeckoClient coinGeckoClient,
            IInvictusClient invictusClient,
            IFundPerformanceRepository repository,
            IFundSettings fund,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            CancellationToken cancellationToken)
        {
            var start = new DateTimeOffset(startDate.Date, TimeSpan.Zero);

            while (!cancellationToken.IsCancellationRequested)
            {
                var end = start
                    .AddDays(MaxDays)
                    .AddHours(23)
                    .AddMinutes(59)
                    .AddSeconds(59);

                end = end > endDate
                    ? endDate
                    : end;

                Console.WriteLine($"[{fund.ContractAddress}] Processing Batch: {start} -> {end}");

                var invictusNavs = await invictusClient
                    .ListPerformanceAsync(fund.Symbol, start, end)
                    .ToListAsync(cancellationToken);

                if (invictusNavs.Any())
                {
                    var marketPrices = fund.Tradable
                        ? await coinGeckoClient
                            .ListCoinPerformanceAsync(fund.CoinGeckoId, start.AddDays(-1), end.AddDays(1))
                            .ToListAsync(cancellationToken)
                        : new List<CoinGeckoCoinPerformance>();

                    foreach (var nav in invictusNavs)
                    {
                        var closestPrice = marketPrices
                            .OrderBy(i => Math.Abs(i.Date.ToUnixTimeSeconds() - nav.Date.ToUnixTimeSeconds()))
                            .FirstOrDefault();

                        var perf = MapPerformance(fund.ContractAddress, nav, closestPrice);

                        await repository.UploadItemsAsync(perf);
                    }
                }

                Console.WriteLine($"[{fund.ContractAddress}] Finished Batch: {start} -> {end}");

                if (end >= endDate)
                {
                    break;
                }
                else
                {
                    start = end.AddSeconds(1);
                }
            }
        }

        private DataFundPerformance MapPerformance(
            EthereumAddress address,
            InvictusPerformance navData,
            CoinGeckoCoinPerformance marketData)
        {
            return new DataFundPerformance()
            {
                Address = address,
                Date = navData.Date.UtcDateTime,
                Nav = navData.NetAssetValuePerToken.FromPythonString(),
                Price = marketData?.Price ?? -1,
                MarketCap = marketData?.MarketCap ?? -1,
                Volume = marketData?.Volume ?? -1
            };
        }
    }
}
