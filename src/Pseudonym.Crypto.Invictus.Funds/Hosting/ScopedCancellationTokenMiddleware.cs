using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Pseudonym.Crypto.Invictus.Funds.Hosting.Models;

namespace Pseudonym.Crypto.Invictus.Funds.Hosting
{
    internal sealed class ScopedCancellationTokenMiddleware : IMiddleware
    {
        private readonly ScopedCancellationToken scopedCancellationToken;

        public ScopedCancellationTokenMiddleware(ScopedCancellationToken scopedCancellationToken)
        {
            this.scopedCancellationToken = scopedCancellationToken;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            scopedCancellationToken.SetToken(context.RequestAborted);

            await next(context);
        }
    }
}
