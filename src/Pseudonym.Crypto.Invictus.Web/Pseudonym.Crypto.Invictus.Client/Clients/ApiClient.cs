using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Pseudonym.Crypto.Invictus.Shared;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Models;
using Pseudonym.Crypto.Invictus.Shared.Models.Filters;
using Pseudonym.Crypto.Invictus.Web.Client.Abstractions;

namespace Pseudonym.Crypto.Invictus.Web.Client.Clients
{
    internal sealed class ApiClient : BaseClient, IApiClient
    {
        private readonly IHostClient hostClient;
        private readonly IUserSettings userSettings;
        private readonly ISessionStore sessionStore;

        public ApiClient(
            IHostClient hostClient,
            ISessionStore sessionStore,
            IUserSettings userSettings,
            IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
            this.hostClient = hostClient;
            this.sessionStore = sessionStore;
            this.userSettings = userSettings;
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
            return ListAsync<ApiPerformance>(string.Format(
                    "/api/v1/funds/{0}/performance?{1}={2}&{3}={4}",
                    symbol,
                    ApiPerformanceQueryFilter.FromQueryName,
                    fromDate.ToString(Format.DateFormat),
                    ApiPerformanceQueryFilter.ToQueryName,
                    toDate.ToString(Format.DateFormat)));
        }

        public IAsyncEnumerable<ApiInvestment> ListInvestmentsAsync()
        {
            if (userSettings.HasValidAddress())
            {
                return ListAsync<ApiInvestment>($"/api/v1/addresses/{userSettings.WalletAddress}/investments");
            }
            else
            {
                return new EmptyAsyncEnumerable<ApiInvestment>();
            }
        }

        public Task<ApiInvestment> GetInvestmentAsync(Symbol symbol)
        {
            return GetAsync<ApiInvestment>($"/api/v1/addresses/{userSettings.WalletAddress}/investments/{symbol}");
        }

        public IAsyncEnumerable<ApiTransaction> ListInvestmentTransactionsAsync(Symbol symbol)
        {
            if (userSettings.HasValidAddress())
            {
                return ListAsync<ApiTransaction>($"/api/v1/addresses/{userSettings.WalletAddress}/investments/{symbol}/transactions");
            }
            else
            {
                return new EmptyAsyncEnumerable<ApiTransaction>();
            }
        }

        protected override async Task<TResponse> GetAsync<TResponse>(string url)
        {
            return await base.GetAsync<TResponse>(
                QueryHelpers.AddQueryString(url, ApiCurrencyQueryFilter.CurrencyQueryName, userSettings.CurrencyCode.ToString()));
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
    }
}
