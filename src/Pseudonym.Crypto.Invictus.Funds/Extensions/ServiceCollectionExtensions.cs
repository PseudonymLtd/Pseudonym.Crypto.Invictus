using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Clients;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Shared;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfuriaClient(this IServiceCollection container)
        {
            container.AddScoped<IEtherClient, EtherClient>()
                .AddHttpClient(nameof(EtherClient), (sp, client) =>
                {
                    var dependency = ConfigureHttpClient(sp, client, d => d.Infuria);

                    var byteArray = Encoding.ASCII.GetBytes($":{dependency.Settings.ProjectSecret}");

                    client.BaseAddress = new Uri($"{dependency.Url.OriginalString}/v3/{dependency.Settings.ProjectId}", UriKind.Absolute);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                });

            return container;
        }

        public static IServiceCollection AddInvictusClient(this IServiceCollection container)
        {
            container.AddScoped<IInvictusClient, InvictusClient>()
                .AddHttpClient(nameof(InvictusClient), (sp, client) => ConfigureHttpClient(sp, client, d => d.Invictus));

            return container;
        }

        public static IServiceCollection AddExchangeRateClient(this IServiceCollection container)
        {
            container.AddScoped<ICurrencyClient, CurrencyClient>()
                .AddHttpClient(nameof(CurrencyClient), (sp, client) => ConfigureHttpClient(sp, client, d => d.ExchangeRate));

            return container;
        }

        private static Dependency<TSettings> ConfigureHttpClient<TSettings>(
            IServiceProvider serviceProvider, HttpClient client, Func<Dependencies, Dependency<TSettings>> selector)
            where TSettings : DependencySettings, new()
        {
            var appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
            var dependency = selector(serviceProvider.GetRequiredService<IOptions<Dependencies>>().Value);

            client.BaseAddress = dependency.Url;
            client.Timeout = dependency.Settings.Timeout;
            client.DefaultRequestHeaders.TryAddWithoutValidation(Headers.Origin, appSettings.HostUrl.OriginalString);
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(appSettings.ServiceName, $"v{appSettings.Version.ToString(3)}"));

            return dependency;
        }
    }
}
