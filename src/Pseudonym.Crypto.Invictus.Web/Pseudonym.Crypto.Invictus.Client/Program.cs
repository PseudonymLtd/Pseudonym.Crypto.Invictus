using System;
using System.Globalization;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.Shared;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Web.Client.Abstractions;
using Pseudonym.Crypto.Invictus.Web.Client.Clients;
using Pseudonym.Crypto.Invictus.Web.Client.Configuration;
using Pseudonym.Crypto.Invictus.Web.Client.Hosting;
using Pseudonym.Crypto.Invictus.Web.Client.Services;

namespace Pseudonym.Crypto.Invictus.Web.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentCulture;

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            ConfigureServices(builder.Services, builder.Configuration, builder.HostEnvironment);

            await builder
                .Build()
                .RunAsync();
        }

        public static void ConfigureServices(IServiceCollection container, IConfiguration configuration, IWebAssemblyHostEnvironment webAssemblyHostEnvironment)
        {
            container.Configure<AppSettings>(configuration.GetSection(nameof(AppSettings)));

            container.AddSingleton<IAppState, AppState>();
            container.AddSingleton<ISessionStore, SessionStore>();
            container.AddSingleton(sp => sp
                    .GetRequiredService<ISessionStore>()
                    .Get<UserSettings>(StoreKeys.UserSettings)
                        ?? new UserSettings())
                .AddSingleton<IUserSettings>(sp => sp.GetRequiredService<UserSettings>());

            container.AddSingleton<IUserSettingsHandle, UserSettingsHandle>();
            container.AddSingleton<IEnvironmentNameAccessor, EnvironmentNameAccessor>();

            container
                .AddScoped<IHostClient, HostClient>()
                .AddHttpClient(
                    nameof(HostClient),
                    (sp, client) =>
                    {
                        var settings = sp.GetRequiredService<IOptions<AppSettings>>();

                        client.BaseAddress = new Uri(webAssemblyHostEnvironment.BaseAddress, UriKind.Absolute);
                        client.Timeout = TimeSpan.FromSeconds(10);
                        client.DefaultRequestHeaders.TryAddWithoutValidation(Headers.Origin, webAssemblyHostEnvironment.BaseAddress);
                        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(settings.Value.ServiceName, $"v{settings.Value.Version}"));
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
                        client.DefaultRequestHeaders.TryAddWithoutValidation(Headers.Origin, webAssemblyHostEnvironment.BaseAddress);
                        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(settings.Value.ServiceName, $"v{settings.Value.Version}"));
                    });
        }
    }
}
