using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Shared;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Hosting
{
    internal sealed class ApiKeyHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly AppSettings appSettings;
        private readonly IEnvironmentNameAccessor environmentNameAccessor;

        public ApiKeyHandler(
            IOptions<AppSettings> appSettings,
            IEnvironmentNameAccessor environmentNameAccessor,
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            this.appSettings = appSettings.Value;
            this.environmentNameAccessor = environmentNameAccessor;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.Headers.ContainsKey(Headers.ApiKey) &&
                Request.Headers[Headers.ApiKey] == appSettings.ApiKey)
            {
                var principal = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new List<Claim>()
                        {
                            new Claim("env", environmentNameAccessor.EnvironmentName, null, appSettings.JwtIssuer),
                            new Claim(ClaimsIdentity.DefaultNameClaimType, "Machine", null, appSettings.JwtIssuer),
                            new Claim(ClaimsIdentity.DefaultRoleClaimType, "None", null, appSettings.JwtIssuer),
                            new Claim("iss", appSettings.JwtIssuer, null, appSettings.JwtIssuer),
                            new Claim("aud", appSettings.JwtAudience, null, appSettings.JwtIssuer)
                        },
                        "Negotiate"));

                Context.User = principal;

                return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Headers.ApiKey)));
            }
            else
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }
        }
    }
}
