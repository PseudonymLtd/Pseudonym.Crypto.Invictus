using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Business;
using Pseudonym.Crypto.Invictus.TrackerService.Configuration;
using Pseudonym.Crypto.Invictus.TrackerService.Hosting;
using Pseudonym.Crypto.Invictus.TrackerService.Hosting.Models;
using Pseudonym.Crypto.Invictus.TrackerService.Services;

namespace Pseudonym.Crypto.Invictus.TrackerService
{
    public sealed class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection container)
        {
            container.Configure<AppSettings>(Configuration.GetSection(nameof(AppSettings)));
            container.Configure<Dependencies>(Configuration.GetSection(nameof(Dependencies)));

            var mvcBuilder = container
                .AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.Converters.Add(new VersionConverter());
                    options.SerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss";
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.Formatting = Formatting.Indented;
                })
                .AddApplicationPart(Assembly.GetExecutingAssembly());

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
            container.AddScoped<StaticFilesMiddleware>();
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
                .AddSwaggerGen(options =>
                {
                    options.SwaggerDoc(
                        "v1",
                        new OpenApiInfo()
                        {
                            Title = "Invictus Funds API",
                            Version = "v1",
                            Contact = new OpenApiContact()
                            {
                                Name = "Pseudonym",
                                Url = new Uri("https://www.pseudonym.org.uk"),
                                Email = "tom.day@pseudonym.org.uk",
                            }
                        });

                    options.EnableAnnotations();
                })
                .AddSwaggerGenNewtonsoftSupport();

            // Clients
            container
                .AddInfuriaClient()
                .AddInvictusClient()
                .AddExchangeRateClient();

            container.AddTransient<IAddressService, AddressService>();
            container.AddTransient<IFundService, FundService>();

            container
                .AddHostedService<CurrencyUpdaterService>()
                .AddSingleton<CurrencyConverter>()
                .AddTransient<ICurrencyConverter>(sp => sp.GetRequiredService<CurrencyConverter>());
        }

        public void Configure(IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseExceptionHandler(new ExceptionHandlerOptions()
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

            applicationBuilder.UseRouting();

            applicationBuilder.UseCors(options =>
                options
                .WithOrigins("https://localhost:5001", "http://localhost:5000")
                .WithExposedHeaders(Headers.CorrelationId)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());

            applicationBuilder.UseAuthentication();
            applicationBuilder.UseAuthorization();
            applicationBuilder.UseHsts();
            applicationBuilder.UseHttpsRedirection();

            applicationBuilder.UseResponseCompression();

            applicationBuilder.UseMiddleware<ScopedCancellationTokenMiddleware>();
            applicationBuilder.UseMiddleware<ScopedCorrelationIdMiddleware>();
            applicationBuilder.UseMiddleware<KestralResponseBodyMiddleware>();
            applicationBuilder.UseMiddleware<StaticFilesMiddleware>();
            applicationBuilder.UseMiddleware<RequestExceptionMiddleware>();

            applicationBuilder.UseSwagger(c => c.RouteTemplate = "{documentName}/schema.json");
            applicationBuilder.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/v1/schema.json", $"Invictus Fund API v1");
                c.RoutePrefix = string.Empty;
                c.InjectStylesheet("/resources/pseudonym-swagger-ui.css");
                c.InjectJavascript("/resources/pseudonym-swagger-ui.js");
                c.DisplayRequestDuration();
                c.DocumentTitle = "Pseudonym | Invictus Fund API";
                c.EnableFilter();
            });

            applicationBuilder.UseStaticFiles();
            applicationBuilder.UseEndpoints(options => options.MapControllers());
        }
    }
}
