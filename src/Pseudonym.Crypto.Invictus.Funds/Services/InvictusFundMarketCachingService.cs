﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.CoinGecko;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Invictus;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Data.Models;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Services
{
    internal sealed class InvictusFundMarketCachingService : BaseCachingService
    {
        private const int MaxDays = 7;

        public InvictusFundMarketCachingService(
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

            foreach (var fund in AppSettings.Funds)
            {
                try
                {
                    var latestDate = await repository.GetLatestDateAsync(fund.Address)
                        ?? fund.InceptionDate;

                    await UpdatePerformanceAsync(
                        coinGeckoClient,
                        invictusClient,
                        repository,
                        fund,
                        latestDate.AddDays(-1),
                        DateTimeOffset.UtcNow.AddDays(1).Round(),
                        cancellationToken);

                    var lowestDate = await repository.GetLowestDateAsync(fund.Address)
                        ?? DateTimeOffset.UtcNow.Round();

                    await UpdatePerformanceAsync(
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

        private async Task UpdatePerformanceAsync(
            ICoinGeckoClient coinGeckoClient,
            IInvictusClient invictusClient,
            IFundPerformanceRepository repository,
            FundSettings fund,
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

                Console.WriteLine($"[{fund.Address}] Processing Batch: {start} -> {end}");

                var invictusNavs = await invictusClient
                    .ListPerformanceAsync(fund.Symbol, start, end)
                    .ToListAsync(cancellationToken);

                if (invictusNavs.Any())
                {
                    if (fund.Tradable)
                    {
                        await foreach (var marketPrice in coinGeckoClient
                            .ListCoinPerformanceAsync(fund.CoinGeckoId, start, end)
                            .WithCancellation(cancellationToken))
                        {
                            var closestNav = invictusNavs
                                .OrderBy(i => Math.Abs(i.Date.ToUnixTimeSeconds() - marketPrice.Date.ToUnixTimeSeconds()))
                                .First();

                            var perf = MapPerformance(fund.Address, closestNav, marketPrice);

                            await repository.UploadItemsAsync(perf);
                        }
                    }
                    else
                    {
                        foreach (var nav in invictusNavs)
                        {
                            var perf = MapPerformance(fund.Address, nav, null);

                            await repository.UploadItemsAsync(perf);
                        }
                    }
                }

                Console.WriteLine($"[{fund.Address}] Finished Batch: {start} -> {end}");

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
                Date = marketData?.Date.UtcDateTime ?? navData.Date.UtcDateTime,
                Nav = navData.NetAssetValuePerToken.FromPythonString(),
                Price = marketData?.Price ?? -1,
                MarketCap = marketData?.MarketCap ?? -1,
                Volume = marketData?.Volume ?? -1
            };
        }
    }
}
