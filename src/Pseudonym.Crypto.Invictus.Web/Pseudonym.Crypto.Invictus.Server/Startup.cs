using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Pseudonym.Crypto.Invictus.Shared;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Hosting;
using Pseudonym.Crypto.Invictus.Shared.Hosting.Models;
using Pseudonym.Crypto.Invictus.Web.Server.Abstractions;
using Pseudonym.Crypto.Invictus.Web.Server.Business;
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

            container.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.Converters.Add(new VersionConverter());
                    options.SerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss";
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.Formatting = Formatting.Indented;
                })
                .AddApplicationPart(Assembly.GetExecutingAssembly())
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var problems = new FailureDetails(context.HttpContext.TraceIdentifier);

                        return new BadRequestObjectResult(problems);
                    };
                });

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

            container.AddScoped<IAuthService, AuthService>();
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
