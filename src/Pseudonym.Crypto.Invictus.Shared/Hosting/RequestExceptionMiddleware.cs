using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;

namespace Pseudonym.Crypto.Invictus.Shared.Hosting
{
    internal sealed class RequestExceptionMiddleware : IMiddleware
    {
        private readonly IExceptionHandler exceptionHandler;
        private readonly IScopedCorrelation scopedCorrelation;

        public RequestExceptionMiddleware(
            IExceptionHandler exceptionHandler,
            IScopedCorrelation scopedCorrelation)
        {
            this.exceptionHandler = exceptionHandler;
            this.scopedCorrelation = scopedCorrelation;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception e)
            {
                await exceptionHandler.HandleAsync(context, e);
            }
            finally
            {
                if (context.Response.Headers.ContainsKey(Headers.CorrelationId))
                {
                    context.Response.Headers[Headers.CorrelationId] = scopedCorrelation.CorrelationId;
                }
                else
                {
                    context.Response.Headers.Add(Headers.CorrelationId, scopedCorrelation.CorrelationId);
                }
            }
        }
    }
}
