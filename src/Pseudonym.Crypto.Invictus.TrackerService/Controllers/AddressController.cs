using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Business.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Controllers.Models;
using Pseudonym.Crypto.Invictus.TrackerService.Controllers.Models.Filters;
using Pseudonym.Crypto.Invictus.TrackerService.Ethereum;

namespace Pseudonym.Crypto.Invictus.TrackerService.Controllers
{
    [Route("address")]
    [ApiController]
    [AllowAnonymous]
    public class AddressController : Controller
    {
        private readonly IFundService fundService;
        private readonly IEtherClientFactory etherClientFactory;
        private readonly IScopedCancellationToken scopedCancellationToken;

        public AddressController(
            IFundService fundService,
            IEtherClientFactory etherClientFactory,
            IScopedCancellationToken scopedCancellationToken)
        {
            this.fundService = fundService;
            this.etherClientFactory = etherClientFactory;
            this.scopedCancellationToken = scopedCancellationToken;
        }

        [HttpGet]
        [Route("{hex}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ApiPortfolio), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAddress([Required, FromRoute] string hex, [FromQuery] ApiCurrencyQueryFilter queryFilter)
        {
            var address = new EthereumAddress(hex);
            var currencyCode = queryFilter.CurrencyCode ?? CurrencyCode.USD;

            var portfolio = new ApiPortfolio()
            {
                Address = address,
                Currency = currencyCode
            };

            using var etherClient = etherClientFactory.CreateClient();

            await foreach (var fund in fundService.ListFundsAsync(currencyCode, scopedCancellationToken.Token))
            {
                var tokenCount = await etherClient.GetContractBalance(fund.Token.ContractAddress, address); ;

                portfolio.Investments.Add(new ApiInvestment()
                {
                    Name = fund.Name,
                    Held = tokenCount,
                    Share = tokenCount / fund.CirculatingSupply * 100,
                    RealValue = fund.NetAssetValuePerToken * tokenCount,
                    MarketValue = fund.MarketValuePerToken.HasValue
                        ? fund.MarketValuePerToken.Value * tokenCount
                        : default(decimal?)
                });
            }

            return Ok(portfolio);
        }
    }
}