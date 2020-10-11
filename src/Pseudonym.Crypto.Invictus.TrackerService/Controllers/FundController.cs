using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Business.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Controllers.Models;
using Pseudonym.Crypto.Invictus.TrackerService.Controllers.Models.Filters;

namespace Pseudonym.Crypto.Invictus.TrackerService.Controllers
{
    [Route("funds")]
    [ApiController]
    [AllowAnonymous]
    public class FundController
    {
        private readonly IFundService fundService;
        private readonly IScopedCancellationToken scopedCancellationToken;

        public FundController(
            IFundService fundService,
            IScopedCancellationToken scopedCancellationToken)
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
                yield return Map(fund);
            }
        }

        [HttpGet]
        [Route("{symbol}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ApiFund), StatusCodes.Status200OK)]
        public async Task<ApiFund> GetFund([Required, FromRoute] Symbol symbol, [FromQuery] ApiCurrencyQueryFilter queryFilter)
        {
            var fund = await fundService.GetFundAsync(symbol, queryFilter.CurrencyCode ?? CurrencyCode.USD);

            return Map(fund);
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

        private ApiFund Map(IFund fund)
        {
            return new ApiFund()
            {
                Name = fund.Name,
                Token = new ApiToken()
                {
                    Symbol = fund.Token.Symbol,
                    Decimals = fund.Token.Decimals,
                    Address = fund.Token.ContractAddress.Address
                },
                IsTradeable = fund.IsTradeable,
                CirculatingSupply = fund.CirculatingSupply,
                NetAssetValue = fund.NetValue,
                NetAssetValuePerToken = fund.NetAssetValuePerToken,
                MarketValue = fund.MarketValue,
                MarketValuePerToken = fund.MarketValuePerToken,
                Assets = fund.Assets
                    .Select(a => new ApiAsset()
                    {
                        Name = a.Name,
                        Symbol = a.Symbol ?? "-",
                        Value = a.Value,
                        Share = a.Share
                    })
                    .ToList()
            };
        }
    }
}
