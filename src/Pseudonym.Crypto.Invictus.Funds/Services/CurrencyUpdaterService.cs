using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Services
{
    internal sealed class CurrencyUpdaterService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;

        public CurrencyUpdaterService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();

                    var client = scope.ServiceProvider.GetRequiredService<ICurrencyClient>();
                    var converter = scope.ServiceProvider.GetRequiredService<CurrencyConverter>();

                    var rates = await client.GetRatesAsync(cancellationToken);

                    converter.UpdateRates(rates);

                    var nextUpdateInterval = rates.NextUpdate.Subtract(DateTimeOffset.UtcNow);

                    await Task.Delay(nextUpdateInterval, cancellationToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}
