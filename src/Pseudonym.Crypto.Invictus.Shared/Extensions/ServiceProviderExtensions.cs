using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Pseudonym.Crypto.Invictus.Shared.Hosting.Models;

namespace System
{
    public static class ServiceProviderExtensions
    {
        public static IServiceScope CreateScope(this IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var scope = serviceProvider.CreateScope();

            scope.ServiceProvider.GetRequiredService<ScopedCorrelation>().SetCorrelationId(Guid.NewGuid().ToString());
            scope.ServiceProvider.GetRequiredService<ScopedCancellationToken>().SetToken(cancellationToken);

            return scope;
        }
    }
}
