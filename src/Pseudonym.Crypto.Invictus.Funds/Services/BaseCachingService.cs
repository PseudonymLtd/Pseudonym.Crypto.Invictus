using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.Funds.Configuration;

namespace Pseudonym.Crypto.Invictus.Funds.Services
{
    internal abstract class BaseCachingService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;

        protected BaseCachingService(
            IOptions<AppSettings> appSettings,
            IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;

            AppSettings = appSettings.Value;
        }

        protected AppSettings AppSettings { get; }

        protected abstract TimeSpan Interval { get; }

        protected sealed override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = serviceProvider.CreateScope(cancellationToken);

                    await ProcessAsync(scope, cancellationToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    await Task.Delay(Interval, cancellationToken);
                }
            }
        }

        protected abstract Task ProcessAsync(IServiceScope scope, CancellationToken cancellationToken);
    }
}
