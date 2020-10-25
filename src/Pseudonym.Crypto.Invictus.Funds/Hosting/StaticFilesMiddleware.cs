using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Pseudonym.Crypto.Invictus.Funds.Hosting
{
    public class StaticFilesMiddleware : IMiddleware
    {
        private const string EmbeddedFileNamespace = "Pseudonym.Crypto.Invictus.Funds.Resources";

        private static readonly Assembly Assembly = typeof(StaticFilesMiddleware).GetTypeInfo().Assembly;

        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly ILoggerFactory loggerFactory;

        public StaticFilesMiddleware(
            IWebHostEnvironment webHostEnvironment,
            ILoggerFactory loggerFactory)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.loggerFactory = loggerFactory;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var staticFileOptions = new StaticFileOptions
            {
                RequestPath = "/resources",
                FileProvider = new EmbeddedFileProvider(Assembly, EmbeddedFileNamespace),
            };

            var staticMiddleware = new StaticFileMiddleware(
                next,
                webHostEnvironment,
                Options.Create(staticFileOptions),
                loggerFactory);

            await staticMiddleware.Invoke(context);
        }
    }
}
