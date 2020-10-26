using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Shared;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Exceptions;
using Pseudonym.Crypto.Invictus.Shared.Models;
using Pseudonym.Crypto.Invictus.Web.Server.Abstractions;

namespace Pseudonym.Crypto.Invictus.Web.Server.Clients
{
    internal sealed class ApiClient : IApiClient
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IScopedCancellationToken scopedCancellationToken;

        public ApiClient(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IScopedCancellationToken scopedCancellationToken)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.httpClientFactory = httpClientFactory;
            this.scopedCancellationToken = scopedCancellationToken;
        }

        public IAsyncEnumerable<ApiFund> ListFundsAsync()
        {
            return ListAsync<ApiFund>($"/api/v1/funds");
        }

        public Task<ApiFund> GetFundAsync(Symbol symbol)
        {
            return GetAsync<ApiFund>($"/api/v1/funds/{symbol}");
        }

        public IAsyncEnumerable<ApiPerformance> ListFundPerformanceAsync(Symbol symbol, DateTime fromDate, DateTime toDate)
        {
            return ListAsync<ApiPerformance>($"/api/v1/funds/{symbol}/performance?from={fromDate}&to={toDate}");
        }

        public Task<ApiPortfolio> ListPortfolioAsync(string address)
        {
            return GetAsync<ApiPortfolio>($"/api/v1/addresses/{address}");
        }

        public IAsyncEnumerable<ApiTransaction> ListTransactionsAsync(string address, Symbol symbol)
        {
            return ListAsync<ApiTransaction>($"/api/v1/addresses/{address}/transactions/{symbol}");
        }

        private async IAsyncEnumerable<TResponse> ListAsync<TResponse>(string url)
            where TResponse : class, new()
        {
            foreach (var item in await GetAsync<List<TResponse>>(url))
            {
                yield return item;
            }
        }

        private async Task<TResponse> GetAsync<TResponse>(string url)
            where TResponse : class, new()
        {
            try
            {
                using var client = httpClientFactory.CreateClient(nameof(ApiClient));

                client.DefaultRequestHeaders.TryAddWithoutValidation(Headers.CorrelationId, httpContextAccessor.HttpContext.TraceIdentifier);

                var response = await client.GetAsync(new Uri(url, UriKind.Relative), scopedCancellationToken.Token);

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<TResponse>(json);
            }
            catch (HttpRequestException e)
            {
                throw new TransientException($"{GetType().Name} Error calling GET {url}", e);
            }
        }
    }
}
