using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Models;
using Pseudonym.Crypto.Invictus.Web.Client.Abstractions;
using Pseudonym.Crypto.Invictus.Web.Client.Configuration;

namespace Pseudonym.Crypto.Invictus.Web.Client.Clients
{
    internal sealed class ApiClient : BaseClient, IApiClient
    {
        private readonly IHostClient hostClient;
        private readonly ISessionStore sessionStore;

        public ApiClient(
            IHostClient hostClient,
            ISessionStore sessionStore,
            IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
            this.hostClient = hostClient;
            this.sessionStore = sessionStore;
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

        protected override async Task<TResponse> GetAsync<TResponse>(string url)
        {
            var currencyCode = await GetCurrencyCodeAsync();

            return await base.GetAsync<TResponse>(
                QueryHelpers.AddQueryString(url, "output-currency", currencyCode.ToString()));
        }

        protected override async Task<HttpClient> CreateClientAsync()
        {
            var client = await base.CreateClientAsync();

            var item = await sessionStore.GetAsync<ApiLogin>(StoreKeys.JwtToken);
            if (item == null || item.ExpiresAt < DateTime.UtcNow)
            {
                item = await hostClient.LoginAsync();

                await sessionStore.SetAsync(StoreKeys.JwtToken, item);
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", item.AccessToken);

            return client;
        }

        private async IAsyncEnumerable<TResponse> ListAsync<TResponse>(string url)
            where TResponse : class, new()
        {
            foreach (var item in await GetAsync<List<TResponse>>(url))
            {
                yield return item;
            }
        }

        private async Task<CurrencyCode> GetCurrencyCodeAsync()
        {
            var userSettings = await sessionStore.GetAsync<UserSettings>(StoreKeys.UserSettings)
                ?? new UserSettings();

            return userSettings.CurrencyCode;
        }
    }
}
