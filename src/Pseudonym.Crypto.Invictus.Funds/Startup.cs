using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Amazon;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business;
using Pseudonym.Crypto.Invictus.Funds.Business.Json;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Data;
using Pseudonym.Crypto.Invictus.Funds.Hosting;
using Pseudonym.Crypto.Invictus.Funds.Services;
using Pseudonym.Crypto.Invictus.Shared;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Hosting;
using Pseudonym.Crypto.Invictus.Shared.Hosting.Models;

namespace Pseudonym.Crypto.Invictus.Funds
{
    public sealed class Startup
    {
        private const string PolicyName = "Mixed";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection container)
        {
            container.Configure<AwsSettings>(Configuration.GetSection("Aws"));
            container.Configure<AppSettings>(Configuration.GetSection(nameof(AppSettings)));
            container.Configure<Dependencies>(Configuration.GetSection(nameof(Dependencies)));

            var mvcBuilder = container
                .AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.Converters.Add(new VersionConverter());
                    options.SerializerSettings.Converters.Add(new DateTimeOffsetConvertor());
                    options.SerializerSettings.Converters.Add(new DateTimeOffsetNullableConvertor());
                    options.SerializerSettings.Converters.Add(new TimeSpanConverter());
                    options.SerializerSettings.Converters.Add(new TimeSpanNullableConvertor());
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
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
            container.AddScoped<StaticFilesMiddleware>();
            container.AddScoped<ResponseHeadersMiddleware>();
            container.AddScoped<RequestExceptionMiddleware>();

            container.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });

            container.AddHttpContextAccessor();

            var appSettings = Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();
            var jwtKey = Encoding.ASCII.GetBytes(appSettings.JwtSecret);

            container
                .AddAuthentication(auth =>
                {
                    auth.DefaultScheme = PolicyName;
                    auth.DefaultChallengeScheme = PolicyName;
                })
                .AddPolicyScheme(PolicyName, "API-Key or Authorization Bearer", options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        if (context.Request.Headers.ContainsKey(Headers.ApiKey))
                        {
                            return Headers.ApiKey;
                        }
                        else
                        {
                            return JwtBearerDefaults.AuthenticationScheme;
                        }
                    };
                })
                .AddScheme<AuthenticationSchemeOptions, ApiKeyHandler>(Headers.ApiKey, options => options.ClaimsIssuer = appSettings.JwtIssuer)
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
                        ValidateIssuer = true,
                        ValidIssuer = appSettings.JwtIssuer,
                        ValidAudience = appSettings.JwtAudience,
                        ValidateAudience = true,
                        ValidateLifetime = true
                    };
                });

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
                    options.AddSecurityDefinition(Headers.ApiKey, new OpenApiSecurityScheme
                    {
                        Description = "Api Key used to interact with the system.",
                        Name = Headers.ApiKey,
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey
                    });
                    options.AddSecurityDefinition(JwtConstants.TokenType, new OpenApiSecurityScheme
                    {
                        Description = "JWT token used to interact with the system.",
                        Name = Headers.Authorization,
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = JwtBearerDefaults.AuthenticationScheme.ToLower(),
                        BearerFormat = JwtConstants.TokenType
                    });
                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Id = JwtConstants.TokenType,
                                    Type = ReferenceType.SecurityScheme,
                                }
                            },
                            new List<string>()
                        },
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Id = Headers.ApiKey,
                                    Type = ReferenceType.SecurityScheme,
                                }
                            },
                            new List<string>()
                        }
                    });
                })
                .AddSwaggerGenNewtonsoftSupport();

            // Clients
            container
                .AddBloxyClient()
                .AddCoinGeckoClient()
                .AddEthplorerClient()
                .AddInfuraClient()
                .AddLightstreamClient()
                .AddInvictusClient()
                .AddExchangeRateClient();

            container.AddTransient<IAuthService, AuthService>();
            container.AddTransient<IStakeService, StakeService>();
            container.AddTransient<IInvestmentService, InvestmentService>();
            container.AddTransient<IFundService, FundService>();

            container.AddScoped<ITransactionRepository, TransactionRepository>();
            container.AddScoped<IOperationRepository, OperationRepository>();
            container.AddScoped<IFundPerformanceRepository, FundPerformanceRepository>();

            container.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            container.AddScoped<IAmazonDynamoDB>(sp =>
            {
                var awsSettings = sp.GetRequiredService<IOptions<AwsSettings>>();
                if (awsSettings.Value.UseConfiguredSecurity())
                {
                    return new AmazonDynamoDBClient(
                        awsSettings.Value.AccessKey,
                        awsSettings.Value.SecretAccessKey,
                        RegionEndpoint.GetBySystemName(awsSettings.Value.Region));
                }
                else
                {
                    return new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(awsSettings.Value.Region));
                }
            });

            container
                .AddHostedService<CurrencyUpdaterService>()
                .AddSingleton<CurrencyConverter>()
                .AddTransient<ICurrencyConverter>(sp => sp.GetRequiredService<CurrencyConverter>());

            if (appSettings.CachingEnabled)
            {
                container.AddHostedService<TransactionCachingService>();
                container.AddHostedService<InvictusFundMarketCachingService>();
            }
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
                .WithOrigins(Configuration
                    .GetSection("Cors")
                    .GetChildren()
                    .Select(x => x.Value)
                    .ToArray())
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
            applicationBuilder.UseMiddleware<ResponseHeadersMiddleware>();
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
