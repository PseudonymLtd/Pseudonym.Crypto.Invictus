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
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Models;
using Pseudonym.Crypto.Invictus.Shared.Models.Filters;

namespace Pseudonym.Crypto.Invictus.Funds.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/addresses")]
    public class AddressController : AbstractController
    {
        private readonly IAddressService addressService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IScopedCancellationToken scopedCancellationToken;

        public AddressController(
            IOptions<AppSettings> appSettings,
            IAddressService addressService,
            IHttpContextAccessor httpContextAccessor,
            IScopedCancellationToken scopedCancellationToken)
            : base(appSettings)
        {
            this.addressService = addressService;
            this.httpContextAccessor = httpContextAccessor;
            this.scopedCancellationToken = scopedCancellationToken;
        }

        [HttpGet]
        [Route("{address}/investments")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(List<ApiInvestment>), StatusCodes.Status200OK)]
        public async IAsyncEnumerable<ApiInvestment> ListInvestments([Required, FromRoute] string address, [FromQuery] ApiCurrencyQueryFilter queryFilter)
        {
            await foreach (var investment in addressService
                .ListInvestmentsAsync(GetAddress(address), queryFilter.CurrencyCode)
                .WithCancellation(scopedCancellationToken.Token))
            {
                yield return new ApiInvestment()
                {
                    Fund = MapFund(investment.Fund),
                    Held = investment.Held,
                    Share = investment.Share,
                    RealValue = investment.RealValue,
                    MarketValue = investment.MarketValue
                };
            }
        }

        [HttpGet]
        [Route("{address}/investments/{symbol}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ApiInvestment), StatusCodes.Status200OK)]
        public async Task<ApiInvestment> GetInvestment(
            [Required, FromRoute] string address, [Required, FromRoute] Symbol symbol, [FromQuery] ApiCurrencyQueryFilter queryFilter)
        {
            var fundInfo = AppSettings.Funds.Single(x => x.Symbol == symbol);

            var investment = await addressService.GetInvestmentAsync(GetAddress(address), symbol, queryFilter.CurrencyCode);

            return new ApiInvestment()
            {
                Fund = MapFund(investment.Fund),
                Held = investment.Held,
                Share = investment.Share,
                RealValue = investment.RealValue,
                MarketValue = investment.MarketValue
            };
        }

        [HttpGet]
        [Route("{address}/investments/{symbol}/transactions")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(List<ApiTransaction>), StatusCodes.Status200OK)]
        public async IAsyncEnumerable<ApiTransaction> GetTransactions(
            [Required, FromRoute] string address, [Required, FromRoute] Symbol symbol, [FromQuery] ApiCurrencyQueryFilter queryFilter)
        {
            var fundInfo = AppSettings.Funds.Single(x => x.Symbol == symbol);

            await foreach (var transaction in addressService
                .ListTransactionsAsync(new EthereumAddress(fundInfo.ContractAddress), GetAddress(address), queryFilter.CurrencyCode)
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

        private EthereumAddress GetAddress(string address)
        {
            var ethAddress = new EthereumAddress(address);

            httpContextAccessor.HttpContext.Response.Headers.TryAdd(Headers.Address, ethAddress.Address);

            return ethAddress;
        }
    }
}