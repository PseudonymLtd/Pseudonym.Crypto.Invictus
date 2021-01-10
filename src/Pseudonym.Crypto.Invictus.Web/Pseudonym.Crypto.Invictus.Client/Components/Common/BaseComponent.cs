using System;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Models;
using Pseudonym.Crypto.Invictus.Web.Client.Abstractions;
using Pseudonym.Crypto.Invictus.Web.Client.Business;

namespace Pseudonym.Crypto.Invictus.Web.Client.Components.Common
{
    public class BaseComponent : ComponentBase
    {
        protected BusinessTransaction Map(IUserSettings userSettings, ApiFund fund, ApiTransactionSet transaction, ApiPerformance performance)
        {
            var businessTransaction = new BusinessTransaction()
            {
                Hash = transaction.Hash,
                ContractAddress = fund.Token.Address,
                BlockNumber = transaction.BlockNumber,
                Confirmations = transaction.Confirmations,
                ConfirmedAt = transaction.ConfirmedAt,
                Success = transaction.Success,
                Eth = transaction.Eth,
                Gas = transaction.Gas,
                GasLimit = transaction.GasLimit,
                GasUsed = transaction.GasUsed,
                Sender = transaction.Sender,
                Recipient = transaction.Recipient,
                Price = performance != null
                    ? new BusinessPrice()
                    {
                        MarketValuePerToken = performance.MarketValuePerToken,
                        NetAssetValuePerToken = performance.NetAssetValuePerToken
                    }
                    : null,
                Operations = transaction.Operations
                    .Select(operation => new BusinessOperation()
                    {
                        IsEth = operation.IsEth,
                        Sender = operation.Sender,
                        Address = operation.Address,
                        PricePerToken =
                            performance != null &&
                            operation.PricePerToken == default &&
                            operation.Contract.Symbol.Equals(fund.Token.Symbol.ToString(), StringComparison.OrdinalIgnoreCase)
                                ? performance.MarketValuePerToken ?? performance.NetAssetValuePerToken
                                : operation.PricePerToken,
                        Priority = operation.Priority,
                        Quantity = operation.Quantity,
                        Type = operation.Type,
                        Value = operation.Value,
                        Recipient = operation.Recipient,
                        TransferAction = GetOperationTransferAction(operation),
                        Contract = new BusinessContract()
                        {
                            Symbol = operation.Contract.Symbol,
                            Address = operation.Contract.Address,
                            Decimals = operation.Contract.Decimals,
                            Holders = operation.Contract.Holders,
                            Issuances = operation.Contract.Issuances,
                            Name = operation.Contract.Name,
                            Link = operation.Contract.Links.Link,
                            ImageLink = operation.Contract.Links.ImageLink,
                            MarketLink = operation.Contract.Links.MarketLink
                        }
                    })
                    .ToList()
            };

            return businessTransaction;

            TransferAction GetOperationTransferAction(ApiOperation operation)
            {
                if (operation.Type == OperationTypes.Transfer &&
                    !string.IsNullOrWhiteSpace(operation.Sender) &&
                    !string.IsNullOrWhiteSpace(operation.Recipient) &&
                    userSettings.HasValidAddress())
                {
                    if (operation.Sender.Equals(userSettings.WalletAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        return TransferAction.Outbound;
                    }
                    else if (operation.Recipient.Equals(userSettings.WalletAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        return TransferAction.Inbound;
                    }
                }

                return TransferAction.None;
            }
        }
    }
}
