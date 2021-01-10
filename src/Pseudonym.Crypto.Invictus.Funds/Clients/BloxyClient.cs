using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Bloxy;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Clients
{
    internal sealed class BloxyClient : BaseHttpClient, IBloxyClient
    {
        private const int PageSize = 1000;

        private readonly Dependencies dependencies;

        public BloxyClient(
            IOptions<Dependencies> dependencies,
            IScopedCancellationToken scopedCancellationToken,
            IHttpClientFactory httpClientFactory)
            : base(scopedCancellationToken, httpClientFactory)
        {
            this.dependencies = dependencies.Value;
        }

        protected override Func<string, string> AugmentUriFunc =>
            uri => QueryHelpers.AddQueryString(uri, "key", dependencies.Bloxy.Settings.ApiKey);

        public async IAsyncEnumerable<BloxyTokenTransfer> ListTransactionsAsync(EthereumAddress contractAddress, DateTime startDate, DateTime endDate)
        {
            var page = 0;

            while (!CancellationToken.IsCancellationRequested)
            {
                var transactions = await GetAsync<List<BloxyTokenTransfer>>(
                    $"/token/transfers?token={contractAddress}&limit={PageSize}&offset={page++ * PageSize}&from_time={startDate.ToISO8601String()}&till_time={endDate.ToISO8601String()}");

                foreach (var transaction in transactions.Distinct())
                {
                    yield return transaction;
                }

                if (transactions.Count < PageSize)
                {
                    break;
                }
            }
        }
    }
}
