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
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Controllers.Filters;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
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
        [ProducesResponseType(typeof(ApiFund), StatusCodes.Status200OK)]
        public async IAsyncEnumerable<ApiStake> GetStakesByFund([Required, FromRoute] Symbol symbol, [FromQuery] ApiCurrencyQueryFilter queryFilter)
        {
            await foreach (var stake in stakeService
                .ListStakesAsync(symbol, queryFilter.CurrencyCode)
                .WithCancellation(scopedCancellationToken.Token))
            {
                yield return MapStake(stake);
            }
        }

        [HttpGet]
        [Route("{symbol}/{hash}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ApiTransactionSet), StatusCodes.Status200OK)]
        public async Task<ApiStake> GetTransaction(
            [Required, FromRoute] Symbol symbol, [Required, FromRoute, TransactionHash] string hash, [FromQuery] ApiCurrencyQueryFilter queryFilter)
        {
            var stake = await stakeService.GetStakeAsync(symbol, new EthereumTransactionHash(hash), queryFilter.CurrencyCode);

            return MapStake(stake);
        }

        private ApiStake MapStake(IStake stake)
        {
            return new ApiStake()
            {
                Hash = stake.Hash,
                ContractAddress = stake.ContractAddress,
                StakedAt = stake.StakedAt,
                Duration = stake.Duration,
                ExpiresAt = stake.ExpiresAt,
                PricePerToken = stake.PricePerToken != default
                    ? stake.PricePerToken
                    : default(decimal?),
                Quantity = stake.Quantity,
                Total = stake.PricePerToken != default
                    ? stake.PricePerToken * stake.Quantity
                    : default(decimal?)
            };
        }
    }
}