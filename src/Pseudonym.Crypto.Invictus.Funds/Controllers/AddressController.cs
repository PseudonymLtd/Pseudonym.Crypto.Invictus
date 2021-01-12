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
using Pseudonym.Crypto.Invictus.Funds.Controllers.Filters;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Models;

namespace Pseudonym.Crypto.Invictus.Funds.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/addresses")]
    public class AddressController : AbstractController
    {
        private readonly IFundService fundService;
        private readonly IStakeService stakeService;
        private readonly IInvestmentService investmentService;
        private readonly IScopedCancellationToken scopedCancellationToken;

        public AddressController(
            IOptions<AppSettings> appSettings,
            IFundService fundService,
            IStakeService stakeService,
            IInvestmentService investmentService,
            IScopedCancellationToken scopedCancellationToken)
            : base(appSettings)
        {
            this.fundService = fundService;
            this.stakeService = stakeService;
            this.investmentService = investmentService;
            this.scopedCancellationToken = scopedCancellationToken;
        }

        [HttpGet]
        [Route("{address}/investments")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(List<ApiInvestment>), StatusCodes.Status200OK)]
        public async IAsyncEnumerable<ApiInvestment> ListInvestments(
            [Required, FromRoute, EthereumAddress] string address, [FromQuery] ApiCurrencyQueryFilter queryFilter)
        {
            await foreach (var investment in investmentService
                .ListInvestmentsAsync(GetAddress(address), queryFilter.CurrencyCode)
                .WithCancellation(scopedCancellationToken.Token))
            {
                yield return new ApiInvestment()
                {
                    Fund = MapFund(investment.Fund),
                    Held = investment.Held,
                    Legacy = investment.Legacy,
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
            [Required, FromRoute, EthereumAddress] string address, [Required, FromRoute] Symbol symbol, [FromQuery] ApiCurrencyQueryFilter queryFilter)
        {
            var investment = await investmentService.GetInvestmentAsync(GetAddress(address), symbol, queryFilter.CurrencyCode);

            return new ApiInvestment()
            {
                Fund = MapFund(investment.Fund),
                Held = investment.Held,
                Legacy = investment.Legacy,
                Share = investment.Share,
                RealValue = investment.RealValue,
                MarketValue = investment.MarketValue,
                Stakes = investment.Stakes.Select(MapStakeEvent).ToList(),
                SubInvestments = investment.SubInvestments
                    .Select(i => new ApiSubInvestment()
                    {
                        Held = i.Held,
                        MarketValue = i.MarketValue,
                        Coin = new ApiHolding()
                        {
                            Name = i.Holding.Name,
                            Symbol = i.Holding.Symbol ?? "-",
                            ContractAddress = i.Holding.ContractAddress?.Address,
                            FixedValuePerCoin = i.Holding.FixedValuePerCoin,
                            HexColour = i.Holding.HexColour,
                            Decimals = i.Holding.Decimals,
                            Links = new ApiHoldingLinks()
                            {
                                [nameof(ApiHoldingLinks.Link)] = i.Holding.Link,
                                [nameof(ApiHoldingLinks.ImageLink)] = i.Holding.ImageLink,
                                [nameof(ApiHoldingLinks.MarketLink)] = i.Holding.MarketLink,
                            }
                        }
                    })
                    .ToList()
            };
        }

        [HttpGet]
        [Route("{address}/investments/{symbol}/transactions")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(List<ApiTransactionSet>), StatusCodes.Status200OK)]
        public async IAsyncEnumerable<ApiTransactionSet> GetTransactions(
            [Required, FromRoute, EthereumAddress] string address, [Required, FromRoute] Symbol symbol, [FromQuery] ApiCurrencyQueryFilter queryFilter)
        {
            var addr = GetAddress(address);

            var transactions = await investmentService
                .ListTransactionsAsync(addr, symbol, queryFilter.CurrencyCode)
                .ToListAsync(scopedCancellationToken.Token);

            foreach (var transaction in transactions.OrderByDescending(x => x.ConfirmedAt))
            {
                yield return MapTransactionSet(transaction, addr);
            }
        }

        [HttpGet]
        [Route("{address}/investments/{symbol}/transactions/{hash}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(List<ApiTransactionSet>), StatusCodes.Status200OK)]
        public async Task<ApiTransactionSet> GetTransaction(
            [Required, FromRoute, EthereumAddress] string address,
            [Required, FromRoute] Symbol symbol,
            [Required, FromRoute, TransactionHash] string hash,
            [FromQuery] ApiCurrencyQueryFilter queryFilter)
        {
            var addr = GetAddress(address);

            var transaction = await fundService.GetTransactionAsync(symbol, new EthereumTransactionHash(hash), queryFilter.CurrencyCode);

            return MapTransactionSet(transaction, addr);
        }

        [HttpGet]
        [Route("{address}/stakes/{symbol}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(List<ApiStakeEvent>), StatusCodes.Status200OK)]
        public async IAsyncEnumerable<ApiStakeEvent> ListStakes(
            [Required, FromRoute, EthereumAddress] string address,
            [Required, FromRoute] Symbol symbol)
        {
            await foreach (var stake in stakeService
                .ListStakeEventsAsync(symbol, GetAddress(address))
                .WithCancellation(scopedCancellationToken.Token))
            {
                yield return MapStakeEvent(stake);
            }
        }

        [HttpGet]
        [Route("{address}/stakes/{symbol}/funds/{fundSymbol}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(List<ApiStakeEvent>), StatusCodes.Status200OK)]
        public async IAsyncEnumerable<ApiStakeEvent> GetStake(
            [Required, FromRoute, EthereumAddress] string address,
            [Required, FromRoute] Symbol symbol,
            [Required, FromRoute] Symbol fundSymbol)
        {
            await foreach (var stake in stakeService
                .ListStakeEventsAsync(symbol, GetAddress(address), fundSymbol)
                .WithCancellation(scopedCancellationToken.Token))
            {
                yield return MapStakeEvent(stake);
            }
        }

        [HttpGet]
        [Route("{address}/stakes/{symbol}/funds/{fundSymbol}/events/{hash}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ApiStakeEvent), StatusCodes.Status200OK)]
        public async Task<ApiStakeEvent> GetStake(
            [Required, FromRoute, EthereumAddress] string address,
            [Required, FromRoute] Symbol symbol,
            [Required, FromRoute] Symbol fundSymbol,
            [Required, FromRoute, TransactionHash] string hash)
        {
            var txHash = new EthereumTransactionHash(hash);
            var stake = await stakeService.GetStakeEventAsync(
                symbol,
                GetAddress(address),
                fundSymbol,
                txHash);

            return MapStakeEvent(stake);
        }
    }
}