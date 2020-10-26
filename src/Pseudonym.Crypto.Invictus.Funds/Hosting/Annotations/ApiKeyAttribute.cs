using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Shared;

namespace Microsoft.AspNetCore.Authorization
{
    public class ApiKeyAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var appSettings = context.HttpContext.RequestServices.GetRequiredService<IOptions<AppSettings>>();

            if (context.HttpContext.Request.Headers.ContainsKey(Headers.ApiKey) &&
                context.HttpContext.Request.Headers[Headers.ApiKey] == appSettings.Value.ApiKey)
            {
                await next();
            }
            else
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
