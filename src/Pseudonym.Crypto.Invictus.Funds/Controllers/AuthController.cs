﻿using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Models;

namespace Pseudonym.Crypto.Invictus.Funds.Controllers
{
    [Authorize]
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
        public IActionResult GetToken()
        {
            var login = authService.Login();

            return Ok(new ApiLogin()
            {
                AccessToken = login.AccessToken,
                ExpiresAt = login.ExpiresAt
            });
        }
    }
}
