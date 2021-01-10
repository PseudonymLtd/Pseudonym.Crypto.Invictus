using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Controllers.Filters;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Models;

namespace Pseudonym.Crypto.Invictus.Funds.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/stakes")]
    public class StakeController : AbstractController
    {
        private readonly IStakeService stakeService;
        private readonly IScopedCancellationToken scopedCancellationToken;

        public StakeController(
            IOptions<AppSettings> appSettings,
            IStakeService stakeService,
            IScopedCancellationToken scopedCancellationToken)
            : base(appSettings)
        {
            this.stakeService = stakeService;
            this.scopedCancellationToken = scopedCancellationToken;
        }

        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(List<ApiStake>), StatusCodes.Status200OK)]
        public async IAsyncEnumerable<ApiStake> GetStakes([FromQuery] ApiCurrencyQueryFilter queryFilter)
        {
            await foreach (var stake in stakeService
                .ListStakesAsync(queryFilter.CurrencyCode)
                .WithCancellation(scopedCancellationToken.Token))
            {
                yield return MapStake(stake);
            }
        }

        [HttpGet]
        [Route("{symbol}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ApiStake), StatusCodes.Status200OK)]
        public async Task<ApiStake> GetStakes(
            [Required, FromRoute] Symbol symbol, [FromQuery] ApiCurrencyQueryFilter queryFilter)
        {
            var stake = await stakeService.GetStakeAsync(symbol, queryFilter.CurrencyCode);

            return MapStake(stake);
        }

        [HttpGet]
        [Route("{symbol}/performance")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(List<ApiStakingPower>), StatusCodes.Status200OK)]
        public async IAsyncEnumerable<ApiStakingPower> ListPerformance(
            [Required, FromRoute] Symbol symbol, [FromQuery] ApiPerformanceQueryFilter queryFilter)
        {
            await foreach (var stakingPower in stakeService
                .ListStakePowersAsync(symbol, queryFilter.Mode, queryFilter.FromDate, queryFilter.ToDate, queryFilter.CurrencyCode)
                .WithCancellation(scopedCancellationToken.Token))
            {
                yield return MapStakingPower(stakingPower);
            }
        }
    }
}