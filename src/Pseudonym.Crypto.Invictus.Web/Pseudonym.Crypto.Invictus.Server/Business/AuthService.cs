using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Shared;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Exceptions;
using Pseudonym.Crypto.Invictus.Shared.Models;
using Pseudonym.Crypto.Invictus.Web.Server.Abstractions;
using Pseudonym.Crypto.Invictus.Web.Server.Configuration;

namespace Pseudonym.Crypto.Invictus.Web.Server.Business
{
    internal sealed class AuthService : IAuthService
    {
        private readonly AppSettings appSettings;
        private readonly IScopedCorrelation scopedCorrelation;
        private readonly IScopedCancellationToken scopedCancellationToken;

        public AuthService(
            IOptions<AppSettings> appSettings,
            IScopedCorrelation scopedCorrelation,
            IScopedCancellationToken scopedCancellationToken)
        {
            this.appSettings = appSettings.Value;
            this.scopedCorrelation = scopedCorrelation;
            this.scopedCancellationToken = scopedCancellationToken;
        }

        public async Task<ApiLogin> LoginAsync()
        {
            try
            {
                using var client = new HttpClient();

                client.BaseAddress = appSettings.ApiUrl;
                client.Timeout = TimeSpan.FromSeconds(10);
                client.DefaultRequestHeaders.TryAddWithoutValidation(Headers.Origin, appSettings.HostUrl.OriginalString);
                client.DefaultRequestHeaders.TryAddWithoutValidation(Headers.CorrelationId, scopedCorrelation.CorrelationId);
                client.DefaultRequestHeaders.TryAddWithoutValidation(Headers.ApiKey, appSettings.ApiKey);
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(appSettings.ServiceName, $"v{appSettings.Version}"));

                var response = await client.GetAsync(new Uri("/api/v1/auth", UriKind.Relative), scopedCancellationToken.Token);

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<ApiLogin>(json);
            }
            catch (HttpRequestException e)
            {
                throw new TransientException($"Error getting authentication details", e);
            }
        }
    }
}
