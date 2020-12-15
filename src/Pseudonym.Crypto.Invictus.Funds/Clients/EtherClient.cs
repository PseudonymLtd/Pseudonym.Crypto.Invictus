using System.Net.Http;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Clients
{
    internal sealed class EtherClient : BaseRpcClient, IEtherClient
    {
        public EtherClient(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory, nameof(EtherClient), true)
        {
        }
    }
}
