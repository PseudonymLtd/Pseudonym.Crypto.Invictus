using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Business.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Controllers.Models;
using Pseudonym.Crypto.Invictus.TrackerService.Models;

namespace Pseudonym.Crypto.Invictus.TrackerService.Controllers
{
    [Route("address")]
    [ApiController]
    [AllowAnonymous]
    public class AddressController : Controller
    {
        private readonly ICurrencyConverter currencyConverter;
        private readonly IFundService fundService;
        private readonly IEtherscanClient etherscanClient;
        private readonly IScopedCancellationToken scopedCancellationToken;

        public AddressController(
            ICurrencyConverter currencyConverter,
            IFundService fundService,
            IEtherscanClient etherscanClient,
            IScopedCancellationToken scopedCancellationToken)
        {
            this.currencyConverter = currencyConverter;
            this.fundService = fundService;
            this.etherscanClient = etherscanClient;
            this.scopedCancellationToken = scopedCancellationToken;
        }

        [HttpGet]
        [Route("{hex}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ApiPortfolio), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAddress([Required, FromRoute] string hex, [FromQuery] ApiCurrencyQueryFilter queryFilter)
        {
            var address = new EthereumAddress(hex);

            var portfolio = new ApiPortfolio()
            {
                Address = address,
                Currency = CurrencyCode.USD
            };

            await foreach (var fund in fundService.ListFundsAsync(scopedCancellationToken.Token))
            {
                var tokenCount = await etherscanClient.GetTokensForAddressAsync(
                    fund.Token,
                    address,
                    scopedCancellationToken.Token);

                portfolio.Investments.Add(new ApiInvestment()
                {
                    Name = string.Join(" ", fund.Name
                        .Trim()
                        .Split('-')
                        .Select(x =>
                        {
                            var chars = x.ToCharArray();
                            chars[0] = char.ToUpperInvariant(chars[0]);
                            return new string(chars);
                        })),
                    Held = tokenCount,
                    Share = tokenCount / fund.CirculatingSupply * 100,
                    MarketValue = currencyConverter.Convert(
                        (fund.MarketValuePerToken ?? fund.NetAssetValuePerToken) * tokenCount,
                        queryFilter.CurrencyCode ?? CurrencyCode.USD),
                    RealValue = currencyConverter.Convert(
                        fund.NetAssetValuePerToken * tokenCount,
                        queryFilter.CurrencyCode ?? CurrencyCode.USD)
                });
            }

            return Ok(portfolio);
        }
    }
}