using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Business
{
    internal sealed class AuthService : IAuthService
    {
        private readonly AppSettings appSettings;
        private readonly IEnvironmentNameAccessor environmentNameAccessor;

        public AuthService(
            IOptions<AppSettings> appSettings,
            IEnvironmentNameAccessor environmentNameAccessor)
        {
            this.appSettings = appSettings.Value;
            this.environmentNameAccessor = environmentNameAccessor;
        }

        public string GenerateToken()
        {
            var time = DateTime.UtcNow;
            var tokenHandler = new JwtSecurityTokenHandler();

            var payload = new JwtPayload(
                appSettings.JwtIssuer,
                appSettings.JwtAudience,
                new List<Claim>()
                {
                    new Claim("env", environmentNameAccessor.EnvironmentName, null, appSettings.JwtIssuer),
                    new Claim(ClaimsIdentity.DefaultNameClaimType, "Machine", null, appSettings.JwtIssuer),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, "None", null, appSettings.JwtIssuer)
                },
                time,
                time.Add(appSettings.JwtTimeout),
                time);

            var token = new JwtSecurityToken(
                new JwtHeader(
                    new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.ASCII.GetBytes(appSettings.JwtSecret)),
                        SecurityAlgorithms.HmacSha256Signature)),
                payload);

            return tokenHandler.WriteToken(token);
        }
    }
}
