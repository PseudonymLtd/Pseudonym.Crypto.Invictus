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
                NetAssetValue = fund.NetValue,
                NetAssetValuePerToken = fund.NetAssetValuePerToken,
                Market = new ApiMarket()
                {
                    IsTradeable = fund.Market.IsTradable,
                    Cap = fund.Market.Cap,
                    Total = fund.Market.Total,
                    PricePerToken = fund.Market.PricePerToken,
                    DiffDaily = fund.Market.DiffDaily,
                    DiffWeekly = fund.Market.DiffWeekly,
                    DiffMonthly = fund.Market.DiffMonthly,
                    Volume = fund.Market.Volume,
                    VolumeDiffDaily = fund.Market.VolumeDiffDaily,
                    VolumeDiffWeekly = fund.Market.VolumeDiffWeekly,
                    VolumeDiffMonthly = fund.Market.VolumeDiffMonthly
                },
                Assets = fund.Assets
                    .Select(a => new ApiAsset()
                    {
                        Name = a.Name,
                        Symbol = a.Symbol ?? "-",
                        Value = a.Value,
                        Share = a.Share,
                        Links = new ApiAssetLinks()
                        {
                            [nameof(ApiAssetLinks.Link)] = a.Link,
                            [nameof(ApiAssetLinks.ImageLink)] = a.ImageLink,
                            [nameof(ApiAssetLinks.MarketLink)] = a.MarketLink,
                        }
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
                TransferAction = GetTransferAction(transactionSet.Sender, transactionSet.Recipient),
                Operations = transactionSet.Operations
                    .Select(o => new ApiOperation()
                    {
                        Address = o.Address?.Address,
                        Sender = o.Sender?.Address,
                        Recipient = o.Recipient?.Address,
                        IsEth = o.IsEth,
                        PricePerToken = o.PricePerToken,
                        Type = o.Type,
                        TransferAction = GetOperationTransferAction(o),
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
                            Links = new ApiAssetLinks()
                            {
                                [nameof(ApiAssetLinks.Link)] = o.ContractLink,
                                [nameof(ApiAssetLinks.ImageLink)] = o.ContractImageLink,
                                [nameof(ApiAssetLinks.MarketLink)] = o.ContractMarketLink,
                            }
                        }
                    })
                    .ToList()
            };

            TransferAction GetOperationTransferAction(IOperation operation)
            {
                return operation.Type == OperationTypes.Transfer
                    ? GetTransferAction(operation.Sender, operation.Recipient)
                    : TransferAction.None;
            }

            TransferAction GetTransferAction(EthereumAddress? sender, EthereumAddress? recipient)
            {
                if (sender.HasValue &&
                    recipient.HasValue &&
                    address.HasValue)
                {
                    if (sender == address)
                    {
                        return TransferAction.Outbound;
                    }
                    else if (recipient == address)
                    {
                        return TransferAction.Inbound;
                    }
                }

                return TransferAction.None;
            }
        }
    }
}
