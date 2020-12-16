using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Pseudonym.Crypto.Invictus.Funds.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/v1/health")]
    public class HealthController : Controller
    {
        [HttpGet]
        public IActionResult Ping()
        {
            return NoContent();
        }
    }
}
