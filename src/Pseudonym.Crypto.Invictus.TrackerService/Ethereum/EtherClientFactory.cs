using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Configuration;
using Pseudonym.Crypto.Invictus.TrackerService.Hosting.Models;

namespace Pseudonym.Crypto.Invictus.TrackerService.Ethereum
{
    internal sealed class EtherClientFactory : IEtherClientFactory
    {
        private readonly IOptions<AppSettings> appSettings;
        private readonly IScopedCorrelation scopedCorrelation;

        public EtherClientFactory(
            IOptions<AppSettings> appSettings,
            IScopedCorrelation scopedCorrelation)
        {
            this.appSettings = appSettings;
            this.scopedCorrelation = scopedCorrelation;
        }

        public IEtherClient CreateClient()
        {
            var httpClient = new HttpClient();
            var byteArray = Encoding.ASCII.GetBytes($":{appSettings.Value.Infuria.ProjectSecret}");

            httpClient.Timeout = TimeSpan.FromSeconds(30);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(Headers.Origin, appSettings.Value.Infuria.Origin);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(Headers.CorrelationId, scopedCorrelation.CorrelationId);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(appSettings.Value.Infuria.UserAgent, $"v{appSettings.Value.Version.ToString(3)}"));

            return new EtherClient(
                new Uri($"https://mainnet.infura.io/v3/{appSettings.Value.Infuria.ProjectId}", UriKind.Absolute),
                httpClient);
        }
    }
}
