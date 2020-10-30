﻿using System.Collections.Generic;
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
    [Route("api/v1/funds")]
    public class FundController : AbstractController
    {
        private readonly IFundService fundService;
        private readonly IScopedCancellationToken scopedCancellationToken;

        public FundController(
            IOptions<AppSettings> appSettings,
            IFundService fundService,
            IScopedCancellationToken scopedCancellationToken)
            : base(appSettings)
        {
            this.fundService = fundService;
            this.scopedCancellationToken = scopedCancellationToken;
        }

        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(List<ApiFund>), StatusCodes.Status200OK)]
        public async IAsyncEnumerable<ApiFund> GetFunds([FromQuery] ApiCurrencyQueryFilter queryFilter)
        {
            await foreach (var fund in fundService
                .ListFundsAsync(queryFilter.CurrencyCode ?? CurrencyCode.USD)
                .WithCancellation(scopedCancellationToken.Token))
            {
                yield return MapFund(fund);
            }
        }

        [HttpGet]
        [Route("{symbol}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ApiFund), StatusCodes.Status200OK)]
        public async Task<ApiFund> GetFund([Required, FromRoute] Symbol symbol, [FromQuery] ApiCurrencyQueryFilter queryFilter)
        {
            var fund = await fundService.GetFundAsync(symbol, queryFilter.CurrencyCode ?? CurrencyCode.USD);

            return MapFund(fund);
        }

        [HttpGet]
        [Route("{symbol}/performance")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ApiFund), StatusCodes.Status200OK)]
        public async IAsyncEnumerable<ApiPerformance> ListPerformance([Required, FromRoute] Symbol symbol, [FromQuery] ApiPerformanceQueryFilter queryFilter)
        {
            await foreach (var perf in fundService
                .ListPerformanceAsync(symbol, queryFilter.FromDate, queryFilter.ToDate, queryFilter.CurrencyCode ?? CurrencyCode.USD)
                .WithCancellation(scopedCancellationToken.Token))
            {
                yield return new ApiPerformance()
                {
                    Date = perf.Date,
                    NetValue = perf.NetValue,
                    NetAssetValuePerToken = perf.NetAssetValuePerToken
                };
            }
        }
    }
}
