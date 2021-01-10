using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Exceptions;

namespace Pseudonym.Crypto.Invictus.Funds.Clients
{
    internal abstract class BaseHttpClient
    {
        private readonly IScopedCancellationToken scopedCancellationToken;
        private readonly IHttpClientFactory httpClientFactory;

        public BaseHttpClient(
            IScopedCancellationToken scopedCancellationToken,
            IHttpClientFactory httpClientFactory)
        {
            this.scopedCancellationToken = scopedCancellationToken;
            this.httpClientFactory = httpClientFactory;

            AugmentUriFunc = x => x;
        }

        protected CancellationToken CancellationToken => scopedCancellationToken.Token;

        protected virtual Func<string, string> AugmentUriFunc { get; }

        protected async Task<TResponse> GetAsync<TResponse>(string url)
            where TResponse : class, new()
        {
            try
            {
                using var client = httpClientFactory.CreateClient(GetType().Name);

                var response = await client.GetAsync(new Uri(AugmentUriFunc.Invoke(url), UriKind.Relative), CancellationToken);

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                return json.Deserialize<TResponse>();
            }
            catch (HttpRequestException e)
            {
                throw new TransientException($"{GetType().Name} Error calling GET {url}", e);
            }
        }

        protected async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest request)
            where TResponse : class, new()
        {
            try
            {
                using var client = httpClientFactory.CreateClient(GetType().Name);

                var response = await client.PostAsync(
                    new Uri(AugmentUriFunc.Invoke(url), UriKind.Relative),
                    new StringContent(request.Serialize(), Encoding.UTF8, MediaTypeNames.Application.Json),
                    CancellationToken);

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                return json.Deserialize<TResponse>();
            }
            catch (HttpRequestException e)
            {
                throw new TransientException($"{GetType().Name} Error calling GET {url}", e);
            }
        }
    }
}
