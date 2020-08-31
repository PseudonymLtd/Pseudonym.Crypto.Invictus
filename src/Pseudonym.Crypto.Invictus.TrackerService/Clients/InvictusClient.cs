using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Clients.Models;
using Pseudonym.Crypto.Invictus.TrackerService.Configuration;

namespace Pseudonym.Crypto.Invictus.TrackerService.Clients
{
    internal sealed class InvictusClient : IInvictusClient
    {
        private readonly AppSettings appSettings;
        private readonly IHttpClientFactory httpClientFactory;

        public InvictusClient(
            IOptions<AppSettings> appSettings,
            IHttpClientFactory httpClientFactory)
        {
            this.appSettings = appSettings.Value;
            this.httpClientFactory = httpClientFactory;
        }

        public async IAsyncEnumerable<InvictusFund> ListFundsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using var client = httpClientFactory.CreateClient(nameof(InvictusClient));

            var response = await client.GetAsync(new Uri($"/v2/funds", UriKind.Relative), cancellationToken);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var data = JsonConvert.DeserializeObject<ListFundsResponse>(json);

            foreach (var item in data.Funds)
            {
                yield return item;
            }
        }

        public async Task<InvictusFund> GetFundAsync(string fundName, CancellationToken cancellationToken)
        {
            using var client = httpClientFactory.CreateClient(nameof(InvictusClient));

            var response = await client.GetAsync(new Uri($"/v2/funds/{fundName}/nav", UriKind.Relative), cancellationToken);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var fund = JsonConvert.DeserializeObject<InvictusFund>(json);

            return fund;
        }
    }
}
