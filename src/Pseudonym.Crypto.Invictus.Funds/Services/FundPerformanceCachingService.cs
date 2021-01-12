using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.CoinGecko;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Configuration.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Data.Models;

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
            var graphClient = scope.ServiceProvider.GetRequiredService<IGraphClient>();

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
                        latestDate.AddDays(-1).Round(),
                        DateTimeOffset.UtcNow.AddDays(1).Round(),
                        cancellationToken);

                    var lowestDate = await repository.GetLowestDateAsync(fund.ContractAddress)
                        ?? DateTimeOffset.UtcNow.Round();

                    if (lowestDate.Date != fund.InceptionDate.Date)
                    {
                        await SyncPerformanceAsync(
                            coinGeckoClient,
                            invictusClient,
                            repository,
                            fund,
                            fund.InceptionDate,
                            lowestDate.AddDays(1).Round(),
                            cancellationToken);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error getting market data for {fund.Symbol}.");
                    Console.WriteLine(e);
                }
            }

            foreach (var stake in AppSettings.Stakes.Cast<IStakeSettings>())
            {
                try
                {
                    var latestDate = await repository.GetLatestDateAsync(stake.ContractAddress)
                        ?? stake.InceptionDate;

                    await SyncPerformanceAsync(
                        graphClient,
                        repository,
                        stake,
                        latestDate.AddDays(-1).Round(),
                        DateTimeOffset.UtcNow.AddDays(1).Round(),
                        cancellationToken);

                    var lowestDate = await repository.GetLowestDateAsync(stake.ContractAddress)
                        ?? DateTimeOffset.UtcNow.Round();

                    if (lowestDate.Date != stake.InceptionDate.Date)
                    {
                        await SyncPerformanceAsync(
                            graphClient,
                            repository,
                            stake,
                            stake.InceptionDate,
                            lowestDate.AddDays(1).Round(),
                            cancellationToken);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error getting market data for {stake.Symbol}.");
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
                if (start >= DateTimeOffset.UtcNow)
                {
                    break;
                }

                var end = start.AddDays(MaxDays).Round();

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
                            .OrderBy(i => Math.Abs(i.Date.ToUnixTimeSeconds() - new DateTimeOffset(nav.Date, TimeSpan.Zero).ToUnixTimeSeconds()))
                            .FirstOrDefault();

                        var perf = new DataFundPerformance()
                        {
                            Address = fund.ContractAddress,
                            Date = nav.Date,
                            Nav = nav.NetAssetValuePerToken,
                            Price = closestPrice?.Price ?? -1,
                            MarketCap = closestPrice?.MarketCap ?? -1,
                            Volume = closestPrice?.Volume ?? -1
                        };

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
                    start = end;
                }
            }
        }

        private async Task SyncPerformanceAsync(
            IGraphClient graphClient,
            IFundPerformanceRepository repository,
            IStakeSettings stake,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            CancellationToken cancellationToken)
        {
            var start = new DateTimeOffset(startDate.Date, TimeSpan.Zero);

            while (!cancellationToken.IsCancellationRequested)
            {
                if (start >= DateTimeOffset.UtcNow)
                {
                    break;
                }

                var end = start.AddDays(MaxDays).Round();

                Console.WriteLine($"[{stake.ContractAddress}] Processing Batch: {start} -> {end}");

                var uniswapPrices = await graphClient
                    .ListUniswapTokenPerformanceAsync(stake.ContractAddress, start, end)
                    .ToListAsync(cancellationToken);

                foreach (var priceData in uniswapPrices)
                {
                    var perf = new DataFundPerformance()
                    {
                        Address = stake.ContractAddress,
                        Date = priceData.Date,
                        Nav = priceData.Price,
                        Price = priceData.Price,
                        MarketCap = priceData.MarketCap,
                        Volume = priceData.Volume
                    };

                    await repository.UploadItemsAsync(perf);
                }

                Console.WriteLine($"[{stake.ContractAddress}] Finished Batch: {start} -> {end}");

                if (end >= endDate)
                {
                    break;
                }
                else
                {
                    start = end;
                }
            }
        }
    }
}
