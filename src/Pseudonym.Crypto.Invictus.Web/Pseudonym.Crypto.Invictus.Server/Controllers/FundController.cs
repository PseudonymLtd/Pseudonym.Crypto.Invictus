using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Models;
using Pseudonym.Crypto.Invictus.Web.Server.Abstractions;

namespace Pseudonym.Crypto.Invictus.Web.Server.Controllers
{
    [ApiController]
    [Route("api/funds")]
    public class FundController : Controller
    {
        private readonly IApiClient apiClient;
        private readonly IScopedCancellationToken scopedCancellationToken;

        public FundController(
            IApiClient apiClient,
            IScopedCancellationToken scopedCancellationToken)
        {
            this.apiClient = apiClient;
            this.scopedCancellationToken = scopedCancellationToken;
        }

        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(List<ApiFund>), StatusCodes.Status200OK)]
        public async IAsyncEnumerable<ApiFund> GetFunds()
        {
            await foreach (var fund in apiClient.ListFundsAsync().WithCancellation(scopedCancellationToken.Token))
            {
                yield return fund;
            }
        }

        [HttpGet]
        [Route("{symbol}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ApiFund), StatusCodes.Status200OK)]
        public async Task<ApiFund> GetFund([Required, FromRoute] Symbol symbol)
        {
            return await apiClient.GetFundAsync(symbol);
        }

        [HttpGet]
        [Route("{symbol}/performance")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ApiFund), StatusCodes.Status200OK)]
        public async IAsyncEnumerable<ApiPerformance> ListPerformance([Required, FromRoute] Symbol symbol)
        {
            await foreach (var perf in apiClient
                .ListFundPerformanceAsync(symbol, DateTime.UtcNow.AddYears(-1), DateTime.UtcNow)
                .WithCancellation(scopedCancellationToken.Token))
            {
                yield return perf;
            }
        }
    }
}
