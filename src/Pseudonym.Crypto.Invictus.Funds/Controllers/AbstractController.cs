using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared;
using Pseudonym.Crypto.Invictus.Shared.Models;

namespace Pseudonym.Crypto.Invictus.Funds.Controllers
{
    public abstract class AbstractController : Controller
    {
        public AbstractController(IOptions<AppSettings> appSettings)
        {
            AppSettings = appSettings.Value;
        }

        protected AppSettings AppSettings { get; }

        protected EthereumAddress GetAddress(string address)
        {
            var ethAddress = new EthereumAddress(address);

            if (HttpContext.Response.Headers.ContainsKey(Headers.Address))
            {
                HttpContext.Response.Headers[Headers.Address] = ethAddress.Address;
            }
            else
            {
                HttpContext.Response.Headers.TryAdd(Headers.Address, ethAddress.Address);
            }

            return ethAddress;
        }

        protected ApiFund MapFund(IFund fund)
        {
            return new ApiFund()
            {
                Name = fund.Name,
                DisplayName = fund.DisplayName,
                Description = fund.Description,
                Token = new ApiToken()
                {
                    Symbol = fund.Token.Symbol,
                    Decimals = fund.Token.Decimals,
                    Address = fund.Token.ContractAddress.Address
                },
                CirculatingSupply = fund.CirculatingSupply,
                Nav = new ApiNav()
                {
                    Value = fund.Nav.Value,
                    ValuePerToken = fund.Nav.ValuePerToken,
                    DiffDaily = fund.Nav.DiffDaily,
                    DiffWeekly = fund.Nav.DiffWeekly,
                    DiffMonthly = fund.Nav.DiffMonthly
                },
                Market = MapMarket(fund.Market),
                Assets = fund.Assets
                    .Select(a => new ApiAsset()
                    {
                        Holding = new ApiHolding()
                        {
                            Name = a.Holding.Name,
                            IsCoin = a.Holding.IsCoin,
                            Symbol = a.Holding.Symbol ?? "-",
                            ContractAddress = a.Holding.ContractAddress?.Address,
                            HexColour = a.Holding.HexColour,
                            FixedValuePerCoin = a.Holding.FixedValuePerCoin,
                            Decimals = a.Holding.Decimals,
                            Links = new ApiHoldingLinks()
                            {
                                [nameof(ApiHoldingLinks.Link)] = a.Holding.Link,
                                [nameof(ApiHoldingLinks.ImageLink)] = a.Holding.ImageLink,
                                [nameof(ApiHoldingLinks.MarketLink)] = a.Holding.MarketLink,
                            }
                        },
                        PricePerToken = a.PricePerToken,
                        Quantity = a.Quantity,
                        Total = a.Total,
                        Share = a.Share
                    })
                    .ToList(),
                Links = new ApiFundLinks()
                {
                    [nameof(ApiFundLinks.Self)] = new Uri(AppSettings.HostUrl.OriginalString.TrimEnd('/') + $"/api/v1/funds/{fund.Token.Symbol}", UriKind.Absolute),
                    [nameof(ApiFundLinks.Lite)] = fund.LitepaperUri,
                    [nameof(ApiFundLinks.Fact)] = fund.FactSheetUri,
                    [nameof(ApiFundLinks.External)] = fund.InvictusUri
                }
            };
        }

        protected ApiTransaction MapTransaction(ITransaction transaction)
        {
            return new ApiTransaction()
            {
                Hash = transaction.Hash,
                ConfirmedAt = transaction.ConfirmedAt,
                Sender = transaction.Sender,
                Recipient = transaction.Recipient,
                Eth = transaction.Eth,
                Success = transaction.Success,
                BlockNumber = transaction.BlockNumber,
                Confirmations = transaction.Confirmations,
                Gas = transaction.Gas,
                GasUsed = transaction.GasUsed,
                GasLimit = transaction.GasLimit
            };
        }

        protected ApiTransactionSet MapTransactionSet(ITransactionSet transactionSet, EthereumAddress? address = null)
        {
            return new ApiTransactionSet()
            {
                Hash = transactionSet.Hash,
                ConfirmedAt = transactionSet.ConfirmedAt,
                Sender = transactionSet.Sender,
                Recipient = transactionSet.Recipient,
                Eth = transactionSet.Eth,
                Success = transactionSet.Success,
                BlockNumber = transactionSet.BlockNumber,
                Confirmations = transactionSet.Confirmations,
                Gas = transactionSet.Gas,
                GasUsed = transactionSet.GasUsed,
                GasLimit = transactionSet.GasLimit,
                Operations = transactionSet.Operations
                    .Select(o => new ApiOperation()
                    {
                        Address = o.Address?.Address,
                        Sender = o.Sender?.Address,
                        Recipient = o.Recipient?.Address,
                        IsEth = o.IsEth,
                        PricePerToken = o.PricePerToken,
                        Type = o.Type,
                        Priority = o.Priority,
                        Value = o.Value,
                        Quantity = o.Quantity,
                        Contract = new ApiContract()
                        {
                            Address = o.ContractAddress.Address,
                            Symbol = o.ContractSymbol,
                            Decimals = o.ContractDecimals,
                            Holders = o.ContractHolders,
                            Issuances = o.ContractIssuances,
                            Name = o.ContractName,
                            Links = new ApiHoldingLinks()
                            {
                                [nameof(ApiHoldingLinks.Link)] = o.ContractLink,
                                [nameof(ApiHoldingLinks.ImageLink)] = o.ContractImageLink,
                                [nameof(ApiHoldingLinks.MarketLink)] = o.ContractMarketLink,
                            }
                        }
                    })
                    .ToList()
            };
        }

        protected ApiStake MapStake(IStake stake)
        {
            return new ApiStake()
            {
                Name = stake.Name,
                DisplayName = stake.DisplayName,
                Description = stake.Description,
                Token = new ApiToken()
                {
                    Symbol = stake.Token.Symbol,
                    Address = stake.Token.ContractAddress,
                    Decimals = stake.Token.Decimals
                },
                CirculatingSupply = stake.CirculatingSupply,
                StakingAddress = stake.StakingAddress,
                Power = MapStakingPower(stake.Power),
                FundMultipliers = stake.FundMultipliers,
                TimeMultipliers = stake.TimeMultipliers
                    .Select(tm => new ApiTimeMultiplier()
                    {
                        RangeMin = tm.RangeMin,
                        RangeMax = tm.RangeMax,
                        Multiplier = tm.Multiplier
                    })
                    .ToList(),
                Market = MapMarket(stake.Market),
                Links = new ApiStakeLinks()
                {
                    [nameof(ApiStakeLinks.Self)] = new Uri(AppSettings.HostUrl.OriginalString.TrimEnd('/') + $"/api/v1/stakes/{stake.Token.Symbol}", UriKind.Absolute),
                    [nameof(ApiStakeLinks.Pool)] = stake.PoolUri,
                    [nameof(ApiStakeLinks.Fact)] = stake.FactSheetUri,
                    [nameof(ApiStakeLinks.External)] = stake.InvictusUri
                }
            };
        }

        protected ApiStakingPower MapStakingPower(IStakingPower stakePower)
        {
            return new ApiStakingPower()
            {
                Date = stakePower.Date,
                Power = stakePower.Power,
                Breakdown = stakePower.Breakdown.Any()
                    ? stakePower.Breakdown
                        .Select(fp => new ApiStakingPowerFund()
                        {
                            Symbol = fp.Symbol,
                            Quantity = fp.Quantity,
                            ModifiedQuantity = fp.ModifiedQuantity,
                            Power = fp.Power
                        })
                        .ToList()
                    : stakePower.Summary
                        .Select(fs => new ApiStakingPowerFund()
                        {
                            Symbol = fs.Symbol,
                            Power = fs.Power
                        })
                        .ToList()
            };
        }

        protected ApiStakeEvent MapStakeEvent(IStakeEvent stake)
        {
            return new ApiStakeEvent()
            {
                Hash = stake.Hash,
                ContractAddress = stake.ContractAddress,
                ConfirmedAt = stake.ConfirmedAt,
                Type = stake.Type,
                Change = stake.Change,
                Lock = stake.Lock != null
                    ? new ApiStakeLock()
                    {
                        Duration = stake.Lock.Duration,
                        ExpiresAt = stake.Lock.ExpiresAt,
                        Quantity = stake.Lock.Quantity
                    }
                    : null,
                Release = stake.Release != null
                    ? new ApiStakeRelease()
                    {
                        Quantity = stake.Release.Quantity,
                        FeeQuantity = stake.Release.FeeQuantity,
                    }
                    : null
            };
        }

        private ApiMarket MapMarket(IMarket market)
        {
            return new ApiMarket()
            {
                IsTradeable = market.IsTradable,
                Cap = market.Cap,
                Total = market.Total,
                PricePerToken = market.PricePerToken,
                DiffDaily = market.DiffDaily,
                DiffWeekly = market.DiffWeekly,
                DiffMonthly = market.DiffMonthly,
                Volume = market.Volume,
                VolumeDiffDaily = market.VolumeDiffDaily,
                VolumeDiffWeekly = market.VolumeDiffWeekly,
                VolumeDiffMonthly = market.VolumeDiffMonthly
            };
        }
    }
}
