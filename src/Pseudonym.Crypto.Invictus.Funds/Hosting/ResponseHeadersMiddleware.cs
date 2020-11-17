using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Pseudonym.Crypto.Invictus.Shared;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Models.Filters;

namespace Pseudonym.Crypto.Invictus.Funds.Hosting
{
    internal sealed class ResponseHeadersMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var currencyCode = context.Request.Query.ContainsKey(ApiCurrencyQueryFilter.CurrencyQueryName) &&
                Enum.TryParse(context.Request.Query[ApiCurrencyQueryFilter.CurrencyQueryName].First(), out CurrencyCode cc)
                ? cc
                : CurrencyCode.USD;

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