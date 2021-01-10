using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Configuration.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Data.Models;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Exceptions;

namespace Pseudonym.Crypto.Invictus.Funds.Services
{
    internal sealed class FundStakingPowerCachingService : BaseCachingService
    {
        private const decimal Precision = 0.000000001m;

        public FundStakingPowerCachingService(
            IOptions<AppSettings> appSettings,
            IServiceProvider serviceProvider)
            : base(appSettings, serviceProvider)
        {
        }

        protected override TimeSpan Interval => TimeSpan.FromHours(1);

        protected override async Task ProcessAsync(IServiceScope scope, CancellationToken cancellationToken)
        {
            var fundService = scope.ServiceProvider.GetRequiredService<IFundService>();
            var stakeService = scope.ServiceProvider.GetRequiredService<IStakeService>();
            var stakingRepository = scope.ServiceProvider.GetRequiredService<IStakingPowerRepository>();

            foreach (var stakeSettings in AppSettings.Stakes.Cast<IStakeSettings>())
            {
                try
                {
                    var latestDate = (await stakingRepository.GetLatestAsync(stakeSettings.ContractAddress))?.Date
                        ?? stakeSettings.InceptionDate;

                    await SyncStakingPowerAsync(
                        fundService,
                        stakeService,
                        stakingRepository,
                        stakeSettings,
                        latestDate == stakeSettings.InceptionDate
                            ? stakeSettings.InceptionDate
                            : latestDate.AddHours(-5).Round(),
                        DateTimeOffset.UtcNow.AddHours(-1).Round(),
                        cancellationToken);

                    var lowestDate = (await stakingRepository.GetLowestAsync(stakeSettings.ContractAddress))?.Date
                        ?? DateTimeOffset.UtcNow.Round();

                    if (lowestDate.Date != stakeSettings.InceptionDate.Date)
                    {
                        await SyncStakingPowerAsync(
                            fundService,
                            stakeService,
                            stakingRepository,
                            stakeSettings,
                            stakeSettings.InceptionDate,
                            lowestDate.AddDays(1).Round(),
                            cancellationToken);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error getting stake data for {stakeSettings.Symbol}.");
                    Console.WriteLine(e);
                }
            }
        }

        private static DateTimeOffset GetHourlyDate(DateTime date)
        {
            return new DateTimeOffset(date.Year, date.Month, date.Day, date.Hour, 0, 0, TimeSpan.Zero);
        }

        private async Task SyncStakingPowerAsync(
            IFundService fundService,
            IStakeService stakeService,
            IStakingPowerRepository stakingRepository,
            IStakeSettings stake,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            CancellationToken cancellationToken)
        {
            var currentDate = GetHourlyDate(startDate.UtcDateTime).UtcDateTime;
            var lastStakingPower = await stakingRepository.GetStakingPowerAsync(stake.ContractAddress, currentDate.AddHours(-1));
            var stakingEvents = await stakeService.ListStakeEventsAsync(stake.Symbol, startDate.UtcDateTime, endDate.UtcDateTime)
                .ToListAsync(cancellationToken);

            var hourlyGroups = stakingEvents
                .OrderBy(x => x.ConfirmedAt)
                .GroupBy(x => GetHourlyDate(x.ConfirmedAt))
                .ToList();

            do
            {
                try
                {
                    var fundPowers = new List<DataStakingPowerFund>();
                    var hourlyEvents = hourlyGroups.SingleOrDefault(g => g.Key == currentDate)
                        ?? Enumerable.Empty<IStakeEvent>();

                    foreach (var symbol in stake.FundMultipliers.Keys)
                    {
                        var fund = AppSettings.Funds
                            .Cast<IFundSettings>()
                            .Single(x => x.Symbol == symbol);

                        var events = lastStakingPower?.Breakdown.SingleOrDefault(x =>
                            x.ContractAddress.Equals(fund.ContractAddress.Address, StringComparison.OrdinalIgnoreCase))
                            ?.Events.ToList()
                            ?? new List<DataStakingEvent>();

                        events.AddRange(hourlyEvents
                            .Where(e =>
                                e.ContractAddress == fund.ContractAddress &&
                                e.Type == StakeEventType.Lockup)
                            .Select(e => new DataStakingEvent()
                            {
                                UserAddress = e.UserAddress,
                                StakedAt = e.ConfirmedAt,
                                Quantity = e.Change,
                                ExpiresAt = e.Lock.ExpiresAt,
                                TimeModifier = stake.TimeMultipliers
                                    .SingleOrDefault(tm =>
                                        tm.RangeMin <= e.Lock.Duration.Days &&
                                        tm.RangeMax >= e.Lock.Duration.Days)
                                    ?.Multiplier ?? 1
                            }));

                        foreach (var releaseEvent in hourlyEvents
                            .Where(e =>
                                e.ContractAddress == fund.ContractAddress &&
                                e.Type != StakeEventType.Lockup))
                        {
                            var approximateQuantity = releaseEvent.Release.Quantity + (releaseEvent.Release.FeeQuantity ?? decimal.Zero);
                            var userStakes = events
                                .Where(e => e.UserAddress.Equals(releaseEvent.UserAddress.Address, StringComparison.OrdinalIgnoreCase))
                                .ToList();

                            var lockUp = userStakes.Count > 0
                                ? userStakes.Count == 1
                                    ? userStakes.Single()
                                    : userStakes
                                        .OrderBy(x => x.StakedAt)
                                        .FirstOrDefault(e => Math.Abs(e.Quantity - approximateQuantity) <= Precision)
                                : throw new PermanentException($"No existing lockup data could be found for release event {releaseEvent.Hash}");

                            events.Remove(lockUp);
                        }

                        if (events.Any())
                        {
                            var prices = await fundService.ListPerformanceAsync(
                                    symbol,
                                    PriceMode.Raw,
                                    currentDate.AddDays(-1),
                                    currentDate.AddDays(1),
                                    CurrencyCode.USD)
                                .ToListAsync(cancellationToken);

                            var closestPrice = prices
                                .OrderBy(i => Math.Abs(
                                    new DateTimeOffset(i.Date, TimeSpan.Zero).ToUnixTimeSeconds() -
                                    new DateTimeOffset(currentDate, TimeSpan.Zero).ToUnixTimeSeconds()))
                                .FirstOrDefault()
                                    ?? throw new PermanentException($"No Price data could be found for date {currentDate}");

                            fundPowers.Add(new DataStakingPowerFund()
                            {
                                ContractAddress = fund.ContractAddress,
                                FundModifier = stake.FundMultipliers[symbol],
                                PricePerToken = closestPrice.NetAssetValuePerToken,
                                Events = events
                            });
                        }
                    }

                    lastStakingPower = new DataStakingPower()
                    {
                        Address = stake.ContractAddress,
                        Date = currentDate.AddHours(1),
                        Power = fundPowers.Sum(fp => fp.PricePerToken * fp.Events.Sum(fpe => fpe.Quantity * fpe.TimeModifier * fp.FundModifier)),
                        Breakdown = fundPowers,
                        Summary = fundPowers
                            .Select(fp => new DataStakingPowerSummary()
                            {
                                ContractAddress = fp.ContractAddress,
                                Power = fp.PricePerToken * fp.Events.Sum(fpe => fpe.Quantity * fpe.TimeModifier * fp.FundModifier)
                            })
                            .ToList()
                    };

                    await stakingRepository.UploadItemsAsync(lastStakingPower);
                }
                finally
                {
                    currentDate = currentDate.AddHours(1);
                }
            }
            while (!cancellationToken.IsCancellationRequested && currentDate < endDate.Round());
        }
    }
}
