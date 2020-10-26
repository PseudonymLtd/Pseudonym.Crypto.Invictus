using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Pseudonym.Crypto.Invictus.Shared.Hosting.Models;

namespace Pseudonym.Crypto.Invictus.Shared.Hosting
{
    internal sealed class ScopedCorrelationIdMiddleware : IMiddleware
    {
        private readonly ScopedCorrelation scopedCorrelation;

        public ScopedCorrelationIdMiddleware(ScopedCorrelation scopedCorrelation)
        {
            this.scopedCorrelation = scopedCorrelation;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var correlationId = Guid.NewGuid().ToString();

            if (context.Request.Headers.ContainsKey(Headers.CorrelationId))
            {
                correlationId = $"{context.Request.Headers[Headers.CorrelationId]}:{correlationId}";
            }

            scopedCorrelation.SetCorrelationId(correlationId);

            await next(context);
        }
    }
}
