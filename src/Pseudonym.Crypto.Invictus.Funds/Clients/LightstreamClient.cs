using System.Net.Http;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Clients
{
    internal sealed class LightstreamClient : BaseRpcClient, ILightstreamClient
    {
        public LightstreamClient(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory, nameof(LightstreamClient), false)
        {
        }
    }
}
