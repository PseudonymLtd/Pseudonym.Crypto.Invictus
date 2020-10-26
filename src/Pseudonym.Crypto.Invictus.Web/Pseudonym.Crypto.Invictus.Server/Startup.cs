using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.Shared;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Hosting;
using Pseudonym.Crypto.Invictus.Shared.Hosting.Models;
using Pseudonym.Crypto.Invictus.Web.Server.Abstractions;
using Pseudonym.Crypto.Invictus.Web.Server.Clients;
using Pseudonym.Crypto.Invictus.Web.Server.Configuration;
using Pseudonym.Crypto.Invictus.Web.Server.Hosting;

namespace Pseudonym.Crypto.Invictus.Web.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection container)
        {
            container.Configure<AppSettings>(Configuration.GetSection(nameof(AppSettings)));

            container.AddControllersWithViews();
            container.AddRazorPages();

            container.AddSingleton<IEnvironmentNameAccessor, EnvironmentNameAccessor>();
            container.AddScoped<IExceptionHandler, ExceptionHandler>();

            container
                .AddScoped<ScopedCorrelation>()
                .AddTransient<IScopedCorrelation>(sp => sp.GetRequiredService<ScopedCorrelation>());

            container
                .AddScoped<ScopedCancellationToken>()
                .AddTransient<IScopedCancellationToken>(sp => sp.GetRequiredService<ScopedCancellationToken>());

            container.AddScoped<KestralResponseBodyMiddleware>();
            container.AddScoped<ScopedCancellationTokenMiddleware>();
            container.AddScoped<ScopedCorrelationIdMiddleware>();
            container.AddScoped<RequestExceptionMiddleware>();

            container.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });

            container.AddHttpContextAccessor();

            container.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.MimeTypes = new List<string>()
                {
                    "application/javascript",
                    "application/json",
                    "application/xml",
                    "application/pdf",
                    "text/css",
                    "text/html",
                    "text/json",
                    "text/plain",
                    "text/xml"
                };
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });

            container
                .AddScoped<IApiClient, ApiClient>()
                .AddHttpClient(
                    nameof(ApiClient),
                    (sp, client) =>
                    {
                        var settings = sp.GetRequiredService<IOptions<AppSettings>>();

                        client.BaseAddress = settings.Value.ApiUrl;
                        client.Timeout = TimeSpan.FromSeconds(10);
                        client.DefaultRequestHeaders.TryAddWithoutValidation(Headers.ApiKey, settings.Value.ApiKey);
                        client.DefaultRequestHeaders.TryAddWithoutValidation(Headers.Origin, settings.Value.HostUrl.OriginalString);
                        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(settings.Value.ServiceName, $"v{settings.Value.Version.ToString(3)}"));
                    });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler(new ExceptionHandlerOptions()
                {
                    ExceptionHandler = new RequestDelegate(async (context) =>
                    {
                        try
                        {
                            var errorContainer = context.Features.Get<IExceptionHandlerFeature>();
                            var exceptionHandler = context.RequestServices.GetRequiredService<IExceptionHandler>();

                            if (errorContainer?.Error != null)
                            {
                                await exceptionHandler.HandleAsync(context, errorContainer.Error);
                            }
                        }
                        finally
                        {
                            context.Response.Body?.Seek(0, SeekOrigin.Begin);
                        }
                    })
                });
            }

            app.UseRouting();

            app.UseCors(options =>
                options
                .WithOrigins(Configuration
                    .GetSection("Cors")
                    .GetChildren()
                    .Select(x => x.Value)
                    .ToArray())
                .WithExposedHeaders(Headers.CorrelationId)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());

            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseResponseCompression();
            app.UseMiddleware<ScopedCancellationTokenMiddleware>();
            app.UseMiddleware<ScopedCorrelationIdMiddleware>();
            app.UseMiddleware<KestralResponseBodyMiddleware>();
            app.UseMiddleware<RequestExceptionMiddleware>();
            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
