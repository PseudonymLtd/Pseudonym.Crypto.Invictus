using System.Net.Http;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Shared.Models;
using Pseudonym.Crypto.Invictus.Web.Client.Abstractions;

namespace Pseudonym.Crypto.Invictus.Web.Client.Clients
{
    internal sealed class HostClient : BaseClient, IHostClient
    {
        public HostClient(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        public Task<ApiLogin> LoginAsync()
        {
            return GetAsync<ApiLogin>("/api/v1/auth");
        }
    }
}
