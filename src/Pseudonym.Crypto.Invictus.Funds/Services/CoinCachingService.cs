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

namespace Pseudonym.Crypto.Invictus.Funds.Services
{
    internal sealed class CoinCachingService : BaseCachingService
    {
        public CoinCachingService(
            IOptions<AppSettings> appSettings,
            IServiceProvider serviceProvider)
            : base(appSettings, serviceProvider)
        {
        }

        protected override TimeSpan Interval => TimeSpan.FromHours(1);

        protected override async Task ProcessAsync(IServiceScope scope, CancellationToken cancellationToken)
        {
            var coinGeckoClient = scope.ServiceProvider.GetRequiredService<ICoinGeckoClient>();

            foreach (var fund in AppSettings.Funds.Where(x => x.Tradable))
            {
                try
                {
                    var coins = await GetCoinsAsync(coinGeckoClient, cancellationToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error getting market data for {fund.Symbol}.");
                    Console.WriteLine(e);
                }
            }
        }

        private async Task<HashSet<CoinGeckoCoin>> GetCoinsAsync(ICoinGeckoClient coinGeckoClient, CancellationToken cancellationToken)
        {
            var hashSet = new HashSet<CoinGeckoCoin>();

            await foreach (var coin in coinGeckoClient
                .ListCoinsAsync()
                .WithCancellation(cancellationToken))
            {
                hashSet.Add(coin);
            }

            return hashSet;
        }
    }
}
