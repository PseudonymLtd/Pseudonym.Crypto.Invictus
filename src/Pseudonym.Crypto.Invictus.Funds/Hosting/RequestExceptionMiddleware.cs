using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Hosting.Models;

namespace Pseudonym.Crypto.Invictus.Funds.Hosting
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
                context.Response.Headers.Add(Headers.CorrelationId, scopedCorrelation.CorrelationId);
            }
        }
    }
}
