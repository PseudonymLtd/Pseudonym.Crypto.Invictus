﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;

namespace Pseudonym.Crypto.Invictus.Shared.Hosting
{
    internal sealed class RequestExceptionMiddleware : IMiddleware
    {
        private readonly IExceptionHandler exceptionHandler;

        public RequestExceptionMiddleware(IExceptionHandler exceptionHandler)
        {
            this.exceptionHandler = exceptionHandler;
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
        }
    }
}
