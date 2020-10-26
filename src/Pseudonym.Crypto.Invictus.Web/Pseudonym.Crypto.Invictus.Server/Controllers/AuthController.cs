using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pseudonym.Crypto.Invictus.Shared.Models;
using Pseudonym.Crypto.Invictus.Web.Server.Abstractions;

namespace Pseudonym.Crypto.Invictus.Web.Server.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : Controller
    {
        private readonly IAuthService authService;

        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }

        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ApiLogin), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetToken()
        {
            var loginResponse = await authService.LoginAsync();

            return Ok(loginResponse);
        }
    }
}
