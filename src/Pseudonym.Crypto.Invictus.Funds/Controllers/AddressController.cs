using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Controllers.Models;
using Pseudonym.Crypto.Invictus.Funds.Controllers.Models.Filters;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Controllers
{
    [ApiKey]
    [ApiController]
    [Route("api/v1/address")]
    public class AddressController : Controller
    {
        private readonly IOptions<AppSettings> appSettings;
        private readonly IAddressService addressService;
        private readonly IScopedCancellationToken scopedCancellationToken;

        public AddressController(
            IOptions<AppSettings> appSettings,
            IAddressService addressService,
            IScopedCancellationToken scopedCancellationToken)
        {
            this.appSettings = appSettings;
            this.addressService = addressService;
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

            await foreach (var investment in addressService
                .ListInvestmentsAsync(address, currencyCode)
                .WithCancellation(scopedCancellationToken.Token))
            {
                portfolio.Investments.Add(new ApiInvestment()
                {
                    Name = investment.Fund.Name,
                    Held = investment.Held,
                    Share = investment.Share,
                    RealValue = investment.RealValue,
                    MarketValue = investment.MarketValue
                });
            }

            return Ok(portfolio);
        }

        [HttpGet]
        [Route("{hex}/transactions/{symbol}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(List<ApiTransaction>), StatusCodes.Status200OK)]
        public async IAsyncEnumerable<ApiTransaction> GetTransactions(
            [Required, FromRoute] string hex, [Required, FromRoute] Symbol symbol, [FromQuery] ApiCurrencyQueryFilter queryFilter)
        {
            var address = new EthereumAddress(hex);
            var currencyCode = queryFilter.CurrencyCode ?? CurrencyCode.USD;

            var fundInfo = appSettings.Value.Funds.Single(x => x.Symbol == symbol);

            await foreach (var transaction in addressService
                .ListTransactionsAsync(new EthereumAddress(fundInfo.ContractAddress), address, currencyCode)
                .WithCancellation(scopedCancellationToken.Token))
            {
                yield return new ApiTransaction()
                {
                    Sender = transaction.Sender,
                    Recipient = transaction.Recipient,
                    Amount = transaction.Amount
                };
            }
        }
    }
}