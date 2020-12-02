using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Pseudonym.Crypto.Invictus.Shared;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Models;

namespace Pseudonym.Crypto.Invictus.Funds.Hosting
{
    internal sealed class ResponseHeadersMiddleware : IMiddleware
    {
        private readonly IScopedCorrelation scopedCorrelation;

        public ResponseHeadersMiddleware(IScopedCorrelation scopedCorrelation)
        {
            this.scopedCorrelation = scopedCorrelation;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var currencyCode = context.Request.Query.ContainsKey(ApiFilterNames.CurrencyQueryName) &&
                Enum.TryParse(context.Request.Query[ApiFilterNames.CurrencyQueryName].First(), out CurrencyCode cc)
                ? cc
                : CurrencyCode.USD;

            if (context.Response.Headers.ContainsKey(Headers.CorrelationId))
            {
                context.Response.Headers[Headers.CorrelationId] = scopedCorrelation.CorrelationId;
            }
            else
            {
                context.Response.Headers.Add(Headers.CorrelationId, scopedCorrelation.CorrelationId);
            }

            if (context.Response.Headers.ContainsKey(Headers.CurrencyCode))
            {
                context.Response.Headers[Headers.CurrencyCode] = currencyCode.ToString();
            }
            else
            {
                context.Response.Headers.TryAdd(Headers.CurrencyCode, currencyCode.ToString());
            }

            await next(context);
        }
    }
}