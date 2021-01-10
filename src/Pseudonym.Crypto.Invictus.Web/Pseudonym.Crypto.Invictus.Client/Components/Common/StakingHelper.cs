using System;
using System.Collections.Generic;
using System.Linq;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Exceptions;
using Pseudonym.Crypto.Invictus.Shared.Models;

namespace Pseudonym.Crypto.Invictus.Web.Client.Components.Common
{
    public static class StakingHelper
    {
        public const int IntervalsPerWeek = 336;
        public const int IntervalMinutes = 30;
        public const decimal DepreciationPercentage = 0.975m;
        public const decimal MinDistribution = 1000;
        public const decimal Precision = 0.000000001m;

        public static readonly DateTime StartDate = new DateTime(2021, 01, 04, 0, 0, 0, DateTimeKind.Utc);

        public static decimal CalculateEarnings(ApiStake stake, ApiFund fund, IReadOnlyList<ApiStakeEvent> events, DateTime toDate)
        {
            return CalculateEarnings(
                stake,
                fund,
                events,
                (int)((toDate - StartDate).TotalHours * 2));
        }

        public static decimal CalculateEarnings(ApiStake stake, ApiFund fund, IReadOnlyList<ApiStakeEvent> events, int intervals)
        {
            var icap = decimal.Zero;
            var stakingPower = decimal.Zero;
            var icapDistribution = 10000m;
            var endDate = StartDate.AddMinutes(IntervalMinutes * intervals);

            if (events.Any() && intervals > 0)
            {
                var eventQueue = new Queue<ApiStakeEvent>(events.OrderBy(x => x.ConfirmedAt));
                if (eventQueue.Count == 0)
                {
                    return decimal.Zero;
                }
                else
                {
                    var eventItem = eventQueue.Dequeue();

                    do
                    {
                        var endItem = eventQueue.Count > 0
                            ? eventQueue.Dequeue()
                            : null;

                        stakingPower += CalculatePowerDifference(stake, fund, events, eventItem);

                        var relativePercentage = stakingPower / stake.Power.Power * 100;

                        var start = NormalizeDate(eventItem.ConfirmedAt);
                        var end = endItem != null
                            ? NormalizeDate(endItem.ConfirmedAt)
                            : endDate;

                        var intervalsBeforeStart = ((decimal)(start - StartDate).TotalHours) * 2m;
                        var chargeableIntervals = Math.Max(intervals - intervalsBeforeStart, decimal.Zero);
                        var remainder = intervalsBeforeStart % IntervalsPerWeek;
                        var depreciations = (intervalsBeforeStart - remainder) / IntervalsPerWeek;
                        var intervalsInsideWindow = Math.Min(chargeableIntervals, IntervalsPerWeek - remainder);

                        icapDistribution = Math.Max(icapDistribution * (decimal)Math.Pow((double)DepreciationPercentage, (double)depreciations), MinDistribution);

                        // Add Remaining Ticks of this distribution
                        icap += (icapDistribution / IntervalsPerWeek) / 100 * relativePercentage * intervalsInsideWindow;

                        var ticks = ((decimal)(end - start).TotalHours * 2m) - intervalsInsideWindow;
                        var endRemainder = ticks % IntervalsPerWeek;
                        var fullRounds = (ticks - endRemainder) / IntervalsPerWeek;

                        icap += Enumerable
                            .Range(0, (int)fullRounds)
                            .Sum(_ =>
                            {
                                icapDistribution = Math.Max(icapDistribution * DepreciationPercentage, MinDistribution);

                                return icapDistribution / 100 * relativePercentage;
                            });

                        if (endRemainder != default)
                        {
                            var nextIcapDistribution = Math.Max(icapDistribution * DepreciationPercentage, MinDistribution);

                            icap += nextIcapDistribution / IntervalsPerWeek / 100 * relativePercentage * endRemainder;
                        }

                        eventItem = endItem;
                    }
                    while (eventQueue.Count > 0);
                }
            }

            return Math.Max(icap, decimal.Zero);
        }

        public static decimal CalculatePowerDifference(ApiStake stake, ApiFund fund, IReadOnlyList<ApiStakeEvent> events, ApiStakeEvent eventItem)
        {
            if (eventItem.Type == StakeEventType.Lockup)
            {
                return CalculatePower(stake, fund, eventItem);
            }
            else
            {
                var approximateQuantity = eventItem.Release.Quantity + (eventItem.Release.FeeQuantity ?? decimal.Zero);

                var items = events
                    .Where(x => x.Type == StakeEventType.Lockup)
                    .ToList();

                var lockUp = items.Count > 0
                    ? items.Count == 1
                        ? items.Single()
                        : items
                            .OrderBy(x => x.ConfirmedAt)
                            .FirstOrDefault(e => Math.Abs(e.Lock.Quantity - approximateQuantity) <= Precision)
                    : throw new PermanentException($"No existing lockup data could be found for release event {eventItem.Hash}");

                return -CalculatePower(stake, fund, lockUp);
            }
        }

        public static DateTime NormalizeDate(DateTime dateTime)
        {
            return dateTime < StartDate
                ? StartDate
                : new DateTime(
                    dateTime.Year,
                    dateTime.Month,
                    dateTime.Day,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc)
                .Add(TimeSpan.FromMinutes(
                    IntervalMinutes * Math.Ceiling(dateTime.TimeOfDay.TotalMinutes / IntervalMinutes)));
        }

        private static decimal CalculatePower(ApiStake stake, ApiFund fund, ApiStakeEvent eventItem)
        {
            var timeModifier = stake.TimeMultipliers
                .SingleOrDefault(tm =>
                    tm.RangeMin <= eventItem.Lock.Duration.Days &&
                    tm.RangeMax >= eventItem.Lock.Duration.Days)
                ?.Multiplier ?? 1m;

            var fundModifier = stake.FundMultipliers.ContainsKey(fund.Token.Symbol)
                ? stake.FundMultipliers[fund.Token.Symbol]
                : 1m;

            return eventItem.Lock.Quantity * timeModifier * fundModifier * fund.Nav.PricePerToken;
        }
    }
}
